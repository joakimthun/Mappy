using System;
using System.Data.SqlClient;
using System.Linq;

namespace Mappy.Schema
{
    internal class PrimaryKey : Constraint
    {
        private const string COLUMN_NAME_COLUMN = "COLUMN_NAME";

        public PrimaryKey(Table table, SqlDataReader reader)
        {
            Table = table;
            Init(reader);
        }

        public Column Column { get; set; }

        public Table Table { get; set; }

        public override ConstraintType ConstraintType
        {
            get
            {
                return ConstraintType.PrimaryKey;
            }
        }

        protected void Init(SqlDataReader reader)
        {
            for (var column = 0; column < reader.FieldCount; column++)
            {
                var columnName = reader.GetName(column);

                if (columnName == COLUMN_NAME_COLUMN)
                {
                    Column = Table.Columns.Single(c => c.Name == reader.GetString(column));
                    break;
                }
            }
        }
    }
}
