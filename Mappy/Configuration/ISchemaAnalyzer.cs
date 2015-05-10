using Mappy.Schema;

namespace Mappy.Configuration
{
    internal interface ISchemaAnalyzer
    {
        DatabaseSchema GetSchema();
    }
}
