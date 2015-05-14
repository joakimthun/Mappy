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
            sb.Append(string.Format(" FROM {0} AS {1}", _table.Name, _helper.GetTableAlias(typeof(TEntity))));
        }

        private void AddJoinStatement(StringBuilder sb)
        {
            foreach (var include in _includes)
            {
                sb.Append(
                    string.Format(
                    " {0} {1} AS {2} ON {3} = {4}",
                    GetJoinType(include),
                    include.UnderlyingPropertyType.Name,
                    _helper.GetTableAlias(include.UnderlyingPropertyType),
                    GetFkColumn(include),
                    GetPkColumn(include)
                    ));
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
            var foreignKey = _configuration.Schema.Constraints.OfType<ForeignKey>().Single(fk => fk.FkTable.Name == include.UnderlyingPropertyType.Name);

            return string.Format("{0}.{1}", _helper.GetTableAlias(include.UnderlyingPropertyType), foreignKey.FkColumn.Name);
        }

        private string GetPkColumn(Include include)
        {
            var primaryKey = _configuration.Schema.Constraints.OfType<PrimaryKey>().Single(pk => pk.Table.Name == _table.Name);

            return string.Format("{0}.{1}", _helper.GetTableAlias(typeof(TEntity)), primaryKey.Column.Name);
        }
    }
}
