using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public PropertyInfo GetPrimaryKeyProperty(Type entityType)
        {
            var pk = Constraints.OfType<PrimaryKey>().Single(x => x.Table.EntityType == entityType);
            return entityType.GetProperty(pk.Column.Name);
        }

        public PropertyInfo GetForeignKeyProperty(Type pkEntityType, Type fkEntityType)
        {
            var fk = Constraints.OfType<ForeignKey>().Single(x => x.PkTable.EntityType == pkEntityType && x.FkTable.EntityType == fkEntityType);
            return fkEntityType.GetProperty(fk.FkColumn.Name);
        }
    }
}
