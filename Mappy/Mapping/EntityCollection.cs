using System;
using System.Collections.Generic;

namespace Mappy.Mapping
{
    internal class EntityCollection
    {
        private Dictionary<Type, List<object>> _entities;

        public EntityCollection()
        {
            _entities = new Dictionary<Type, List<object>>();
        }

        public void AddEntity(object entity)
        {
            var entityType = entity.GetType();

            if (!_entities.ContainsKey(entityType))
            {
                _entities.Add(entityType, new List<object>());
            }

            _entities[entityType].Add(entity);
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
