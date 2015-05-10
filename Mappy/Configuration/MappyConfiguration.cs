using Mappy.Schema;
using Mappy.SqlServer;

namespace Mappy.Configuration
{
    internal class MappyConfiguration
    {
        private IDatabaseConnection _connection;
        private ISchemaAnalyzer _schemaAnalyzer;
        private DatabaseSchema _schema;

        public MappyConfiguration(string connectionString)
        {
            _connection = new SqlServerConnection(connectionString);
            _schemaAnalyzer = new SqlServerSchemaAnalyzer(_connection);
        }

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
