using Mappy.Configuration;
using Mappy.Mapping;
using Mappy.Queries;
using Mappy.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mappy
{
    public class DbContext : IDisposable
    {
        private readonly IDatabaseConnection _connection;
        private readonly MappyConfiguration _configuration;
        private readonly List<IConfigurator> _configurators;

        public DbContext(string connectionString)
        {
            _connection = new SqlServerConnection(connectionString);
            _configuration = new MappyConfiguration(connectionString, Assembly.GetCallingAssembly());
            _configurators = new List<IConfigurator>();
        }

        public IEnumerable<TEntity> Repository<TEntity>(SqlQuery<TEntity> query = null) where TEntity : new()
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

            var entityMapper = new EntityMapper(_configuration, query.AliasHelpers);

            using (var command = _connection.GetCommand(compiledQuery))
            using (var reader = command.ExecuteReader())
            {
                return entityMapper.Map<TEntity>(reader);
            }
        }
    }
}
