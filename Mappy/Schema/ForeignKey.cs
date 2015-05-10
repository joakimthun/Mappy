using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Mappy.Schema
{
    internal class ForeignKey : Constraint
    {
        private const string PKTABLE_NAME_COLUMN = "PKTABLE_NAME";
        private const string PKCOLUMN_NAME_COLUMN = "PKCOLUMN_NAME";
        private const string FKTABLE_NAME_COLUMN = "FKTABLE_NAME";
        private const string FKCOLUMN_NAME_COLUMN = "FKCOLUMN_NAME";

        public ForeignKey(IEnumerable<Table> tables, SqlDataReader reader)
        {
            Init(tables, reader);
        }

        public Table PkTable { get; set; }

        public Table FkTable { get; set; }

        public Column PkColumn { get; set; }

        public Column FkColumn { get; set; }

        public bool IsNullable
        {
            get
            {
                return FkColumn.IsNullable;
            }
        }

        public override ConstraintType ConstraintType
        {
            get
            {
                return ConstraintType.ForeignKey;
            }
        }

        protected void Init(IEnumerable<Table> tables, SqlDataReader reader)
        {
            for (var column = 0; column < reader.FieldCount; column++)
            {
                var columnName = reader.GetName(column);

                if (columnName == PKTABLE_NAME_COLUMN)
                {
                    PkTable = tables.Single(t => t.Name == reader.GetString(column));
                    continue;
                } 

                if(columnName == PKCOLUMN_NAME_COLUMN)
                {
                    PkColumn = PkTable.Columns.Single(c => c.Name == reader.GetString(column));
                    continue;
                }

                if (columnName == FKTABLE_NAME_COLUMN)
                {
                    FkTable = tables.Single(t => t.Name == reader.GetString(column));
                    continue;
                }

                if (columnName == FKCOLUMN_NAME_COLUMN)
                {
                    FkColumn = FkTable.Columns.Single(c => c.Name == reader.GetString(column));
                    continue;
                }
            }
        }
    }
}
