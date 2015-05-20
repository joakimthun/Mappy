using Mappy.Configuration;
using Mappy.LazyLoading;
using Mappy.Reflection;
using System;

namespace Mappy.Mapping
{
    internal class EntityFactory
    {
        private readonly MappyConfiguration _configuration;
        private readonly ProxyFactory _proxyFactory;
        private readonly bool _lazyLoading;

        public EntityFactory(MappyConfiguration configuration, bool lazyLoading)
        {
            _configuration = configuration;
            _proxyFactory = new ProxyFactory(configuration);
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
