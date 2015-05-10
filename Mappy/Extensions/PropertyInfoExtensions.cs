using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mappy.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool SupportedMappingType(this PropertyInfo propertyInfo)
        {
            return true;
        }
    }
}
