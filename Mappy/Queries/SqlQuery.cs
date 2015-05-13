using Mappy.Configuration;
using Mappy.Extensions;
using Mappy.Helpers;
using Mappy.Reflection;
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
        private const string ColumnNameTemplate = "[{0}]";

        private MappyConfiguration _configuration;
        private List<string> _columns;
        private List<Include> _includes;
        private Table _table;
        private QueryHelper _helper;

        public SqlQuery()
        {
            SetColumns();
            _includes = new List<Include>();
            _helper = new QueryHelper();
        }

        public SqlQuery<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            var properyInfo = ExpressionHelper.GetPropertyInfo(expression);

            _includes.Add(new Include
            {
                UnderlyingPropertyType = properyInfo.GetUnderlyingPropertyType(),
                PropertyName = properyInfo.Name
            });

            return this;
        }

        internal string Compile()
        {
            _table = _configuration.Schema.Tables.Single(t => t.Name == typeof(TEntity).Name);

            var sb = new StringBuilder();

            var fromSegment = new FromSegment<TEntity>(_configuration, _helper, _includes);

            AddSelectStatement(sb);
            AddIncludedColumns(sb);
            fromSegment.Compile(sb);

            return sb.ToString();
        }

        internal MappyConfiguration Configuration
        {
            set { _configuration = value; }
        }

        private void AddSelectStatement(StringBuilder sb)
        {
            sb.Append("SELECT ");
        }

        private void AddIncludedColumns(StringBuilder sb)
        {
            sb.Append(string.Join(", ", _columns));
        }

        private void SetColumns()
        {
            _columns = new List<string>();
            var properties = PropertyHelper.GetPublicInstanceProperties<TEntity>();

            foreach (var property in properties)
            {
                if(SupportedTypes.Contains(property.PropertyType))
                    _columns.Add(string.Format(ColumnNameTemplate, property.Name));
            }
        }
    }
}
