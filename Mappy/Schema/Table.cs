using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Mappy.Schema
{
    internal class Table
    {
        public Table(SqlDataReader reader)
        {
            Init(reader);
        }

        public string Name { get; set; }
        public ICollection<Column> Columns { get; set; }
        public Type EntityType { get; set; }

        private void Init(SqlDataReader reader)
        {
            Name = reader.GetString(0);
        }
    }
}
