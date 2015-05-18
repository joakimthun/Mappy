using Mappy.LazyLoading;
using System;

namespace Mappy.Extensions
{
    public static class ObjectExtensions
    {
        public static Type GetStaticType(this object obj)
        {
            var type = obj.GetType();
        
            if (type.ImplementsInterface(typeof(IMappyProxy)))
                return type.BaseType;
        
            return type;
        }
    }
}
