using System;

namespace Mappy.Mapping
{
    internal class EntityFactory
    {
        public EntityFactory()
        {
        }

        public object CreateEntity(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
