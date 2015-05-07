using System;

namespace Mappy.Configuration
{
    internal class HasMany
    {
        public string Property { get; set; }

        public Type Type { get; set; }

        public string ForeignKey { get; set; }
    }
}
