using System;
using System.Data.SqlClient;
using System.Configuration;
using Mappy.Exceptions;
using System.Data;
using System.Text;

namespace Mappy.SqlServer
{
    internal class SqlServerConnection : IDatabaseConnection, IDisposable
    {
        private readonly SqlConnection _connection;

        public SqlServerConnection(string connectionString)
        {
            _connection = new SqlConnection(GetConnectionString(connectionString));
        }

        public SqlCommand GetCommand(string query)
        {
            OpenConnectionIfNotOpen();

            return new SqlCommand(query, _connection);
        }

        public DataTable GetTables()
        {
            OpenConnectionIfNotOpen();

            return _connection.GetSchema("Tables");
        }

        public DataTable GetSchema()
        {
            OpenConnectionIfNotOpen();

            return _connection.GetSchema();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        private void OpenConnectionIfNotOpen()
        {
            if (_connection.State == ConnectionState.Open)
                return;

            _connection.Open();
        }

        private string GetConnectionString(string connectionString)
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionString];
            if (cs == null)
                throw new MappyException("Can not find the connection string '{0}'. \r\n Did you add it to the .config file?", connectionString);

            return cs.ConnectionString;
        }
    }
}
