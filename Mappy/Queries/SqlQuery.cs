using Mappy.Configuration;
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
        private const string TableAliasTemplate = "Table{0}";
        private const string ColumnNameTemplate = "[{0}]";

        private MappyConfiguration _configuration;
        private List<string> _columns;
        private List<string> _includes;
        private int _aliasCounter;
        private Table _table;

        public SqlQuery()
        {
            SetColumns();
            _includes = new List<string>();
            _aliasCounter = 1;
        }

        public SqlQuery<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            _includes.Add(ExpressionHelper.GetPropertyName(expression));

            return this;
        }

        internal string Compile()
        {
            _table = _configuration.Schema.Tables.Single(t => t.Name == typeof(TEntity).Name);

            var sb = new StringBuilder();

            AddSelectStatement(sb);
            AddIncludedColumns(sb);
            AddFromStatement(sb);

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

        private void AddFromStatement(StringBuilder sb)
        {
            sb.Append(string.Format(" FROM {0} AS {1}", _table.Name, GetNextTableAlias()));
        }

        private string GetNextTableAlias()
        {
            return string.Format(TableAliasTemplate, _aliasCounter++);
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
