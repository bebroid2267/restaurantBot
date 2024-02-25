using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace restaurantBot
{
    public static class DataBase
    {
        private static readonly string connectionString = @"Data Source = C:\Users\кирилл\Desktop\restaurant.db";

        public async static Task<bool> IfExistsUser(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT count(*) FROM users WHERE user_id LIKE '{userId}'";

                object result = await command.ExecuteScalarAsync();
                long count = Convert.ToInt64(result);

                if (count > 0)
                {
                    await connection.CloseAsync();
                    return true;
                }
                else
                {
                    await connection.CloseAsync();
                    return false;
                }

            }

        }

        public async static Task<bool> AddUser(string userId, string regDate, string userName)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                if (await IfExistsUser(userId) == false)
                {
                    command.CommandText = $"INSERT INTO users (id, user_id, reg_date, user_name) values (@user_id, @reg_date, @user_name)";

                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@reg_date", regDate);
                    command.Parameters.AddWithValue("@user_name", userName);
                    
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                    return true;
                }

                await connection.CloseAsync();
                return false;
                
            }

        }
    }
}
