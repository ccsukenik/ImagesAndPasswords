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

        public void AddImage(Image image)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Images(FileName, Password, Views)
VALUES(@fileName, @password, 0) SELECT SCOPE_IDENTITY()";
            command.Parameters.AddWithValue("@fileName", image.FileName);
            command.Parameters.AddWithValue("@password", image.Password);
            connection.Open();
            image.ID = (int)(decimal)command.ExecuteScalar(); ;
        }

        public Image GetImage(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT TOP 1 * FROM Images WHERE ID = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = command.ExecuteReader();
            reader.Read();
            if (!reader.Read())
            {
                return null;
            }
            return new Image
            {
                ID = (int)reader["ID"],
                FileName = (string)reader["FileName"],
                Password = (string)reader["Password"],
                Views = (int)reader["Views"]
            };
        }

        public void UpdateViews(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Images SET Views = Views + 1 WHERE ID = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
