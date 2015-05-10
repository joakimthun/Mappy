using Mappy.Configuration;
using Mappy.Exceptions;
using System;
using System.Data.SqlClient;

namespace Mappy.Schema
{
    internal class Column
    {
        public Column(Table table, SqlDataReader reader)
        {
            Table = table;
            Init(reader);
        }

        public string Name { get; set; }
        public bool IsNullable { get; set; }
        public Type DataType { get; set; }
        public Table Table { get; set; }

        private void Init(SqlDataReader reader)
        {
            Name = reader.GetString(0);
            SetIsNullable(reader.GetString(2));
            SetType(reader.GetString(1));
        }

        private void SetType(string sqlType)
        {
            DataType = SupportedTypes.TryResolveType(sqlType, IsNullable);
        }

        private void SetIsNullable(string sqlValue)
        {
            if (sqlValue == "YES")
                IsNullable = true;
        }
    }
}
