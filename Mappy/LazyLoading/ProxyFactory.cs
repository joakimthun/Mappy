using Mappy.Configuration;
using Mappy.Extensions;
using Mappy.Helpers;
using Mappy.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Mappy.LazyLoading
{
    internal class ProxyFactory
    {
        private const string DynamicAssemblyName = "Mappy_Dynamic";
        private const string DynamicModuleName = "Proxies";

        private readonly MappyConfiguration _configuration;
        private readonly QueryFactory _queryFactory;
        private readonly Dictionary<Type, Type> _typeCache;

        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
        private Random _random;

        private bool _saved = false;

        public ProxyFactory(MappyConfiguration configuration)
        {
            _configuration = configuration;
            _queryFactory = new QueryFactory();
            _typeCache = new Dictionary<Type, Type>();
        }

        private AssemblyBuilder AssemblyBuilder
        {
            get
            {
                if (_assemblyBuilder == null)
                {
                    var assemblyName = new AssemblyName(DynamicAssemblyName);
                    _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                }

                return _assemblyBuilder;
            }
        }

        private ModuleBuilder ModuleBuilder
        {
            get
            {
                if (_moduleBuilder == null)
                {
                    #if DEBUG
                    _moduleBuilder = AssemblyBuilder.DefineDynamicModule(DynamicModuleName, "mod_test.dll");
                    #else
                    _moduleBuilder = AssemblyBuilder.DefineDynamicModule(DynamicModuleName);
                    #endif
                }

                return _moduleBuilder;
            }
        }

        private Random Random
        {
            get
            {
                if (_random == null)
                {
                    _random = new Random();
                }

                return _random;
            }
        }

        public Type CreateProxy(Type baseType)
        {
            var publicVirtualInstanceProperties = baseType.GetPublicVirtualInstanceProperties();
            var subType = CreateSubType(baseType);

            #if DEBUG
            if (!_saved && _typeCache.Keys.Count == 3)
            {
                AssemblyBuilder.Save("mappy_test.dll");
                _saved = true;
            }
            #endif

            return subType;
        }

        private Type CreateSubType(Type baseType)
        {
            if (!_typeCache.ContainsKey(baseType))
            {
                var proxy = ModuleBuilder.DefineType(GetProxyName(baseType), baseType.Attributes, baseType);
                proxy.AddInterfaceImplementation(typeof(IMappyProxy));

                foreach (var property in baseType.GetPublicVirtualInstanceProperties())
                {
                    OverrideVirtualProperty(proxy, property);
                }

                _typeCache.Add(baseType, proxy.CreateType());
            }

            return _typeCache[baseType];
        }

        private void OverrideVirtualProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            var property = DefineProperty(typeBuilder, propertyInfo);
            var field = DefineField(typeBuilder, propertyInfo);

            var getter = DefineGetter(typeBuilder, propertyInfo, field);
            var setter = DefineSetter(typeBuilder, propertyInfo, field);

            property.SetGetMethod(getter);
            property.SetSetMethod(setter);
        }

        private MethodBuilder DefineGetter(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldBuilder field)
        {
            var getter = typeBuilder.DefineMethod(string.Format("get_{0}", propertyInfo.Name), MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyInfo.PropertyType, Type.EmptyTypes);
            var ilGenerator = getter.GetILGenerator();

            var contextConstructorInfo = _configuration.ContextType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);

            var entityType = GetEntityType(propertyInfo);

            var contextMethodName = ExpressionHelper.GetMethodName<DbContext>(x => x.RepositoryToList<object>(null));
            var repositoryMethodInfo = _configuration.ContextType.GetMethod(contextMethodName).MakeGenericMethod(entityType);

            var queryFactoryConstructor = typeof(QueryFactory).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            var createQueryMethodName = ExpressionHelper.GetMethodName<QueryFactory>(x => x.CreateQuery<object>(null, null, null));
            var createQueryMethod = typeof(QueryFactory).GetMethod(createQueryMethodName, new Type[] { typeof(object), typeof(string), typeof(string) }).MakeGenericMethod(entityType);

            // Define our label we will jump to if the field already has a value
            var returnLabel = ilGenerator.DefineLabel();

            // Push the object(this) onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            // Push the field value onto the stack
            ilGenerator.Emit(OpCodes.Ldfld, field);
            // If the field has a value(not null) we will jump to our label and return without going to the database
            ilGenerator.Emit(OpCodes.Brtrue, returnLabel);

            // Push the object(this) onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            // Create a context instance
            ilGenerator.Emit(OpCodes.Newobj, contextConstructorInfo);
            
            // Create a query factory instance
            ilGenerator.Emit(OpCodes.Newobj, queryFactoryConstructor);

            // Push all the CreateQuery parameters onto the stack and call the CreateQuery method on our query factory instance
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldstr, _configuration.Schema.GetPrimaryKeyProperty(typeBuilder.BaseType).Name);
            ilGenerator.Emit(OpCodes.Ldstr, _configuration.Schema.GetForeignKeyProperty(typeBuilder.BaseType, entityType).Name);
            ilGenerator.Emit(OpCodes.Callvirt, createQueryMethod);

            // Call the repository method on the context instance
            ilGenerator.Emit(OpCodes.Callvirt, repositoryMethodInfo);
            // Store the value returned in our field
            ilGenerator.Emit(OpCodes.Stfld, field);

            // Mark our label we will be jumping to if our field already has a value
            ilGenerator.MarkLabel(returnLabel);

            // Push the object(this) onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            // Push the field value onto the stack
            ilGenerator.Emit(OpCodes.Ldfld, field);

            // Return the value stored in our field
            ilGenerator.Emit(OpCodes.Ret);

            return getter;
        }

        private Type GetEntityType(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsICollection())
                return propertyInfo.GetUnderlyingPropertyType();

            return propertyInfo.PropertyType;
        }

        private MethodBuilder DefineGetterDep(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldBuilder field)
        {
            var getter = typeBuilder.DefineMethod(string.Format("get_{0}", propertyInfo.Name), MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyInfo.PropertyType, Type.EmptyTypes);
            var ilGenerator = getter.GetILGenerator();

            // Added for debugging purposes
            ilGenerator.Emit(OpCodes.Ldstr, string.Format("Getter invoked on '{0}.{1}'", propertyInfo.DeclaringType.Name, propertyInfo.Name));
            ilGenerator.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) }));

            // Push the object(this) onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);

            // Push the fields value onto the stack
            ilGenerator.Emit(OpCodes.Ldfld, field);

            // Return the value on top of the stack
            ilGenerator.Emit(OpCodes.Ret);

            return getter;
        }

        private MethodBuilder DefineSetter(TypeBuilder typeBuilder, PropertyInfo propertyInfo, FieldBuilder field)
        {
            var setter = typeBuilder.DefineMethod(string.Format("set_{0}", propertyInfo.Name), MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { propertyInfo.PropertyType });
            var ilGenerator = setter.GetILGenerator();

            // Push the object(this) onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);

            // Push the "value" onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_1);

            // Store the value on the stack in our field
            ilGenerator.Emit(OpCodes.Stfld, field);

            // Return from the method, no value on the stack at this point
            ilGenerator.Emit(OpCodes.Ret);

            return setter;
        }

        private FieldBuilder DefineField(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            return typeBuilder.DefineField(string.Format("_{0}", propertyInfo.Name), propertyInfo.PropertyType, FieldAttributes.Private);
        }

        private PropertyBuilder DefineProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            return typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);
        }

        private string GetProxyName(Type baseType)
        {
            return string.Format("{0}_Proxy_{1}", baseType.Name, Random.Next(0, int.MaxValue));
        }
    }
}

