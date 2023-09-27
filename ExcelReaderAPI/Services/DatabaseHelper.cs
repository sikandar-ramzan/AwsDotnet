using System.Data.SqlClient;
namespace ExcelReaderAPI.Services
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string GetDbConnString()
        {

            return _connectionString;
        }

        public SqlConnection CreateDbConnection()
        {
            var connection = new SqlConnection(_connectionString);
            return connection;
        }

        public SqlCommand CreateSqlCommand(string query, SqlConnection connection)
        {
            var command = new SqlCommand(query, connection);
            return command;
        }
    }
}
