using Npgsql;
using System;
using System.Data;

using SEB.Models;

namespace SEB.Repository
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddUser(User user)
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            using IDbCommand command = connection.CreateCommand();
            connection.Open();

            command.CommandText = "INSERT INTO Users (Username, Password) " +
            "VALUES (@Username, @Password) RETURNING UserId";
            AddParameterWithValue(command, "Username", DbType.String, user.Username);
            AddParameterWithValue(command, "Password", DbType.String, user.Password);
            user.Id = (int)(command.ExecuteScalar() ?? 0);
        }
        public static void AddParameterWithValue(IDbCommand command, string parameterName, DbType type, object value)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = type;
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}