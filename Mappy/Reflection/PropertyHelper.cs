using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mappy.Reflection
{
    internal static class PropertyHelper
    {
        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        }
    }
}
