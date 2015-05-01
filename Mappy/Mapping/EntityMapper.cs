using Mappy.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Mappy.Mapping
{
    internal class EntityMapper
    {
        private readonly DataTable _tables;
        private readonly EntityFactory _entityFactory;

        public EntityMapper(DataTable tables)
        {
            _tables = tables;
            _entityFactory = new EntityFactory();
        }

        public IEnumerable<TEntity> Map<TEntity>(SqlDataReader reader) where TEntity : new()
        {
            var type = typeof(TEntity);
            if (!ContainsTable(type))
                throw new MappyException("The entity type '{0}' has no matching table in the database.", type.Name);

            return MapImpl<TEntity>(reader);
        }

        private bool ContainsTable(Type entityType)
        {
            foreach (DataRow row in _tables.Rows)
            {
                if ((string)row[2] == entityType.Name)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<TEntity> MapImpl<TEntity>(SqlDataReader reader) where TEntity : new()
        {
            var result = new List<TEntity>();

            while (reader.Read())
            {
                result.Add(_entityFactory.CreateEntity<TEntity>(reader));
            }

            return result;
        }
    }
}
