using Mappy.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mappy.Helpers
{
    internal static class ExpressionHelper
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression)
        {
            var type = typeof(TSource);

            var member = expression.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", expression.ToString()));

            var propertyInfo = member.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", expression.ToString()));

            return propertyInfo;
        }

        public static string GetPropertyName<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression)
        {
            var propertyInfo = GetPropertyInfo(expression);
            return propertyInfo.Name;
        }
    }
}
