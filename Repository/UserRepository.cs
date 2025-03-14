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

        public void Add(User user)
        {
            using IDbConnection connection = new NpgsqlConnection(_connectionString); // create connection to postgresql database
            using IDbCommand command = connection.CreateCommand(); // create sql command
            connection.Open();

            command.CommandText = "INSERT INTO Users (Username, Password) " +
            "VALUES (@Username, @Password) RETURNING UserId"; // Sets the SQL command to insert a user and return the UserId
            AddParameterWithValue(command, "Username", DbType.String, user.Username); // Adds a parameter for Username with type String.
            AddParameterWithValue(command, "Password", DbType.String, user.Password);
            user.Id = (int)(command.ExecuteScalar() ?? 0);
        }

        public void Delete(int userId)
        {   
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            using IDbCommand command = connection.CreateCommand();
            connection.Open();
            
            command.CommandText = "DELETE FROM Users WHERE Userid=@Userid";
            AddParameterWithValue(command, "Userid", DbType.Int32, userId);
            command.ExecuteNonQuery();
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            using IDbCommand command = connection.CreateCommand();
            connection.Open();

            command.CommandText = "SELECT Userid, Username, Password, Elo FROM Users";

            using IDataReader reader = command.ExecuteReader();
            while(reader.Read())
            {
                users.Add(new User()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Elo = reader.GetInt32(3)
                });
            }
            return users;
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