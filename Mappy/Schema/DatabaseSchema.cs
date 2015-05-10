using System.Collections.Generic;

namespace Mappy.Schema
{
    internal class DatabaseSchema
    {
        public DatabaseSchema(IEnumerable<Table> tables, IEnumerable<Constraint> constraints)
        {
            Tables = tables;
            Constraints = constraints;
        }

        public IEnumerable<Table> Tables { get; set; }
        public IEnumerable<Constraint> Constraints { get; set; }
    }
}
