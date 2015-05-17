using Mappy.Configuration;
using Mappy.Extensions;
using Mappy.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mappy.Mapping
{
    internal class EntityCollection
    {
        private MappyConfiguration _configuration;
        private Dictionary<Type, List<object>> _entities;

        public EntityCollection(MappyConfiguration configuration)
        {
            _configuration = configuration;
            _entities = new Dictionary<Type, List<object>>();
        }

        public void AddDistinctEntity(object entity)
        {
            var entityType = entity.GetType();

            if (!_entities.ContainsKey(entityType))
            {
                _entities.Add(entityType, new List<object>());
            }

            var primaryKey = _configuration.Schema.Constraints.OfType<PrimaryKey>().Single(pk => pk.Table.EntityType == entityType);
            var primaryKeyProperty = entityType.GetProperty(primaryKey.Column.Name);

            if (!_entities[entityType].Any(x => primaryKeyProperty.AreValuesEqual(x, entity)))
            {
                _entities[entityType].Add(entity);
            }
        }

        public IEnumerable<Type> AllEntityTypes()
        {
            return _entities.Keys;
        }

        public IEnumerable<object> GetEntities(Type entityType)
        {
            if(_entities.ContainsKey(entityType))
                return _entities[entityType];

            return null;
        }
    }
}
