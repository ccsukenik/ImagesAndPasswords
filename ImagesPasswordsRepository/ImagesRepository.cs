using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ImagesAndPasswords.Data
{
    public class ImagesRepository
    {
        private string _connectionString { get; set; }

        public ImagesRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int AddImage(string fileName, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Images(FileName, Password, Views)
VALUES(@name, @password, @views)
SELECT SCOPE_IDENTITY()";
            command.Parameters.AddWithValue("@name", fileName);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@views", 0);
            connection.Open();

            return (int)(decimal)command.ExecuteScalar(); ;
        }

        public Image GetImage(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Images WHERE ID = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            reader.Read();

            var image = new Image
            {
                ID = (int)reader["ID"],
                FileName = (string)reader["FileName"],
                Password = (string)reader["Password"],
                Views = (int)reader["Views"]
            };

            return image;
        }

        public string GetPassword(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Password FROM Images WHERE ID = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            return (string)command.ExecuteScalar();
        }

        public Image VerifyPassword(int id, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Images WHERE ID = @id AND Password = @password";
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@password", password);
            connection.Open();
            var reader = command.ExecuteReader();
            reader.Read();

            var image = new Image
            {
                ID = (int)reader["ID"],
                FileName = (string)reader["FileName"],
                Password = (string)reader["Password"],
                Views = (int)reader["Views"]
            };

            return image;
        }

        public void UpdateView(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Images SET Views = Views + 1 WHERE ID = @id";
            command.Parameters.AddWithValue("@id", id);
        }
    }
}
