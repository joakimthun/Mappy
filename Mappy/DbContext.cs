﻿using Mappy.Configuration;
using Mappy.Mapping;
using Mappy.Queries;
using Mappy.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mappy
{
    public abstract class DbContext : IDisposable
    {
        [ThreadStatic]
        private static MappyConfiguration _configuration;

        private readonly IDatabaseConnection _connection;
        private readonly List<IConfigurator> _configurators;

        public DbContext(string connectionString)
        {
            _connection = new SqlServerConnection(connectionString);

            if (_configuration == null)
            {
                _configuration = new MappyConfiguration(connectionString, Assembly.GetCallingAssembly(), GetType());
            }

            _configurators = new List<IConfigurator>();

            LazyLoading = true;
        }

        public bool LazyLoading { get; private set; }

        public IEnumerable<TEntity> Repository<TEntity>() where TEntity : new()
        {
            return Repository<TEntity>(null);
        }

        public List<TEntity> RepositoryToList<TEntity>(SqlQuery<TEntity> query) where TEntity : new()
        {
            return Repository<TEntity>(query).ToList();
        }

        public IEnumerable<TEntity> Repository<TEntity>(SqlQuery<TEntity> query) where TEntity : new()
        {
            return RepositoryImpl(query);
        }

        public void Configure<TEntity>(Action<Configurator<TEntity>> configurator) where TEntity : new()
        {
            var type = typeof(TEntity);
            var mappyConfigurator = _configurators.SingleOrDefault(x => x.EntityType == type);

            if (mappyConfigurator == null)
            {
                mappyConfigurator = new Configurator<TEntity>(typeof(TEntity));
                _configurators.Add(mappyConfigurator);
            }

            configurator((Configurator<TEntity>)mappyConfigurator);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        private IEnumerable<TEntity> RepositoryImpl<TEntity>(SqlQuery<TEntity> query) where TEntity : new()
        {
            if (query == null)
                query = new SqlQuery<TEntity>();

            query.Configuration = _configuration;

            var compiledQuery = query.Compile();

            var entityMapper = new EntityMapper(_configuration, query.AliasHelpers, LazyLoading);

            using (var command = _connection.GetCommand(compiledQuery))
            using (var reader = command.ExecuteReader())
            {
                return entityMapper.Map<TEntity>(reader);
            }
        }
    }
}
