using Mappy.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

namespace Mappy.Schema
{
    internal class Table
    {
        private Type _entityType;

        public Table(SqlDataReader reader, Assembly callingAssembly)
        {
            Init(reader);
            SetEntityType(callingAssembly);
        }

        public string Name { get; set; }
        public ICollection<Column> Columns { get; set; }

        public Type EntityType
        {
            get
            {
                return _entityType;
            }

            set
            {
                _entityType = value;
            }
        }

        private void Init(SqlDataReader reader)
        {
            Name = reader.GetString(0);
        }

        private void SetEntityType(Assembly callingAssembly)
        {
            EntityType = callingAssembly.GetMatchingMappyEntity(Name);
        }
    }
}
