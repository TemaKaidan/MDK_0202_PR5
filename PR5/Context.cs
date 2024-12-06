using MySql.Data.MySqlClient;
using System;

namespace PR5
{
    public class Context
    {
        private readonly string ConnectionString = "Server=localhost;database=pr5;uid=root;port=3303";

        public bool AuthenticateUser(string username, string password, out bool isBlackListed)
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username = @username";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count == 0)
                {
                    query = "INSERT INTO users (username, password, is_blacklisted) VALUES (@username, @password, 0)";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.ExecuteNonQuery();
                    isBlackListed = false;
                    return true;
                }
                else
                {
                    query = "SELECT is_blacklisted FROM users WHERE username = @username AND password = @password";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isBlackListed = reader.GetBoolean("is_blacklisted");
                            return true;
                        }
                        else
                        {
                            isBlackListed = false;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                isBlackListed = false;
                return false;
            }
        }

        public void AddToBlacklist(string username)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string query = "UPDATE users SET is_blacklisted = 1 WHERE username = @username";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Клиент {username} добавлен в черный список.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Клиент {username} не найден.");
            }
        }

        public void RemoveFromBlacklist(string username)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string query = "UPDATE users SET is_blacklisted = 0 WHERE username = @username";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Клиент {username} удален из черного списка.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Клиент {username} не найден или его уже нет в черном списке.");
            }
        }

        public void ShowBlacklist()
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string query = "SELECT username FROM users WHERE is_blacklisted = 1";
            MySqlCommand command = new MySqlCommand(query, connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Заблокированные:");
                while (reader.Read())
                {
                    Console.WriteLine(reader["username"]);
                }
            }
        }
    }
}