using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mappy.Reflection
{
    internal static class PropertyHelper
    {
        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties<T>()
        {
            return GetPublicInstanceProperties(typeof(T));
        }

        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        }
    }
}
