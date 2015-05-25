using Mappy.Configuration;
using Mappy.Queries.Parsing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Mappy.Queries
{
    internal class WhereSegment<TEntity> : QuerySegment<TEntity>
    {
        private readonly ExpressionParser _expressionParser;
        private readonly List<Expression<Func<TEntity, bool>>> _predicates;

        public WhereSegment(MappyConfiguration configuration, QueryHelper helper, List<Include> includes, List<Expression<Func<TEntity, bool>>> predicates) : base(configuration, helper, includes)
        {
            _expressionParser = new ExpressionParser(_helper);
            _predicates = predicates;
        }

        public override void Compile(StringBuilder sb)
        {
            AddWhereStatement(sb);
        }

        private void AddWhereStatement(StringBuilder sb)
        {
            sb.Append(" WHERE ");

            foreach (var predicate in _predicates)
            {
                _expressionParser.Parse(predicate, sb);
            }
        }
    }
}
