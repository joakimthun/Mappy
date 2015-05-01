using System;
using System.Reflection;

namespace Mappy.Reflection
{
    internal static class MethodInvoker
    {
        public static object InvokeGenericMethod(object obj, string name, Type genericType, params object[] parameters)
        {
            var type = obj.GetType();
            var method = type.GetMethod(name);
            var generic = method.MakeGenericMethod(genericType);

            return generic.Invoke(obj, parameters);
        }
    }
}
