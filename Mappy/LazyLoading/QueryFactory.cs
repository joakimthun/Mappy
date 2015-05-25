using Mappy.Helpers;
using Mappy.Queries;

namespace Mappy.LazyLoading
{
    public class QueryFactory
    {
        public SqlQuery<TEntity> CreateQuery<TEntity>(object parentEntity, string primaryKeyProperty, string foreignKeyProperty) where TEntity : new()
        {
            var parentType = parentEntity.GetType();
            var primaryKeyPropertyInfo = parentType.GetProperty(primaryKeyProperty);
            var primaryKeyPropertyValue = primaryKeyPropertyInfo.GetValue(parentEntity);

            var foreignKeyPropertyInfo = typeof(TEntity).GetProperty(foreignKeyProperty);

            var predicate = ExpressionBuilder.MakeBinaryEqualsMemberExpression<TEntity>(primaryKeyPropertyValue, foreignKeyPropertyInfo);
            return new SqlQuery<TEntity>().Where(predicate);
        }
    }
}

