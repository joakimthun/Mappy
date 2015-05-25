using Mappy.Exceptions;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Mappy.Queries.Parsing
{
    internal class BinaryExpressionParselet : ExpressionParselet
    {
        public override void Parse(ExpressionParser parser, Expression expression, StringBuilder sb)
        {
            var binaryExpression = expression as BinaryExpression;

            parser.ParseExpression(binaryExpression.Left, sb);
            GetOperator(binaryExpression.NodeType, sb);
            parser.ParseExpression(binaryExpression.Right, sb);
        }

        private void GetOperator(ExpressionType nodeType, StringBuilder sb)
        {
            if (nodeType == ExpressionType.Equal)
            {
                sb.Append(" = ");

            }
            else if (nodeType == ExpressionType.OrElse)
            {
                sb.Append(" OR ");
            }
            else if (nodeType == ExpressionType.AndAlso)
            {
                sb.Append(" AND ");
            }
            else
            {
                throw new MappyException($"The ExpressionType '{nodeType}' is not yet supported.");
            }
        }
    }
}
