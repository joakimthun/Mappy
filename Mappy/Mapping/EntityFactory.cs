using Mappy.LazyLoading;
using Mappy.Reflection;
using System;

namespace Mappy.Mapping
{
    internal class EntityFactory
    {
        private ProxyFactory _proxyFactory;
        private bool _lazyLoading;

        public EntityFactory(bool lazyLoading)
        {
            _proxyFactory = new ProxyFactory();
            _lazyLoading = lazyLoading;
        }

        public object CreateEntity(Type type)
        {
            if (_lazyLoading)
            {
                return Activator.CreateInstance(_proxyFactory.CreateProxy(type));
            }

            return Activator.CreateInstance(type);
        }
    }
}
