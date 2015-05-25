using Mappy.Exceptions;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Mappy.Queries.Parsing
{
    internal class ConstantExpressionParselet : ExpressionParselet
    {
        public override void Parse(ExpressionParser parser, Expression expression, StringBuilder sb)
        {
            var constantExpression = expression as ConstantExpression;
            sb.Append(constantExpression.Value);
        }
    }
}
