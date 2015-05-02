using Mappy.Configuration;
using Mappy.Mapping;
using Mappy.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mappy
{
    public class DbContext : IDisposable
    {
        private readonly SqlServerConnection _connection;
        private readonly EntityMapper _entityMapper;
        private readonly List<IMappyConfigurator> _configurators;

        public DbContext(string connectionString)
        {
            _connection = new SqlServerConnection(connectionString);
            _entityMapper = new EntityMapper(_connection.GetTables());
            _configurators = new List<IMappyConfigurator>();
        }

        public IEnumerable<TEntity> ExectuteQuery<TEntity>(string query) where TEntity : new()
        {
            using(var command = _connection.GetCommand(query))
            using(var reader = command.ExecuteReader())
            {
                return _entityMapper.Map<TEntity>(reader);
            }
        }

        public void Configure<TEntity>(Action<MappyConfigurator<TEntity>> configurator) where TEntity : new()
        {
            var type = typeof(TEntity);
            var mappyConfigurator = _configurators.SingleOrDefault(x => x.EntityType == type);

            if (mappyConfigurator == null)
            {
                mappyConfigurator = new MappyConfigurator<TEntity>(typeof(TEntity));
            }

            _configurators.Add(mappyConfigurator);

            configurator((MappyConfigurator<TEntity>)mappyConfigurator);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
