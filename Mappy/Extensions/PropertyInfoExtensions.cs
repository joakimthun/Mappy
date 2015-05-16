using Mappy.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mappy.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsICollection(this PropertyInfo propertyInfo)
        {
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

        public static bool AreValuesEqual(this PropertyInfo propertyInfo, object obj1, object obj2)
        {
            var value1 = propertyInfo.GetValue(obj1);
            var value2 = propertyInfo.GetValue(obj2);

            return AreValuesEqual(value1, value2);
        }

        public static bool AreValuesEqual(this PropertyInfo propertyInfo1, PropertyInfo propertyInfo2, object obj1, object obj2)
        {
            var value1 = propertyInfo1.GetValue(obj1);
            var value2 = propertyInfo2.GetValue(obj2);

            return AreValuesEqual(value1, value2);
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

            throw new MappyException("Unsupported type '{0}'", type.FullName);
        }
    }
}
