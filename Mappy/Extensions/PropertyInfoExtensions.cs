using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mappy.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool SupportedMappingType(this PropertyInfo propertyInfo)
        {
            return true;
        }

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
    }
}
