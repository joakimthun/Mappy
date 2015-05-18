using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mappy.Extensions
{
    internal static class TypeExtensions
    {
        public static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            return type.GetInterfaces().Contains(interfaceType);
        }

        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(this Type type)
        {
            return Reflection.PropertyHelper.GetPublicInstanceProperties(type);
        }

        public static bool IsMappyEntity(this Type type)
        {
            return type.ImplementsInterface(typeof(IMappyEntity));
        }

        public static IEnumerable<PropertyInfo> GetPublicVirtualInstanceProperties(this Type type)
        {
            return Reflection.PropertyHelper.GetPublicVirtualInstanceProperties(type);
        }
    }
}
