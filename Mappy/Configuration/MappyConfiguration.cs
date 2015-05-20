using Mappy.Schema;
using Mappy.SqlServer;
using System;
using System.Reflection;

namespace Mappy.Configuration
{
    internal class MappyConfiguration
    {
        private IDatabaseConnection _connection;
        private ISchemaAnalyzer _schemaAnalyzer;
        private DatabaseSchema _schema;

        public MappyConfiguration(string connectionString, Assembly callingAssembly, Type contextType)
        {
            _connection = new SqlServerConnection(connectionString);
            ConnectionString = connectionString;
            _schemaAnalyzer = new SqlServerSchemaAnalyzer(_connection, callingAssembly);
            ContextType = contextType;
        }

        public string ConnectionString { get; private set; }

        public Type ContextType { get; private set; }

        public DatabaseSchema Schema
        {
            get
            {
                if(_schema == null)
                    _schema = _schemaAnalyzer.GetSchema();

                return _schema;
            }
        }
    }
}
