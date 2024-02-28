﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace restaurantBot
{
    public static class DataBase
    {
        //private static readonly string connectionString = @"Data Source = C:\Users\porka\OneDrive\Рабочий стол\restaurant.db";
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
                    command.CommandText = $"INSERT INTO users (user_id, reg_date, user_name) values (@user_id, @reg_date, @user_name)";

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
        private async static Task<bool> IfStateEsixsts(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT COUNT(*) FROM state_reservetion WHERE user_id LIKE '{userId}'";

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

        public async static Task AddCountPeopleState(string userId,string countPeople)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                if (await IfStateEsixsts(userId))
                {
                    command.CommandText = $"UPDATE state_reservetion SET count_people = '{countPeople}' WHERE user_id LIKE '{userId}'";
                }
                else
                {
                    command.CommandText = $"INSERT INTO state_reservetion (user_id, count_people) values (@user_id, @count_people)";
                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@count_people", countPeople);
                }

                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }

        }
        public async static Task AddInfoState(string userId, string info, string whatIs)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                if (whatIs == "date")
                {
                    command.CommandText = $"UPDATE state_reservetion SET date = '{info}' WHERE user_id LIKE '{userId}'";
                }
                else if (whatIs == "time")
                {
                    command.CommandText = $"UPDATE state_reservetion SET time = '{info}' WHERE user_id LIKE '{userId}'";
                }
                else if (whatIs == "table")
                {
                    command.CommandText = $"UPDATE state_reservetion SET id_table = '{info}' WHERE user_id LIKE '{userId}'";
                }

                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }

        }

        public async static Task<string> GetInfoState(string userId, string info)
        {
            string infoState = string.Empty;

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                switch (info)
                {
                    case "countPeople":
                        command.CommandText = $"select count_people FROM state_reservetion WHERE user_id LIKE '{userId}'";
                        break;
                    case "date":
                        command.CommandText = $"select date FROM state_reservetion WHERE user_id LIKE '{userId}'";
                        break;
                    case "time":
                        command.CommandText = $"select time FROM state_reservetion WHERE user_id LIKE '{userId}'";
                        break;
                    default:
                        break;
                }

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    infoState = reader.GetString(0);
                }

                await connection.CloseAsync();
                return infoState;

                
            }
        }

        public async static Task<List<string>> GetAllInfoState(string userId, string ifIdExists)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string> allInfoState = new List<string>();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                if (ifIdExists == "noId")
                {
                    command.CommandText = $"SELECT count_people, date, time FROM state_reservetion WHERE user_id LIKE '{userId}'";

                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        allInfoState.Add(reader.GetString(0));
                        allInfoState.Add(reader.GetString(1));
                        allInfoState.Add(reader.GetString(2));
                    }
                }
                else if (ifIdExists == "id")
                {
                    command.CommandText = $"SELECT count_people, date, time, id_table FROM state_reservetion WHERE user_id LIKE '{userId}'";

                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        allInfoState.Add(reader.GetString(0));
                        allInfoState.Add(reader.GetString(1));
                        allInfoState.Add(reader.GetString(2));
                        allInfoState.Add(reader.GetString(3));
                    }
                }

                await connection.CloseAsync();
                return allInfoState;


            }

        }

        private async static Task<bool> IfExistsReserve(string countPeople, string date, string time)
        {
            using (SqliteConnection connection =  new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT COUNT(*) FROM reservation WHERE count_people LIKE '{countPeople}'" +
                    $"AND  reserve_date LIKE'{date}' AND reserve_time LIKE '{time}'";

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
        public async static Task<List<string>> GetFreeIdTables(string countPeople, string date, string time)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string> freeIds = new List<string>();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                List<string> idsBusyTable = await GetBusyIdTableReservation(countPeople, date, time);

                if (idsBusyTable.Count == 0)
                {
                    command.CommandText = $"SELECT id_table FROM tables WHERE count_seats LIKE '{countPeople}'";
                }
                else
                { 
                    command.CommandText = $"SELECT id_table FROM tables WHERE count_seats LIKE '{countPeople}' AND id_table NOT IN '{string.Join(",",idsBusyTable)}'";
                }
                
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    freeIds.Add(reader.GetString(0));
                }
                await connection.CloseAsync();
                return freeIds;
            }

        }

        private async static Task<List<string>> GetBusyIdTableReservation(string countPeople, string date, string time)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string> idTables = new List<string>();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                if (await IfExistsReserve(countPeople, date, time))
                {
                    command.CommandText = $"SELECT id_table FROM reservation WHERE count_people LIKE '{countPeople}'" +
                    $"AND  reserve_date LIKE'{date}' AND reserve_time LIKE '{time}'";
                }
                else
                {
                    await connection.CloseAsync();
                    return idTables;
                }

                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    idTables.Add(reader.GetString(0));
                }
                await connection.CloseAsync();
                return idTables;
            }

        }



    }
}
