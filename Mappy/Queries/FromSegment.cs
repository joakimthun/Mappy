using Mappy.Configuration;
using Mappy.Exceptions;
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
            AssertIncludesAreValid();

            //sb.Append(string.Format(" FROM {0} AS {1}", _table.Name, GetNextTableAlias()));
        }

        private void AddFromStatement(StringBuilder sb)
        {
            sb.Append(string.Format(" FROM {0} AS {1}", _table.Name, _helper.GetNextTableAlias(typeof(TEntity))));
        }

        private void AddJoinStatement(StringBuilder sb)
        {
            sb.Append(string.Format(" FROM {0} AS {1}", _table.Name, _helper.GetNextTableAlias(typeof(TEntity))));
        }

        private void AssertIncludesAreValid()
        {
            var test = _configuration.Schema.Constraints.OfType<ForeignKey>().ToList();

            foreach (var include in _includes)
            {
                if (!_configuration.Schema.Constraints.OfType<ForeignKey>().Any(fk => fk.FkTable.Name == include.UnderlyingPropertyType.Name && fk.PkTable.Name == _table.Name))
                {
                    throw new MappyException("The included property {0} does not have a valid foreign key to the table {1}", include, _table.Name);
                }
            }
        }
    }
}
