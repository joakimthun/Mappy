using Mappy.Configuration;
using Mappy.Schema;
using System.Collections.Generic;
using System.Reflection;

namespace Mappy.SqlServer
{
    internal class SqlServerSchemaAnalyzer : ISchemaAnalyzer
    {
        const string TablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";
        const string ColumnsQueryTemplate = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'";
        const string PrimaryKeysQueryTemplate = "EXEC sys.sp_pkeys '{0}'";
        const string ForeignKeysQueryTemplate = "EXEC sys.sp_fkeys '{0}'";

        private readonly IDatabaseConnection _connection;
        private readonly Assembly _callingAssembly;
        private DatabaseSchema _schema;

        public SqlServerSchemaAnalyzer(IDatabaseConnection connection, Assembly callingAssembly)
        {
            _connection = connection;
            _callingAssembly = callingAssembly;
        }

        public DatabaseSchema GetSchema()
        {
            if (_schema != null)
                return _schema;

            var tables = GetTables();
            var constraints = GetConstraints(tables);

            _schema = new DatabaseSchema(tables, constraints);

            return _schema;
        }

        private IEnumerable<Table> GetTables()
        {
            var tables = GetTableNames();

            foreach (var table in tables)
                GetColumns(table);

            return tables;
        }

        private IEnumerable<Constraint> GetConstraints(IEnumerable<Table> tables)
        {
            var constraints = new List<Constraint>();

            constraints.AddRange(GetPrimaryKeys(tables));
            constraints.AddRange(GetForeignKeys(tables));

            return constraints;
        }

        private IEnumerable<Constraint> GetPrimaryKeys(IEnumerable<Table> tables)
        {
            var constraints = new List<Constraint>();

            foreach (var table in tables)
            {
                using (var command = _connection.GetCommand(string.Format(PrimaryKeysQueryTemplate, table.Name)))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        constraints.Add(new PrimaryKey(table, reader));
                    }
                }
            }

            return constraints;
        }

        private IEnumerable<Constraint> GetForeignKeys(IEnumerable<Table> tables)
        {
            var constraints = new List<Constraint>();

            foreach (var table in tables)
            {
                using (var command = _connection.GetCommand(string.Format(ForeignKeysQueryTemplate, table.Name)))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        constraints.Add(new ForeignKey(tables, reader));
                    }
                }
            }

            return constraints;
        }

        private IEnumerable<Table> GetTableNames()
        {
            var tables = new List<Table>();

            using (var command = _connection.GetCommand(TablesQuery))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tables.Add(new Table(reader, _callingAssembly));
                }
            }

            return tables;
        }

        private void GetColumns(Table table)
        {
            var columns = new List<Column>();

            using (var command = _connection.GetCommand(string.Format(ColumnsQueryTemplate, table.Name)))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(new Column(table, reader));
                }
            }

            table.Columns = columns;
        }
    }
}
