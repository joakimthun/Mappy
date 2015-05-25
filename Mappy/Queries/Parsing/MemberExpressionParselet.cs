using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mappy.Queries.Parsing
{
    internal class MemberExpressionParselet : ExpressionParselet
    {
        public override void Parse(ExpressionParser parser, Expression expression, StringBuilder sb)
        {
            var memberExpression = expression as MemberExpression;

            var tableAlias = GetMemberTableAlias(parser, memberExpression.Member);
            sb.Append($"{tableAlias}.[{memberExpression.Member.Name}]");
        }

        private string GetMemberTableAlias(ExpressionParser parser, MemberInfo memberInfo)
        {
            return parser.QueryHelper.GetTableAlias(memberInfo.ReflectedType);
        }
    }
}
