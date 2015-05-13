using System;

namespace Mappy.Queries
{
    internal class Include
    {
        public Type UnderlyingPropertyType { get; set; }

        public string PropertyName { get; set; }

        public bool IsCollection { get; set; }
    }
}
