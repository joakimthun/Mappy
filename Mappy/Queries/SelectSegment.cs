using Mappy.Configuration;
using Mappy.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Mappy.Queries
{
    internal class SelectSegment<TEntity> : QuerySegment<TEntity>
    {
        private const string ColumnNameTemplate = "{0}.[{1}] AS {2}";

        private List<string> _columnsToSelect;

        public SelectSegment(MappyConfiguration configuration, QueryHelper helper, List<Include> includes) : base(configuration, helper, includes)
        {
            _columnsToSelect = new List<string>();
        }

        public override void Compile(StringBuilder sb)
        {
            GetColumnsToSelect();
            AddSelectStatement(sb);
        }

        private void AddSelectStatement(StringBuilder sb)
        {
            sb.Append("SELECT ");
            sb.Append(string.Join(", ", _columnsToSelect));
        }

        private void GetColumnsToSelect()
        {
            var entityProperties = PropertyHelper.GetPublicInstanceProperties<TEntity>();
            var entityType = typeof(TEntity);

            foreach (var property in entityProperties)
            {
                if (SupportedTypes.Contains(property.PropertyType))
                    _columnsToSelect.Add(string.Format(ColumnNameTemplate, _helper.GetTableAlias(entityType), property.Name, _helper.GetColumnAlias(entityType, property.Name)));
            }

            foreach (var include in _includes)
            {
                var includedProperties = PropertyHelper.GetPublicInstanceProperties(include.UnderlyingPropertyType);

                foreach (var property in includedProperties)
                {
                    if (SupportedTypes.Contains(property.PropertyType))
                        _columnsToSelect.Add(string.Format(ColumnNameTemplate, _helper.GetTableAlias(include.UnderlyingPropertyType), property.Name, _helper.GetColumnAlias(include.UnderlyingPropertyType, property.Name)));
                }
            }
        }
    }
}
