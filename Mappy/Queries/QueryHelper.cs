using System;
using System.Collections.Generic;

namespace Mappy.Queries
{
    internal class QueryHelper
    {
        private Dictionary<Type, TableAlias> _tableAliases;
        private int _aliasCounter;

        public QueryHelper()
        {
            _aliasCounter = 1;
            _tableAliases = new Dictionary<Type, TableAlias>();
        }

        public void GetNextTableAliasIfNotExists(Type type, int nestingLevel)
        {
            if (!_tableAliases.ContainsKey(type))
            {
                var alias = new TableAlias(ref _aliasCounter, nestingLevel);
                _tableAliases.Add(type, alias);
            }
        }

        public string GetTableAlias(Type type, int nestingLevel = 0)
        {
            GetNextTableAliasIfNotExists(type, nestingLevel);

            return _tableAliases[type].Aliases[nestingLevel];
        }

        private class TableAlias
        {
            private const string TableAliasTemplate = "Table{0}";

            public TableAlias(ref int aliasCounter, int nestingLevel)
            {
                Aliases = new Dictionary<int, string>();
                Aliases.Add(nestingLevel, GetNextTableAlias(ref aliasCounter));
            }

            public Dictionary<int, string> Aliases { get; set; }

            public string GetNextTableAlias(ref int aliasCounter)
            {
                return string.Format(TableAliasTemplate, aliasCounter++);
            }
        }
    }
}
