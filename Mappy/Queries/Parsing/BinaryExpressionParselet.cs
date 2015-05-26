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
            var isLeftOrRightBranchANullExpression = IsLeftOrRightBranchANullExpression(binaryExpression);

            parser.ParseExpression(binaryExpression.Left, sb);
            GetOperator(binaryExpression.NodeType, isLeftOrRightBranchANullExpression, sb);
            parser.ParseExpression(binaryExpression.Right, sb);
        }

        private void GetOperator(ExpressionType nodeType, bool isLeftOrRightBranchANullExpression, StringBuilder sb)
        {
            if (nodeType == ExpressionType.Equal)
            {
                if (isLeftOrRightBranchANullExpression)
                {
                    sb.Append(" IS ");
                }
                else
                {
                    sb.Append(" = ");
                }

            }
            else if (nodeType == ExpressionType.OrElse)
            {
                sb.Append(" OR ");
            }
            else if (nodeType == ExpressionType.AndAlso)
            {
                sb.Append(" AND ");
            }
            else if (nodeType == ExpressionType.NotEqual)
            {
                if (isLeftOrRightBranchANullExpression)
                {
                    sb.Append(" IS NOT ");
                }
                else
                {
                    sb.Append(" <> ");
                }
            }
            else
            {
                throw new MappyException($"The ExpressionType '{nodeType}' is not yet supported.");
            }
        }

        private bool IsLeftOrRightBranchANullExpression(BinaryExpression expression)
        {
            return IsBranchANullExpression(expression.Left) || IsBranchANullExpression(expression.Right);
        }

        private bool IsBranchANullExpression(Expression expression)
        {
            if (expression is ConstantExpression)
            {
                var ce = expression as ConstantExpression;
                return ce.Value == null;
            }

            return false;
        }
    }
}
