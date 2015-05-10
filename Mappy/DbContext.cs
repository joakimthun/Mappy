﻿using Mappy.Configuration;
using Mappy.Mapping;
using Mappy.Queries;
using Mappy.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mappy
{
    public class DbContext : IDisposable
    {
        private readonly IDatabaseConnection _connection;
        private readonly MappyConfiguration _configuration;
        private readonly EntityMapper _entityMapper;
        private readonly List<IConfigurator> _configurators;

        public DbContext(string connectionString)
        {
            _connection = new SqlServerConnection(connectionString);
            _configuration = new MappyConfiguration(connectionString);
            _entityMapper = new EntityMapper(_configuration);
            _configurators = new List<IConfigurator>();
        }

        public IEnumerable<TEntity> Tables<TEntity>(SqlQuery<TEntity> query = null) where TEntity : new()
        {
            return TablesImpl(query);
        }

        public IEnumerable<TEntity> ExectuteQuery<TEntity>(string query) where TEntity : new()
        {
            using(var command = _connection.GetCommand(query))
            using(var reader = command.ExecuteReader())
            {
                return _entityMapper.Map<TEntity>(reader);
            }
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

        private IEnumerable<TEntity> TablesImpl<TEntity>(SqlQuery<TEntity> query) where TEntity : new()
        {
            if (query == null)
                query = new SqlQuery<TEntity>();

            query.Configuration = _configuration;

            var compiledQuery = query.Compile();

            using (var command = _connection.GetCommand(compiledQuery))
            using (var reader = command.ExecuteReader())
            {
                return _entityMapper.Map<TEntity>(reader);
            }
        }
    }
}
