using Mappy.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Mappy.Queries.Parsing
{
    internal class ExpressionParser
    {
        private readonly Dictionary<Type, ExpressionParselet> _parselets;

        public ExpressionParser(QueryHelper queryHelper)
        {
            QueryHelper = queryHelper;
            _parselets = new Dictionary<Type, ExpressionParselet>();
            RegisterParselets();
        }

        public QueryHelper QueryHelper { get; private set; }

        public void Parse<TEntity>(Expression<Func<TEntity, bool>> expression, StringBuilder sb)
        {
            ParseExpression(expression.Body, sb);
        }

        public void ParseExpression(Expression expression, StringBuilder sb)
        {
            if (expression is LambdaExpression)
            {
                expression = (expression as LambdaExpression).Body;
            }

            var parselet = GetParselet(expression);
            parselet.Parse(this, expression, sb);
        }

        private ExpressionParselet GetParselet(Expression expression)
        {
            var expressionType = expression.GetType();

            if (_parselets.ContainsKey(expressionType))
                return _parselets[expressionType];

            if (_parselets.ContainsKey(expressionType.BaseType))
                return _parselets[expressionType.BaseType];

            throw new MappyException($"The expression type '{expressionType.FullName}' is not supported.");
        }

        private void RegisterParselets()
        {
            _parselets.Add(typeof(BinaryExpression), new BinaryExpressionParselet());
            _parselets.Add(typeof(MemberExpression), new MemberExpressionParselet());
            _parselets.Add(typeof(ConstantExpression), new ConstantExpressionParselet());
        }
    }
}
