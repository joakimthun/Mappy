using Mappy.Configuration;

namespace Mappy.SqlServer
{
    internal class SqlServerSchemaAnalyzer : ISchemaAnalyzer
    {
        public IMappyConfiguration GetConfigurator()
        {

            return new MappyConfiguration();
        }
    }
}
