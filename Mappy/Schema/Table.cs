using System;
using System.Collections.Generic;

namespace Mappy.Schema
{
    internal class Table
    {
        public string Name { get; set; }
        public ICollection<Column> Columns { get; set; }
        public Type EntityType { get; set; }
    }
}
