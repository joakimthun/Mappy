using Mappy.Configuration;
using Mappy.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mappy.Queries
{
    internal abstract class QuerySegment<TEntity>
    {
        protected MappyConfiguration _configuration;
        protected QueryHelper _helper;
        protected List<Include> _includes;
        protected Table _table;

        public QuerySegment(MappyConfiguration configuration, QueryHelper helper, List<Include> includes)
        {
            _configuration = configuration;
            _helper = helper;
            _includes = includes;

            _table = _configuration.Schema.Tables.Single(t => t.Name == typeof(TEntity).Name);
        }

        public abstract void Compile(StringBuilder sb);
    }
}
