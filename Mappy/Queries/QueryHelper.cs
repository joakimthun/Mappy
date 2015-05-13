using System;
using System.Collections.Generic;

namespace Mappy.Queries
{
    internal class QueryHelper
    {
        private Dictionary<Type, TableAlias> _tableAliases;

        public QueryHelper()
        {
            _tableAliases = new Dictionary<Type, TableAlias>();
        }

        public string GetNextTableAlias(Type type, int nestingLevel = 0)
        {
            TableAlias alias;

            if (!_tableAliases.ContainsKey(type))
            {
                alias = new TableAlias(nestingLevel);
                _tableAliases.Add(type, alias);
            }
            else
            {
                alias = _tableAliases[type];
            }

            return alias.Aliases[nestingLevel];
        }

        public string GetTableAlias(Type type, int nestingLevel = 0)
        {
            return _tableAliases[type].Aliases[nestingLevel];
        }

        private class TableAlias
        {
            private const string TableAliasTemplate = "Table{0}";

            private int _aliasCounter;

            public TableAlias(int nestingLevel)
            {
                Aliases = new Dictionary<int, string>();
                Aliases.Add(nestingLevel, GetNextTableAlias());
                _aliasCounter = 1;
            }

            public Dictionary<int, string> Aliases { get; set; }

            public string GetNextTableAlias()
            {
                return string.Format(TableAliasTemplate, _aliasCounter++);
            }
        }
    }
}
