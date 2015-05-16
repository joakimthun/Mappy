using System;
using System.Collections.Generic;

namespace Mappy.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<object> DynamicCast(this IEnumerable<object> source, Type type)
        {
            var result = new List<object>();

            foreach (var obj in source)
            {
                result.Add(Convert.ChangeType(obj, type));
            }

            return result;
        }
    }
}
