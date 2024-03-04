using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;

namespace restaurantBot
{
    public static class DataBase
    {
        private static readonly string connectionString = @"Data Source = C:\Users\porka\OneDrive\Рабочий стол\restaurant.db";
        //private static readonly string connectionString = @"Data Source = C:\Users\кирилл\Desktop\restaurant.db";


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

        public async static Task<ReservationInfo> GetAllInfoState(string userId, string ifIdExists)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                ReservationInfo allInfoState = new ReservationInfo();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                if (ifIdExists == "noId")
                {
                    command.CommandText = $"SELECT count_people, date, time FROM state_reservetion WHERE user_id LIKE '{userId}'";

                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    { 
                        allInfoState.CountPeople = reader.GetString(0);
                        allInfoState.ReserveDate = reader.GetString(1);
                        allInfoState.ReserveTime = reader.GetString(2);
                    }
                }
                else if (ifIdExists == "id")
                {
                    command.CommandText = $"SELECT count_people, date, time, id_table FROM state_reservetion WHERE user_id LIKE '{userId}'";

                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    { 
                        allInfoState.CountPeople = reader.GetString(0);
                        allInfoState.ReserveDate = reader.GetString(1);
                        allInfoState.ReserveTime = reader.GetString(2);
                        allInfoState.IdTable = reader.GetInt32(3);
                    }
                }

                await connection.CloseAsync();
                return allInfoState;


            }

        }
        public async static Task DeleteStateReservation(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"DELETE FROM state_reservetion WHERE user_id LIKE '{userId}' LIMIT 1";

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
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

        private async static Task<long> GetIdClientUser(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                long idUser = 0;
                connection.Open();

                var command = new SqliteCommand();
                command.Connection= connection;
                command.CommandText = $"SELECT id_client FROM users WHERE user_id LIKE '{userId}'";

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    idUser = reader.GetInt64(0);
                }
                await connection.CloseAsync();
                return idUser;
            }

        }
        public async static Task AddReservation(int idTable, string reserveDate, string userId, string reserveTime, string countPeople)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                connection.ConnectionString = connectionString;

                long idClient = await GetIdClientUser(userId);
                DateTime dateNow = DateTime.UtcNow;
                DateTime reserveEndTime = Convert.ToDateTime(reserveTime);
                
                if (reserveEndTime.Hour <= 19)
                {
                    reserveEndTime.AddHours(5);
                }
                else
                { 
                    reserveEndTime.AddHours(24 - reserveEndTime.Hour);
                }

                command.CommandText = $"INSERT INTO reservation (id_table, reg_date, reserve_date, id_client, reserve_time, count_people, reserve_end_time)" +
                    $" values (@id_table, @reg_date, @reserve_date, @id_client, @count_people, @reserve_end_time, @confirmation)";

                command.Parameters.AddWithValue("@id_table", idTable);
                command.Parameters.AddWithValue("@reg_date", dateNow.ToString());
                command.Parameters.AddWithValue("@reserve_date", reserveDate);
                command.Parameters.AddWithValue("@id_client", idClient);
                command.Parameters.AddWithValue("@reserve_time", reserveTime);
                command.Parameters.AddWithValue("@count_people", countPeople);
                command.Parameters.AddWithValue("@reserve_end_time", reserveEndTime);
                command.Parameters.AddWithValue("@confirmation", "no");

                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }


        }

        public async static Task<List<ReservationInfo>> GetReservationNoConfiration()
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<ReservationInfo> reservations = new();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT id_table, reserve_date, reserve_time, count_people, id_reservation FROM reservation WHERE confirmation LIKE 'no'";

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ReservationInfo info = new();
                    info.IdTable = reader.GetInt32(0);
                    info.ReserveDate = reader.GetString(1);
                    info.ReserveTime = reader.GetString(2);
                    info.CountPeople = reader.GetString(3);
                    info.IdReservation = reader.GetInt32(4);

                    reservations.Add(info);
                }

                await connection.CloseAsync();

                return reservations;
            }

        }

        public async static Task DeleteReservation(int idReservation)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"DELETE FROM reservation WHERE id_reservation LIKE '{idReservation}' LIMIT 1";

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }

        }

        public async static Task<bool> IfExistsAdmin(long userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT COUNT(*) from admins WHERE user_id LIKE '{userId}'";

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
        public async static Task AddAdmin(long userId, string name)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                if (await IfExistsAdmin(userId))
                {
                    command.CommandText = $"INSERT INTO admins (user_id, name) values (@user_id, @name)";

                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@name", name);

                    await command.ExecuteNonQueryAsync();
                    
                }

                await connection.CloseAsync();

            }


        }

    }
}
