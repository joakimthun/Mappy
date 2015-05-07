using System;

namespace Mappy.Schema
{
    internal class Column
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public Table Table { get; set; }
    }
}
