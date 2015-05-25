using Mappy.Configuration;
using Mappy.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mappy.Queries
{
    internal class FromSegment<TEntity> : QuerySegment<TEntity>
    {
        public FromSegment(MappyConfiguration configuration, QueryHelper helper, List<Include> includes) : base(configuration, helper, includes)
        {
        }

        public override void Compile(StringBuilder sb)
        {
            AddFromStatement(sb);

            if (_includes.Any())
                AddJoinStatement(sb);
        }

        private void AddFromStatement(StringBuilder sb)
        {
            sb.Append($" FROM [{_table.Name}] AS {_helper.GetTableAlias(typeof(TEntity))}");
        }

        private void AddJoinStatement(StringBuilder sb)
        {
            foreach (var include in _includes)
            {
                sb.Append($" {GetJoinType(include)} [{include.UnderlyingPropertyType.Name}] AS {_helper.GetTableAlias(include.UnderlyingPropertyType)} ON {GetFkColumn(include)} = {GetPkColumn(include)}");
            }
        }

        private string GetJoinType(Include include)
        {
            if (include.IsCollection)
                return "LEFT OUTER JOIN";

            return "INNER JOIN";
        }

        private string GetFkColumn(Include include)
        {
            var foreignKey = GetForeignKey(include);

            if (foreignKey.PkTable.Name == typeof(TEntity).Name)
            {
                return $"{_helper.GetTableAlias(include.UnderlyingPropertyType)}.{foreignKey.FkColumn.Name}";
            }
            else
            {
                return $"{_helper.GetTableAlias(include.UnderlyingPropertyType)}.{foreignKey.PkColumn.Name}";
            }
        }

        private string GetPkColumn(Include include)
        {
            var primaryKey = _configuration.Schema.Constraints.OfType<PrimaryKey>().Single(pk => pk.Table.Name == _table.Name);
            var foreignKey = GetForeignKey(include);

            if (foreignKey.PkTable.Name == typeof(TEntity).Name)
            {
                return $"{_helper.GetTableAlias(typeof(TEntity))}.{primaryKey.Column.Name}";
            }
            else
            {
                return $"{_helper.GetTableAlias(typeof(TEntity))}.{foreignKey.FkColumn.Name}";
            }
        }

        private ForeignKey GetForeignKey(Include include)
        {
            var entityType = typeof(TEntity);

            return _configuration.Schema.Constraints.OfType<ForeignKey>().Single(fk =>
                (fk.FkTable.Name == include.UnderlyingPropertyType.Name && fk.PkTable.Name == entityType.Name) ||
                (fk.PkTable.Name == include.UnderlyingPropertyType.Name && fk.FkTable.Name == entityType.Name));
        }
    }
}
