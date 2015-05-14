using System;
using System.Collections.Generic;
using System.Linq;

namespace Mappy.Queries
{
    internal class QueryHelper
    {
        private int _aliasCounter;

        public QueryHelper()
        {
            _aliasCounter = 1;
            AliasHelpers = new List<AliasHelper>();
        }

        public List<AliasHelper> AliasHelpers { get; private set; }

        public string GetTableAlias(Type type)
        {
            CreateNextTableAliasIfNotExists(type);

            return AliasHelpers.SingleOrDefault(x => x.EntityType == type).TableAlias;
        }

        public string GetColumnAlias(Type type, string propertyName)
        {
            return AliasHelpers.SingleOrDefault(x => x.EntityType == type).GetColumnAlias(propertyName);
        }

        private void CreateNextTableAliasIfNotExists(Type type)
        {
            var aliasHelper = AliasHelpers.SingleOrDefault(x => x.EntityType == type);

            if (aliasHelper == null)
            {
                AliasHelpers.Add(new AliasHelper(type, ref _aliasCounter));
            }
        }
    }
}
