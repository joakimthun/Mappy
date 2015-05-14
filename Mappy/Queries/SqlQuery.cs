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
        private MappyConfiguration _configuration;
        private List<Include> _includes;
        private Table _table;
        private QueryHelper _helper;

        public SqlQuery()
        {
            _includes = new List<Include>();
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

        internal string Compile()
        {
            _table = _configuration.Schema.Tables.Single(t => t.Name == typeof(TEntity).Name);

            AssertIncludesAreValid();

            var selectSegment = new SelectSegment<TEntity>(_configuration, _helper, _includes);
            var fromSegment = new FromSegment<TEntity>(_configuration, _helper, _includes);

            var sb = new StringBuilder();

            selectSegment.Compile(sb);
            fromSegment.Compile(sb);

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
                if (!_configuration.Schema.Constraints.OfType<ForeignKey>().Any(fk => fk.FkTable.Name == include.UnderlyingPropertyType.Name && fk.PkTable.Name == _table.Name))
                {
                    throw new MappyException("The included property {0} does not have a valid foreign key to the table {1}", include, _table.Name);
                }
            }
        }
    }
}
