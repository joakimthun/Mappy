using System;
using System.Linq;
using System.Reflection;

namespace Mappy.Extensions
{
    internal static class AssemblyExtensions
    {
        public static Type GetMatchingMappyEntity(this Assembly assembly, string typeName)
        {
            return assembly.DefinedTypes.SingleOrDefault(t => t.IsMappyEntity() && t.Name == typeName);
        }
    }
}
