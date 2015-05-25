using Mappy.Exceptions;
using Mappy.LazyLoading;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mappy.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsICollection(this PropertyInfo propertyInfo)
        {
            if (!propertyInfo.PropertyType.IsGenericType)
                return false;

            return typeof(ICollection<>).IsAssignableFrom(propertyInfo.PropertyType.GetGenericTypeDefinition());
        }

        public static Type GetCollectionType(this PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.GetGenericArguments()[0];
        }

        public static Type GetUnderlyingPropertyType(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsICollection())
            {
                return propertyInfo.GetCollectionType();
            }

            return propertyInfo.PropertyType;
        }

        public static bool IsMappyEntity(this PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.ImplementsInterface(typeof(IMappyEntity));
        }

        public static object GetSafeValue(this PropertyInfo propertyInfo, object obj)
        {
            var objType = obj.GetType();
        
            if (objType.ImplementsInterface(typeof(IMappyProxy)))
            {
                var proxyProperty = objType.GetProperty(propertyInfo.Name);
                return proxyProperty.GetValue(obj);
            }
        
            return propertyInfo.GetValue(obj);
        }

        public static bool AreValuesEqual(this PropertyInfo propertyInfo, object obj1, object obj2)
        {
            var value1 = propertyInfo.GetValue(obj1);
            var value2 = propertyInfo.GetValue(obj2);

            return AreValuesEqual(value1, value2);
        }

        public static bool AreValuesEqual(this PropertyInfo propertyInfo1, PropertyInfo propertyInfo2, object obj1, object obj2)
        {
            object value1;
            object value2;

            GetPropertyValues(propertyInfo1, propertyInfo2, obj1, obj2, out value1, out value2);

            return AreValuesEqual(value1, value2);
        }

        private static void GetPropertyValues(PropertyInfo propertyInfo1, PropertyInfo propertyInfo2, object obj1, object obj2, out object value1, out object value2)
        {
            if (propertyInfo1.DeclaringType == obj1.GetType() || propertyInfo1.DeclaringType == obj1.GetType().BaseType)
            {
                value1 = propertyInfo1.GetValue(obj1);
                value2 = propertyInfo2.GetValue(obj2);
            }
            else
            {
                value1 = propertyInfo1.GetValue(obj2);
                value2 = propertyInfo2.GetValue(obj1);
            }
        }

        private static bool AreValuesEqual(object value1, object value2)
        {
            if (value1.GetType() != value2.GetType())
                return false;

            var type = value1.GetType();

            if (type == typeof(int))
                return ((int)value1) == ((int)value2);

            if (type == typeof(Guid))
                return ((Guid)value1) == ((Guid)value2);

            throw new MappyException($"Unsupported type '{type.FullName}'");
        }
    }
}
