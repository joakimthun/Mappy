using Mappy.Configuration;
using Mappy.Exceptions;
using Mappy.Extensions;
using Mappy.Helpers;
using Mappy.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mappy.Queries
{
    public class SqlQuery<TEntity> where TEntity : new()
    {
        private readonly List<Include> _includes;
        private readonly List<Expression<Func<TEntity, bool>>> _predicates;
        private readonly QueryHelper _helper;

        private MappyConfiguration _configuration;
        private Table _table;

        public SqlQuery()
        {
            _includes = new List<Include>();
            _predicates = new List<Expression<Func<TEntity, bool>>>();
            _helper = new QueryHelper();
        }

        public SqlQuery<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            var properyInfo = ExpressionHelper.GetPropertyInfo(expression);

            _includes.Add(new Include
            {
                UnderlyingPropertyType = properyInfo.GetUnderlyingPropertyType(),
                PropertyName = properyInfo.Name,
                IsCollection = properyInfo.IsICollection()
            });

            return this;
        }

        public SqlQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            if (_predicates.Any())
                throw new MappyException("Right now only one predicate is supported");

            _predicates.Add(predicate);

            return this;
        }

        internal string Compile()
        {
            _table = _configuration.Schema.Tables.Single(t => t.Name == typeof(TEntity).Name);

            AssertIncludesAreValid();

            var selectSegment = new SelectSegment<TEntity>(_configuration, _helper, _includes);
            var fromSegment = new FromSegment<TEntity>(_configuration, _helper, _includes);
            var whereSegment = new WhereSegment<TEntity>(_configuration, _helper, _includes, _predicates);

            var sb = new StringBuilder();

            selectSegment.Compile(sb);
            fromSegment.Compile(sb);
            whereSegment.Compile(sb);

            return sb.ToString();
        }

        internal MappyConfiguration Configuration
        {
            set { _configuration = value; }
        }

        internal List<AliasHelper> AliasHelpers
        {
            get { return _helper.AliasHelpers; }
        }

        private void AssertIncludesAreValid()
        {
            var test = _configuration.Schema.Constraints.OfType<ForeignKey>().ToList();

            foreach (var include in _includes)
            {
                if (!_configuration.Schema.Constraints.OfType<ForeignKey>().Any(fk => 
                    (fk.FkTable.Name == include.UnderlyingPropertyType.Name && fk.PkTable.Name == _table.Name) ||
                    (fk.PkTable.Name == include.UnderlyingPropertyType.Name && fk.FkTable.Name == _table.Name)))
                {
                    throw new MappyException($"The included property {include.PropertyName} does not have a valid foreign key to the table {_table.Name}");
                }
            }
        }
    }
}
