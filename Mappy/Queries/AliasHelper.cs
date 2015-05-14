using System;

namespace Mappy.Queries
{
    internal class AliasHelper
    {
        private const string TableAliasTemplate = "Table{0}";
        private const string ColumnAliasTemplate = "{0}_{1}";

        public AliasHelper(Type type, ref int aliasCounter)
        {
            EntityType = type;
            SetTableAlias(ref aliasCounter);
        }

        public Type EntityType { get; private set; }

        public string GetColumnAlias(string propertyName)
        {
            return string.Format(ColumnAliasTemplate, TableAlias, propertyName);
        }

        public string GetPropertyName(string columnAlias)
        {
            return columnAlias.Split('_')[1];
        }

        public bool ColumnBelongsToEntity(string columnName)
        {
            return columnName.StartsWith(TableAlias);
        }

        public string TableAlias { get; private set; }

        private void SetTableAlias(ref int aliasCounter)
        {
            TableAlias = string.Format(TableAliasTemplate, aliasCounter++);
        }
    }
}
