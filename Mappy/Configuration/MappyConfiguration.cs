using Mappy.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mappy.Configuration
{
    internal class MappyConfiguration : IMappyConfiguration
    {
        private IDatabaseConnection _connection;
        private ISchemaAnalyzer _schemaAnalyzer;

        public MappyConfiguration(string connectionString)
        {
            _connection = new SqlServerConnection("");
            _schemaAnalyzer = new SqlServerSchemaAnalyzer();
        }
    }
}
