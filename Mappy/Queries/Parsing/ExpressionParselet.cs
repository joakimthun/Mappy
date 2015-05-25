using System.Linq.Expressions;
using System.Text;

namespace Mappy.Queries.Parsing
{
    internal abstract class ExpressionParselet
    {
        public abstract void Parse(ExpressionParser parser, Expression expression, StringBuilder sb);
    }
}
