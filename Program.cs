using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApp93
{

    //Написать программу которая будет принимать у пользователя название БД и создать ее.
    //    После чего запрашивать у пользователя Название таблицы и создать ее с полями которые нужны пользователю. 
    //    После чего дать возможность ему наполнить ее)

    class Program
    {
        private const string connectionString = @"Data Source = WIN-RO38MGBKCE5; Initial Catalog = master; Trusted_Connection=True; TrustServerCertificate= True";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Введите имя базы данных:");
            string dbName = Console.ReadLine();

            await CreateDatabaseAsync(dbName);
            Console.WriteLine("Введите имя таблицы:");
            string tableName = Console.ReadLine();
            await CreateData(dbName, tableName);
            Console.WriteLine("Какое поле вы хотите заполнить? (например: Name или Age)");
            string field = Console.ReadLine();
            Console.WriteLine($"Введите значение для {field}:");
            string value = Console.ReadLine();
            await InsertData(dbName, tableName, field, value);
            Console.WriteLine("База данных с таблицей и ее наполнением успешно создана");
        }

        public static async Task CreateDatabaseAsync(string databaseName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand
                {
                    CommandText = $"CREATE DATABASE [{databaseName}]",
                    Connection = connection
                };

                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"База данных {databaseName} успешно создана.");
            }
        }

        public static async Task CreateData(string databaseName, string tableName)
        {
            string specificConnectionString = connectionString.Replace("Initial Catalog = master", $"Initial Catalog = {databaseName}");

            Console.WriteLine("Сколько полей вы хотите добавить? максимальное количество 2 поля ");
            if (!int.TryParse(Console.ReadLine(), out int fieldCount) || fieldCount <= 0)
            {
                Console.WriteLine("Недопустимое количество полей!");
                return;
            }

            StringBuilder commandText = new StringBuilder($"CREATE TABLE [{tableName}] (");

            Dictionary<string, string> availableFields = new Dictionary<string, string>
    {
        { "1", "Name NVARCHAR(100)" },
        { "2", "Age INT" }
    };

            for (int i = 0; i < fieldCount; i++)
            {
                Console.WriteLine("Выберите поле:");
                foreach (var field in availableFields)
                {
                    Console.WriteLine($"{field.Key}. {field.Value}");
                }

                string selectedFieldKey = Console.ReadLine();
                if (availableFields.ContainsKey(selectedFieldKey))
                {
                    commandText.Append($"{availableFields[selectedFieldKey]}");

                    if (i < fieldCount - 1)
                        commandText.Append(", ");

                    availableFields.Remove(selectedFieldKey);
                }
                else
                {
                    Console.WriteLine("Выбрано недопустимое поле!");
                    i--;
                }
            }

            commandText.Append(")");

            using (SqlConnection connection = new SqlConnection(specificConnectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand
                {
                    CommandText = commandText.ToString(),
                    Connection = connection
                };

                await command.ExecuteNonQueryAsync();

                Console.WriteLine($"Таблица {tableName} успешно создана.");
            }
        }


        public static bool IsValidName(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z0-9_]+$");
        }

        public static async Task InsertData(string databaseName, string tableName, string field, string value)
        {
            string specificConnectionString = connectionString.Replace("Initial Catalog = master", $"Initial Catalog = {databaseName}");

            string sqlExpression = $"INSERT INTO [{tableName}]({field}) VALUES (@Value)";

            using (SqlConnection connection = new SqlConnection(specificConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    SqlCommand command = new SqlCommand(sqlExpression, connection);

                    command.Parameters.AddWithValue("@Value", value);

                    int number = await command.ExecuteNonQueryAsync();

                    Console.WriteLine($"Добавлено объектов {number}");
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Произошла ошибка при выполнении SQL запроса: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
                }
            }
        }
    }
}
