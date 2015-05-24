using Mappy.Queries;
using System.Reflection;

namespace Mappy.LazyLoading
{
    public class QueryFactory
    {
        public SqlQuery<TEntity> CreateQuery<TEntity>(object parentEntity, string primaryKeyProperty, string foreignKeyProperty) where TEntity : new()
        {
            var parentType = parentEntity.GetType();

            return null;
        }
    }
}

