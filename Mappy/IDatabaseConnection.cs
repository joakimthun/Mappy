using System.Data;
using System.Data.SqlClient;

namespace Mappy
{
    internal interface IDatabaseConnection
    {
        SqlCommand GetCommand(string query);
        void Dispose();
    }
}
