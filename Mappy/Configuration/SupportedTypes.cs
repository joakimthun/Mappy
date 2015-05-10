using Mappy.Exceptions;
using System;
using System.Linq;

namespace Mappy.Configuration
{
    internal static class SupportedTypes
    {
        public static bool Contains(Type type)
        {
            return Types().Any(x => x.Type == type);
        }

        public static bool Contains(string sqlType)
        {
            return Types().Any(x => x.SqlType == sqlType);
        }

        public static Type TryResolveType(string sqlType, bool nullable)
        {
            var supportedType = Types().SingleOrDefault(x => x.SqlType == sqlType && x.Nullable == nullable);

            if (supportedType == null)
                throw new MappyException("Unsupported type '{0}'.", sqlType);

            return supportedType.Type;
        }

        public static string TryResolveSqlType(Type type)
        {
            var supportedType = Types().SingleOrDefault(x => x.Type == type);

            if(supportedType == null)
                throw new MappyException("Unsupported type '{0}'.", type.FullName);

            return supportedType.SqlType;
        }

        private static SupportedType[] Types()
        {
            return new SupportedType[]
            {
                new SupportedType { Type = typeof(int), SqlType = "int" },
                new SupportedType { Type = typeof(string), SqlType = "nvarchar", Nullable = true },
                new SupportedType { Type = typeof(bool), SqlType = "bit" },
                new SupportedType { Type = typeof(bool?), SqlType = "bit", Nullable = true },
                new SupportedType { Type = typeof(DateTime), SqlType = "datetime" },
                new SupportedType { Type = typeof(DateTime?), SqlType = "datetime", Nullable = true },
            };
        }
    }
}
