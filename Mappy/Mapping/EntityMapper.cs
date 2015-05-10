using Mappy.Configuration;
using Mappy.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Mappy.Mapping
{
    internal class EntityMapper
    {
        private readonly MappyConfiguration _configuration;
        private readonly EntityFactory _entityFactory;

        public EntityMapper(MappyConfiguration configuration)
        {
            _configuration = configuration;
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
            foreach (var table in _configuration.Schema.Tables)
            {
                if (table.Name == entityType.Name)
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
