using System;

namespace Mappy.Configuration
{
    internal class SupportedType
    {
        public Type Type { get; set; }
        public string SqlType { get; set; }
        public bool Nullable { get; set; }
    }
}
