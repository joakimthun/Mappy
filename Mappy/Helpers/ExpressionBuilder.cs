using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mappy.Helpers
{
    internal static class ExpressionBuilder
    {
        public static Expression<Func<T, bool>> MakeBinaryEqualsMemberExpression<T>(object value, PropertyInfo memberPropertyInfo)
        {
            var constantType = value.GetType();

            if (constantType != memberPropertyInfo.PropertyType)
                throw new ArgumentException($"The value type '{constantType.FullName}' and the member type '{memberPropertyInfo.PropertyType.FullName}' does not match.");

            var parameter = Expression.Parameter(typeof(T), "x");

            var memberAccess = Expression.MakeMemberAccess(parameter, memberPropertyInfo);

            var constant = Expression.Constant(value, constantType);

            var equal = Expression.Equal(memberAccess, constant);

            return Expression.Lambda<Func<T, bool>>(equal, new ParameterExpression[] { parameter });
        }
    }
}
