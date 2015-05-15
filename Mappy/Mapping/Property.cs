using System;
using System.Reflection;

namespace Mappy.Mapping
{
    internal class Property
    {
        public PropertyInfo PropertyInfo { get; set; }

        public string Name
        {
            get
            {
                return PropertyInfo.Name;
            }
        }

        public Type Type
        {
            get
            {
                return PropertyInfo.PropertyType;
            }
        }
    }
}
