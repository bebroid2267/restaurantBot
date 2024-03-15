using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Types;

namespace restaurantBot
{
    public static class DataBase
    {
        //private static readonly string connectionString = @"Data Source = C:\Users\porka\OneDrive\Рабочий стол\restaurant .db";
        //private static readonly string connectionString = @"Data Source = C:\Users\кирилл\Desktop\restaurant.db";
        private static readonly string connectionString = @"Data Source = /root/restaurantbot/restaurant.db";


        public async static Task<bool> IfExistsUser(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT count(*) FROM users WHERE user_id LIKE '{userId}'";

                long result = (long)await command.ExecuteScalarAsync();

                if (result > 0)
                {
                    connection.Close();
                    return true;
                }
                else
                {
                    connection.Close();
                    return false;       
                }

            }

        }

        public async static Task<bool> AddUser(string userId, string regDate)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                if (await IfExistsUser(userId) == false)
                {
                    command.CommandText = $"INSERT INTO users (user_id, reg_date) values (@user_id, @reg_date)";

                    command.Parameters.AddWithValue("@user_id", userId);
                    command.Parameters.AddWithValue("@reg_date", regDate);

                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    return true;
                }

                connection.Close();
                return false;
                
            }

        }
        public async static Task<bool> IfUserInfoExists(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                string numberPhone = string.Empty;
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT number_phone FROM users WHERE user_id LIKE '{userId}'";

                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                    {
                        numberPhone = reader.GetString(0);
                    }
                    
                }

                if (numberPhone != string.Empty && numberPhone != null)
                {
                    connection.Close();
                    return true;
                }
                else
                {
                    connection.Close();
                    return false;
                }
            }       

        }

        public async static Task<List<string>> GetInfoUser(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string> infoUsers = new List<string>();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT user_name ,number_phone FROM users WHERE user_id LIKE '{userId}'";

                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    infoUsers.Add(reader.GetString(0));
                    infoUsers.Add(reader.GetString(1));
                }
                connection.Close();
                return infoUsers;
            }
        }

        public async static Task AddNameOrNumberPhoneUser(string userId, string whatIs, string numberPhoneOrName)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                if (whatIs == "number")
                {
                    command.CommandText = $"UPDATE users SET number_phone = '{numberPhoneOrName}' WHERE user_id LIKE '{userId}' ";
                }

                else if (whatIs == "name")
                {
                    command.CommandText = $"UPDATE users SET user_name = '{numberPhoneOrName}' WHERE user_id LIKE '{userId}' ";
                }

                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();

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

                long result = (long)await command.ExecuteScalarAsync();
                

                if (result > 0)
                {
                    connection.Close();
                    return true;
                }
                else
                {
                    connection.Close();
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
                connection.Close();
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
                connection.Close();
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

                connection.Close();
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
                        if (!reader.IsDBNull(1))
                        { 
                            allInfoState.CountPeople = reader.GetString(0);
                            allInfoState.ReserveDate = reader.GetString(1);
                            allInfoState.ReserveTime = reader.GetString(2);
                        }
                    }
                }
                else if (ifIdExists == "id")
                {
                    command.CommandText = $"SELECT count_people, date, time, id_table FROM state_reservetion WHERE user_id LIKE '{userId}'";

                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(1))
                        { 
                            allInfoState.CountPeople = reader.GetString(0);
                            allInfoState.ReserveDate = reader.GetString(1);
                            allInfoState.ReserveTime = reader.GetString(2);
                            allInfoState.IdTable = reader.GetInt32(3);
                        }
                    }
                }

                 connection.Close();
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
                command.CommandText = $"DELETE FROM state_reservetion WHERE user_id LIKE '{userId}' ";

                await command.ExecuteNonQueryAsync();

                connection.Close();
            }

        }

        private async static Task<bool> IfExistsReserve(string countPeople, string date)
        {
            using (SqliteConnection connection =  new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT COUNT(*) FROM reservation WHERE count_people LIKE '{countPeople}'" +
                    $"AND  reserve_date LIKE'{date}'";

                long result = (long)await command.ExecuteScalarAsync();
                

                if (result > 0)
                {
                    connection.Close();
                    return true;
                }
                else
                {
                    connection.Close();
                    return false;
                }
            }

        }
        public async static Task<List<string>> GetFreeIdTables(string countPeople, string date, string time)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string> freeIds = new List<string>();
                List<int> busyIds = new List<int>();

                DateTime reservationDate = DateTime.Parse(date);
                DateTime reservationTime = DateTime.Parse(time);
                DateTime reserveEndTime = new DateTime(reservationDate.Year, reservationDate.Month, reservationDate.Day)
                    .AddHours(reservationTime.Hour)
                    .AddMinutes(reservationTime.Minute);

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                List<ReservationInfo> reservationsInfo = await GetBusyIdTableReservation(countPeople, date);

                foreach (var reservation in reservationsInfo)
                {
                    DateTime endTimeReservationDataBase = DateTime.Parse(reservation.ReserveEndTime);

                    if (reserveEndTime <= endTimeReservationDataBase )
                    { 
                        busyIds.Add(reservation.IdTable);
                    }

                }

                if (busyIds.Count == 0)
                {
                    command.CommandText = $"SELECT id_table FROM tables WHERE count_seats LIKE '{countPeople}'";
                }
                else
                { 
                    command.CommandText = $"SELECT id_table FROM tables WHERE count_seats LIKE '{countPeople}' AND id_table NOT IN ({string.Join(",", busyIds)})";
                }
                
                var reader =  command.ExecuteReader();

                while ( reader.Read())
                {
                    freeIds.Add(reader.GetString(0));
                }
                connection.Close();
                return freeIds;
            }

        }

        private async static Task<List<ReservationInfo>> GetBusyIdTableReservation(string countPeople, string date)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<ReservationInfo> idTables = new List<ReservationInfo>();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                if (await IfExistsReserve(countPeople, date))
                {
                    command.CommandText = $"SELECT id_table, reserve_end_time FROM reservation WHERE count_people LIKE '{countPeople}'" +
                    $"AND  reserve_date LIKE'{date}'";
                }
                else
                {
                    connection.Close();
                    return idTables;
                }

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ReservationInfo reservationInfo = new ReservationInfo();

                    reservationInfo.IdTable = Convert.ToInt32(reader.GetString(0));
                    reservationInfo.ReserveEndTime = reader.GetString(1);
                    idTables.Add(reservationInfo);
                }
                connection.Close();
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
                connection.Close();
                return idUser;
            }

        }
        public async static Task AddReservation(int idTable, string reserveDate, string userId, string reserveTime, string countPeople, string confirmYesNo)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();

                command.Connection = connection;

                long idClient = await GetIdClientUser(userId);
                string dateNow = Convert.ToDateTime(reserveDate).ToString("d");

                DateTime date = DateTime.Parse(dateNow);
                DateTime time = DateTime.Parse(reserveTime);
                DateTime reserveEndTime = new DateTime(date.Year,date.Month, date.Day)
                    .AddHours(time.Hour)
                    .AddMinutes(time.Minute);
                

                if (reserveEndTime.Hour <= 19)
                {
                    reserveEndTime = reserveEndTime.AddHours(5);
                }
                else
                { 
                    reserveEndTime = reserveEndTime.AddHours(24 - reserveEndTime.Hour);
                }

                command.CommandText = $"INSERT INTO reservation (id_table, reg_date, reserve_date, id_client, reserve_time, count_people, reserve_end_time, confirmation, user_id)" +
                    $" values (@id_table, @reg_date, @reserve_date, @id_client, @reserve_time, @count_people, @reserve_end_time, @confirmation, @user_id)";

                command.Parameters.AddWithValue("@id_table", idTable);
                command.Parameters.AddWithValue("@reg_date", DateTime.UtcNow.AddHours(3).ToString());
                command.Parameters.AddWithValue("@reserve_date", reserveDate);
                command.Parameters.AddWithValue("@id_client", idClient);
                command.Parameters.AddWithValue("@reserve_time", reserveTime);
                command.Parameters.AddWithValue("@count_people", countPeople);
                command.Parameters.AddWithValue("@reserve_end_time", reserveEndTime.ToString("f"));
                command.Parameters.AddWithValue("@confirmation", confirmYesNo);
                command.Parameters.AddWithValue("@user_id", userId);

                await command.ExecuteNonQueryAsync();
                connection.Close();
            }


        }

        public async static Task<List<ReservationInfo>> GetReservetionsToDate(string date)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<ReservationInfo> infoReservetions = new List<ReservationInfo>();
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT * FROM reservation WHERE reserve_date LIKE '{date}' AND confirmation LIKE 'Yes'";

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ReservationInfo info = new ReservationInfo();
                    info.IdReservation = reader.GetInt32(0);
                    info.IdTable = reader.GetInt32(1);
                    info.RegDate = reader.GetString(2);
                    info.ReserveDate = reader.GetString(3);
                    info.idClient = reader.GetInt32(4);
                    info.ReserveTime = reader.GetString(5);
                    info.CountPeople = reader.GetString(6);
                    info.ReserveEndTime = reader.GetString(7);
                    info.Confirmation = reader.GetString(8);
                    info.UserId = reader.GetString(9);

                    infoReservetions.Add(info);
                }

                connection.Close();
                return infoReservetions;
            }

        }

        public async static Task<ReservationInfo> GetAllInfoReservation(int idReservation)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                ReservationInfo reservation = new ReservationInfo();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT * FROM reservation WHERE id_reservation LIKE '{idReservation}'";

                var reader =  command.ExecuteReader();

                while (reader.Read())
                { 
                    reservation.IdReservation = reader.GetInt32(0);
                    reservation.IdTable = reader.GetInt32(1);
                    reservation.RegDate = reader.GetString(2);
                    reservation.ReserveDate = reader.GetString(3);
                    reservation.idClient = reader.GetInt32(4);
                    reservation.ReserveTime = reader.GetString(5);
                    reservation.CountPeople = reader.GetString(6);
                    reservation.ReserveEndTime = reader.GetString(7);
                    reservation.Confirmation = reader.GetString(8);
                    reservation.UserId = reader.GetString(9);
                }

                connection.Close();
                return reservation;
            }
        }
        public async static Task<List<ReservationInfo>> GetReservetionsUser(string userId)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<ReservationInfo> infoReservetions = new List<ReservationInfo>();   
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT * FROM reservation WHERE user_id LIKE '{userId}' AND confirmation LIKE 'Yes'";

                var reader =  await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ReservationInfo info = new ReservationInfo();
                    info.IdReservation = reader.GetInt32(0);
                    info.IdTable = reader.GetInt32(1);
                    info.RegDate = reader.GetString(2);
                    info.ReserveDate = reader.GetString(3);
                    info.idClient = reader.GetInt32(4);
                    info.ReserveTime = reader.GetString(5);
                    info.CountPeople = reader.GetString(6);
                    info.ReserveEndTime = reader.GetString(7);
                    info.Confirmation = reader.GetString(8);
                    info.UserId = reader.GetString(9);

                    infoReservetions.Add(info);
                }

                connection.Close();
                return infoReservetions;
            }

        }

        public async static Task<List<ReservationInfo>> GetReservationNoYesConfiration(string yesNo)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<ReservationInfo> reservations = new();

                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = $"SELECT id_table, reserve_date, reserve_time, count_people, id_reservation, user_id FROM reservation WHERE confirmation LIKE '{yesNo}'";

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ReservationInfo info = new();
                    info.IdTable = reader.GetInt32(0);
                    info.ReserveDate = reader.GetString(1);
                    info.ReserveTime = reader.GetString(2);
                    info.CountPeople = reader.GetString(3);
                    info.IdReservation = reader.GetInt32(4);
                    info.UserId = reader.GetString(5);

                    reservations.Add(info);
                }

                connection.Close();
                return reservations;
            }

        }
        public async static Task ConfirmReservation(int idReservation)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"UPDATE reservation SET confirmation = 'Yes' WHERE id_reservation LIKE '{idReservation}'";

                await command.ExecuteNonQueryAsync();
                connection.Close();
            }

        }

        public async static Task DeleteReservation(int idReservation)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"DELETE FROM reservation WHERE id_reservation LIKE '{idReservation}' ";

                await command.ExecuteNonQueryAsync();

                connection.Close();
            }

        }

        public async static Task<List<ReservationInfo>> GetCheckStartReservation()
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<ReservationInfo> reservations = new List<ReservationInfo>();
                connection.Open();

                var command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT id_reservation, reserve_date, reserve_time FROM reservation WHERE confirmation LIKE 'Yes'";

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ReservationInfo reservation = new ReservationInfo();
                    reservation.IdReservation = reader.GetInt32(0);
                    reservation.ReserveDate = reader.GetString(1);
                    reservation.ReserveTime = reader.GetString(2);

                    reservations.Add(reservation);
                }
                connection.Close();
                return reservations;

            }

        }



    }
}
