﻿using System.Net;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace restaurantBot
{
    internal class Program
    {
        private static string callbackData = string.Empty;
        private static Admin admin;

        enum Users
        {
            Client = 1,
            Admin
        }
        enum StateReserve
        {
            ChoiceCountPeople,
            ChoiceDateTime,
            ShowTables

        }
        private StateReserve _stateReserve;
        private static Users statusUser;
        static void Main(string[] args)
        {
            var bot = new TelegramBotClient("6717902573:AAFllwaelabWcpQyJI6_BjO8PUOQ1aNWhT4");

            bot.StartReceiving(Update,Error);

            Console.ReadLine();
        }

        private static async Task Update(ITelegramBotClient bot, Update update, CancellationToken cts)
        {
            if (update != null)
            {
                
                if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                {
                    if (update.Message.Chat.Id == 809666698 && admin != null)
                    {
                        statusUser = Users.Admin;
                        admin = new Admin(update.Message.Chat.Id, bot);
                    }
                    else if (statusUser == 0)
                    {
                        statusUser = Users.Client;
                    }
                    await HandleMessage(bot,update.Message);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery.Message.Chat.Id == 809666698 && admin != null)
                    {
                        statusUser = Users.Admin;
                        admin = new Admin(update.Message.Chat.Id, bot);
                    }
                    else if (statusUser == 0)
                    {
                        statusUser = Users.Client;
                    }
                    await HandleCallbackQueary(bot,update.CallbackQuery);
                }

            }

        }
        private static async Task HandleMessage(ITelegramBotClient bot, Message message)
        { 

            if (message.Text != null)
            {
                var msg = message.Text;

                if (msg == "/start") 
                {

                    var userName = message.Chat.FirstName;

                    if (userName != null)
                    {
                        userName = "no username";
                    }

                    await DataBase.AddUser(message.Chat.Id.ToString(),DateTime.UtcNow.ToString(),userName);

                    if (statusUser == Users.Admin)
                    {
                        await bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Приветствую! Вы - Админ",
                            replyMarkup: ButtonMarkupAdmin());
                    }

                    await bot.SendTextMessageAsync(message.Chat.Id,
                        text: "Приветствую! \n Вы можете забронировать столик в нашем ресторане прямо сейчас. " +
                        "\n Или же выберите что вас интересует",
                        replyMarkup: ShowInlineReserveButton());
                }

                else if (message.Text == "Добавить админа" && statusUser == Users.Admin)
                {

                }
                else 
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "Вы");
                }

            }

        }

        private static async Task HandleCallbackQueary(ITelegramBotClient bot, CallbackQuery? callback)
        { 

            if (callback.Data == "bron" || callback.Data == "backdays")
            { 
                await bot.SendTextMessageAsync(
                    chatId: callback.Message.Chat.Id, 
                    text: "Выберите на какое количество людей вы хотите забронировать столик ", 
                    replyMarkup: ShowInlineCountPeopleButton());
            }

            else if (callback.Data.Contains('-') || callback.Data == "backtime")
            {
                if (callback.Data.Contains('-'))
                {
                    callbackData = callback.Data;
                }

                List<string> days = GetDaysInMonth();

                await bot.SendTextMessageAsync(
                    chatId: callback.Message.Chat.Id,
                    text: "Выберите дату бронирования",
                    replyMarkup: ShowInlineDateTimeReservation(days, "days"));

                await DataBase.AddCountPeopleState(callback.Message.Chat.Id.ToString(),callbackData);
            }

            else if (callback.Data.StartsWith("days") || callback.Data == "backTable")
            {
                if (callback.Data.Contains("days"))
                {
                    callbackData = callback.Data;
                }

                List<string> hours = GetTimeDay();

                await bot.SendTextMessageAsync(
                    chatId: callback.Message.Chat.Id, 
                    text: "Выберите время для бронирования", 
                    replyMarkup: ShowInlineDateTimeReservation(hours, "time"));
                
                string dateState = callbackData.Substring(4);
                await DataBase.AddInfoState(callback.Message.Chat.Id.ToString(),dateState, "date");
            }

            else if (callback.Data.Contains("time") || callback.Data == "backFinally")
            {
                if (callback.Data.Contains("time"))
                {
                    callbackData = callback.Data;
                }

                string timeState = callbackData.Substring(4);
                await DataBase.AddInfoState(callback.Message.Chat.Id.ToString(), timeState, "time");

                ReservationInfo infoReresvation = await DataBase.GetAllInfoState(callback.Message.Chat.Id.ToString(), "noId");
                List<string> idsFreeTables = await DataBase.GetFreeIdTables(infoReresvation.CountPeople, infoReresvation.ReserveDate, infoReresvation.ReserveTime);

                await bot.SendTextMessageAsync(
                    chatId: callback.Message.Chat.Id.ToString(),
                    text: "<b>Вот свободные столики на указанное время, дату и количество человек</b>",
                    replyMarkup: ShowInlineTableReservation(idsFreeTables),
                    parseMode: ParseMode.Html);
            }

            else if (callback.Data.Contains("table"))
            {
                string idTable = callback.Data.Substring(6);
                await DataBase.AddInfoState(callback.Message.Chat.Id.ToString(),idTable, "table");

                ReservationInfo infoReservation = await DataBase.GetAllInfoState(callback.Message.Chat.Id.ToString(), "id");

                await bot.SendTextMessageAsync(
                    chatId: callback.Message.Chat.Id, 
                    text: $"Проверьте вашу заявку: \n Количество человек: {infoReservation.CountPeople} " +
                    $"\n Дата: {infoReservation.ReserveDate} \n Время: {infoReservation.ReserveTime} \n Номер столика: {infoReservation.IdTable}",
                    replyMarkup: ShowFinallyReservationButton());
            }

            else if (callback.Data == "sendBron")
            {
                ReservationInfo allInfo = await DataBase.GetAllInfoState(callback.Message.Chat.Id.ToString(),"id");

                await DataBase.AddReservation(
                    allInfo.IdTable, 
                    allInfo.ReserveDate, 
                    callback.Message.Chat.Id.ToString(),
                    allInfo.ReserveTime, 
                    allInfo.CountPeople
                    );

                await bot.SendTextMessageAsync(
                    chatId: callback.Message.Chat.Id, 
                    text: "Ваша бронь отправлена на подтверждение администратору. \n Пожалуйста ожидайте! ");
                

            }

            else if (callback.Data.Contains("cancel"))
            {
                int idResevation = Convert.ToInt32(callback.Data.Substring(7));
                await bot.SendTextMessageAsync(
                    chatId: callback.Message.Chat.Id, 
                    text: "К сожалению, ваша бронь была отменена. \n Попробуйте еще раз.");

                await DataBase.DeleteReservation(idResevation);
                await DataBase.DeleteStateReservation(callback.Message.Chat.Id.ToString());
            }

            else if (callback.Data.Contains("change"))
            {

            }

            else if (callback.Data.Contains("accept"))
            {

            }
                    

        }



        private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static InlineKeyboardMarkup ShowInlineReserveButton()
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Забронировать","bron")
            });
            return new InlineKeyboardMarkup (buttonRows);
        }

        private static InlineKeyboardMarkup ShowInlineCountPeopleButton()
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "1 - 2", "1-2"),
                InlineKeyboardButton.WithCallbackData(text: "3 - 4", "3-4"),
                InlineKeyboardButton.WithCallbackData(text: "5 - 6", "5-6"),
                InlineKeyboardButton.WithCallbackData(text: "◀️", "backCountPeople")

            });
            return new InlineKeyboardMarkup(buttonRows);
        }

        private static InlineKeyboardMarkup ShowInlineDateTimeReservation(List<string> dateTime, string infoDateTime)
        { 
            var inlineButtons = new List<List<InlineKeyboardButton>>();
            List<string> daysInMonth = dateTime;

            var groups = daysInMonth.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 4)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            foreach (var group in groups)
            {
                var buttonsRow = new List<InlineKeyboardButton>();

                foreach (var day in group)
                {


                    buttonsRow.Add(InlineKeyboardButton.WithCallbackData(day.ToString(), $"{infoDateTime}" + day.ToString()));
                }

                

                inlineButtons.Add(buttonsRow);
            }
            var buttonRowBack = new List<InlineKeyboardButton>();
            buttonRowBack.Add(InlineKeyboardButton.WithCallbackData(text: "◀️", $"back{infoDateTime}"));

            inlineButtons.Add(buttonRowBack);

            return new InlineKeyboardMarkup(inlineButtons);

        }
        private static InlineKeyboardMarkup ShowInlineTableReservation(List<string> tables)
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            foreach (var table in tables)
            {
                buttonRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: $"Столик N {table}", $"table {table}" )
                });
                
                    
            }
            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "◀️", "backTable")
            });
                
            return new InlineKeyboardMarkup(buttonRows);
        }

        private static InlineKeyboardMarkup ShowFinallyReservationButton()
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "✅","sendBron"),
                InlineKeyboardButton.WithCallbackData(text: "◀️", "backFinally")
            });
            return new InlineKeyboardMarkup(buttonRows);
        }

        private static List<string> GetTimeDay()
        {
            List<string> hours = new List<string>();
            DateTime time = DateTime.UtcNow.AddHours(3);
            DateTime startDay = DateTime.Now.Date;

            DateTime finallyHour = startDay.AddDays(1).AddSeconds(-1);

            for (DateTime currentHour = time; currentHour <= finallyHour; 
                currentHour = currentHour.AddHours(1))
            {
                hours.Add(new DateTime(currentHour.Year,currentHour.Month,currentHour.Day,  currentHour.Hour, 0, 0).ToString("t"));
            }
            return hours;
        }
        private static List<string> GetDaysInMonth()
        {
            List<string> days = new List<string>();
            DateTime date = DateTime.UtcNow;
            int lastDayMonth = DateTime.DaysInMonth(date.Year, date.Month);
            int countDays = 0;

                for (int i = date.Day; i <= lastDayMonth; i++)
                {
                    days.Add(date.AddDays(countDays).ToString("M"));
                    countDays++;
                }
                return days;

        }
        public static ReplyKeyboardMarkup ButtonMarkupAdmin()
        {
            ReplyKeyboardMarkup reply = new(new[]
            {
                new KeyboardButton( "Добавить админа" )
            })
            {
                ResizeKeyboard = true
            };
            return reply;
        }

    }
}
