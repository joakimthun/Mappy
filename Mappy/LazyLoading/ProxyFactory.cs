using Mappy.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mappy.LazyLoading
{
    public class ProxyFactory
    {
        private const string DynamicAssemblyName = "Mappy_Dynamic";
        private const string DynamicModuleName = "Proxies";

        private readonly Dictionary<Type, Type> _typeCache;

        private AssemblyBuilder _assemblyBuilder;
        private ModuleBuilder _moduleBuilder;
        private Random _random;

        public ProxyFactory()
        {
            _typeCache = new Dictionary<Type, Type>();
        }

        private AssemblyBuilder AssemblyBuilder
        {
            get
            {
                if (_assemblyBuilder == null)
                {
                    var assemblyName = new AssemblyName(DynamicAssemblyName);
                    _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
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
                    _moduleBuilder = AssemblyBuilder.DefineDynamicModule(DynamicModuleName);
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
            return CreateSubType(baseType);
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
            return typeBuilder.DefineField(string.Format("_{0}", propertyInfo.Name), propertyInfo.DeclaringType, FieldAttributes.Private);
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

