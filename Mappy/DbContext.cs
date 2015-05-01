using Mappy.Mapping;
using Mappy.SqlServer;
using System;
using System.Collections.Generic;

namespace Mappy
{
    public class DbContext : IDisposable
    {
        private readonly SqlServerConnection _connection;
        private readonly EntityMapper _entityMapper;

        public DbContext(string connectionString)
        {
            _connection = new SqlServerConnection(connectionString);
            _entityMapper = new EntityMapper(_connection.GetTables());
        }

        public IEnumerable<TEntity> ExectuteQuery<TEntity>(string query) where TEntity : new()
        {
            using(var command = _connection.GetCommand(query))
            using(var reader = command.ExecuteReader())
            {
                return _entityMapper.Map<TEntity>(reader);
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
