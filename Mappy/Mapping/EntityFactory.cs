using System;

namespace Mappy.Mapping
{
    internal class EntityFactory
    {
        public EntityFactory()
        {
        }

        public TEntity CreateEntity<TEntity>() where TEntity : new()
        {
            return Activator.CreateInstance<TEntity>();
        }
    }
}
