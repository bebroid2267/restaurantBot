using System.Net;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace restaurantBot
{
    public class Program
    {
        public static string callbackData = string.Empty;
        public static readonly long userIdAdmin = 809666698;
        public static Admin admin;
        public static string userIdChangedReservationByAdmin;

        enum StateReserve
        {
            ChoiceCountPeople,
            ChoiceDateTime,
            ShowTables
        }

        static void Main(string[] args)
        {
            var bot = new TelegramBotClient("6717902573:AAFllwaelabWcpQyJI6_BjO8PUOQ1aNWhT4");

            admin = new Admin(bot);
            bot.StartReceiving(Update,Error);

            Console.ReadLine();
        }

        private static async Task Update(ITelegramBotClient bot, Update update, CancellationToken cts)
        {
            if (update != null)
            {

                if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                {
                    await HandleMessage(bot, update.Message);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallbackQueary(bot, update.CallbackQuery);
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

                    if (message.Chat.Id == userIdAdmin)
                    {
                        await bot.SendTextMessageAsync(message.Chat.Id,
                        text: "Приветствую! \n Вы можете просмотреть есть ли сейчас не подтвержденные заявки на бронь \n Для этого нажмите кнопку снизу: ",
                        replyMarkup: ShowInlineCheckReservationNoConfirmAdminButton());
                    }

                    else
                    { 
                        await bot.SendTextMessageAsync(message.Chat.Id,
                            text: "Приветствую! \n Вы можете забронировать столик в нашем ресторане прямо сейчас. " +
                            "\n Или же выберите что вас интересует",
                            replyMarkup: ShowInlineReserveButton());
                    }
                }

                else 
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "Вы");
                }

            }

        }

        private static async Task HandleCallbackQueary(ITelegramBotClient bot, CallbackQuery? callback)
        {
            if (callback.Message.Chat.Id == userIdAdmin && userIdChangedReservationByAdmin != null)
            {
                await admin.HandleCallbackQuearyAdmin(bot, callback, userIdAdmin, Convert.ToInt64(userIdChangedReservationByAdmin));
            }
            else
            {



                if (callback.Data == "bron" || callback.Data == "backdays")
                {
                    await bot.EditMessageTextAsync(
                        chatId: callback.Message.Chat.Id,
                        messageId: callback.Message.MessageId,
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

                    await bot.EditMessageTextAsync(
                        chatId: callback.Message.Chat.Id,
                        messageId: callback.Message.MessageId,
                        text: "Выберите дату бронирования",
                        replyMarkup: ShowInlineDateTimeReservation(days, "days"));

                    await DataBase.AddCountPeopleState(callback.Message.Chat.Id.ToString(), callbackData);
                }

                else if (callback.Data.StartsWith("days") || callback.Data == "backTable")
                {

                    if (callback.Data.Contains("days"))
                    {
                        callbackData = callback.Data;
                    }
                    List<string> hours = new List<string>();

                    if (callbackData.Length > 4)
                    {
                        string dateState = callbackData.Substring(4);
                        dateState = Convert.ToDateTime(dateState).ToString("D");
                        await DataBase.AddInfoState(callback.Message.Chat.Id.ToString(), dateState, "date");
                        string dateReservation = Convert.ToDateTime(dateState).ToString("d");
                        hours = GetTimeDay(dateReservation);
                    }
                    else
                    {
                        string info = await DataBase.GetInfoState(callback.Message.Chat.Id.ToString(), "date");
                        string dateReservation = Convert.ToDateTime(info).ToString("d");
                        hours = GetTimeDay(dateReservation);
                    }

                    await bot.EditMessageTextAsync(
                        chatId: callback.Message.Chat.Id,
                        messageId: callback.Message.MessageId,
                        text: "Выберите время для бронирования",
                        replyMarkup: ShowInlineDateTimeReservation(hours, "time"));

                    
                }

                else if (callback.Data.Contains("time") || callback.Data == "backFinally")
                {
                    if (callback.Data.Contains("time"))
                    {
                        callbackData = callback.Data;
                    }
                    if (callbackData != null && callbackData != string.Empty)
                    {
                        string timeState = callbackData.Substring(4);
                        await DataBase.AddInfoState(callback.Message.Chat.Id.ToString(), timeState, "time");

                        ReservationInfo infoReresvation = await DataBase.GetAllInfoState(callback.Message.Chat.Id.ToString(), "noId");
                        if (infoReresvation.CountPeople == string.Empty)
                        {
                            await bot.EditMessageTextAsync(
                                chatId: callback.Message.Chat.Id.ToString(),
                                messageId: callback.Message.MessageId,
                                text: "<b>Возникла ошибка при выборе количества персон. \n Пожалуйста, заполните бронь заново</b>",
                                parseMode: ParseMode.Html);
                            return;
                        }

                        List<string> idsFreeTables = await DataBase.GetFreeIdTables(infoReresvation.CountPeople, infoReresvation.ReserveDate, infoReresvation.ReserveTime);

                        await bot.EditMessageTextAsync(
                            chatId: callback.Message.Chat.Id.ToString(),
                            messageId: callback.Message.MessageId,
                            text: "<b>Вот свободные столики на указанное время, дату и количество человек</b>",
                            replyMarkup: ShowInlineTableReservation(idsFreeTables),
                            parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(callback.Message.Chat.Id, "Ошибка");
                    }
                    

                    
                }

                else if (callback.Data.Contains("table"))
                {
                    string idTable = callback.Data.Substring(6);
                    await DataBase.AddInfoState(callback.Message.Chat.Id.ToString(), idTable, "table");

                    ReservationInfo infoReservation = await DataBase.GetAllInfoState(callback.Message.Chat.Id.ToString(), "id");

                    await bot.EditMessageTextAsync(
                        chatId: callback.Message.Chat.Id,
                        messageId: callback.Message.MessageId,
                        text: $"Проверьте вашу заявку: \n Количество человек: {infoReservation.CountPeople} " +
                        $"\n Дата: {infoReservation.ReserveDate} \n Время: {infoReservation.ReserveTime} \n Номер столика: {infoReservation.IdTable}",
                        replyMarkup: ShowFinallyReservationButton());
                }

                else if (callback.Data == "sendBron")
                {

                    ReservationInfo allInfo = await DataBase.GetAllInfoState(callback.Message.Chat.Id.ToString(), "id");

                    await DataBase.AddReservation(
                        allInfo.IdTable,
                        allInfo.ReserveDate,
                        callback.Message.Chat.Id.ToString(),
                        allInfo.ReserveTime,
                        allInfo.CountPeople
                        );

                    await bot.EditMessageTextAsync(
                        chatId: callback.Message.Chat.Id,
                        messageId: callback.Message.MessageId,
                        text: "Ваша бронь отправлена на подтверждение администратору. \n Пожалуйста ожидайте! ");

                    await admin.SendReservationForConfirationToAdmin();

                }

                else if (callback.Data.Contains("cancel"))
                {
                    int idResevation = Convert.ToInt32(callback.Data.Substring(7));

                    ReservationInfo info = await DataBase.GetAllInfoReservation(idResevation);

                    await bot.EditMessageTextAsync(
                        chatId: userIdAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Вы успешно отменили бронь клиента");

                    await admin.SendStatusReservationToClient(bot, info.UserId, info, "cancel");

                    await DataBase.DeleteReservation(idResevation);
                    await DataBase.DeleteStateReservation(callback.Message.Chat.Id.ToString());
                }

                else if (callback.Data.Contains("change"))
                {
                    await bot.SendTextMessageAsync(
                        chatId: callback.Message.Chat.Id,
                        text: "Выберите на какое количество людей вы хотите забронировать столик клиенту",
                        replyMarkup: ShowInlineCountPeopleButton());

                    int idReservation = Convert.ToInt32(callback.Data.Substring(7));

                    ReservationInfo infoReservations = await DataBase.GetAllInfoReservation(idReservation);

                    userIdChangedReservationByAdmin = infoReservations.UserId;

                    await DataBase.DeleteReservation(idReservation);
                    await DataBase.DeleteStateReservation(infoReservations.UserId);
                }

                else if (callback.Data.Contains("accept"))
                {
                    int idReservation = Convert.ToInt32(callback.Data.Substring(7));

                    ReservationInfo info = await DataBase.GetAllInfoReservation(idReservation);

                    await bot.EditMessageTextAsync(
                        chatId: userIdAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Вы успешно подтвердили бронь клиента");

                    await admin.SendStatusReservationToClient(bot, info.UserId, info, "accept");

                    await DataBase.ConfirmReservation(Convert.ToInt32(callback.Data.Substring(7)));
                }

                else if (callback.Data == "CheckReserveAdmin")
                {
                    List<ReservationInfo> infoResevations = await DataBase.GetReservationNoConfiration();

                    if (infoResevations.Count > 0)
                    { 
                        foreach (var reservation in infoResevations)
                        {
                            await bot.SendTextMessageAsync(
                                chatId: callback.Message.Chat.Id,
                                text: $"<b> Бронь на подтверждение {reservation.IdReservation}!  \n  Номер столика: {reservation.IdTable} \n " +
                                $"Дата: {reservation.RegDate} \n Время: {reservation.ReserveTime} Количество мест: {reservation.CountPeople}</b>",
                                replyMarkup: admin.ShowInlineResevationToAnswerAdminButtons(reservation.IdReservation.ToString()),
                                parseMode: ParseMode.Html
                                );
                        }
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(callback.Message.Chat.Id, "В данный момент заявки требующие подтверждения - отсутствуют");
                    }
                }

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

        public static InlineKeyboardMarkup ShowInlineCountPeopleButton()
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "1 - 2", "1-2"),
                InlineKeyboardButton.WithCallbackData(text: "3 - 4", "3-4"),
                InlineKeyboardButton.WithCallbackData(text: "5 - 6", "5-6")

            });
            return new InlineKeyboardMarkup(buttonRows);
        }

        public static InlineKeyboardMarkup ShowInlineDateTimeReservation(List<string> dateTime, string infoDateTime)
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
                    if (infoDateTime == "time")
                    {
                        buttonsRow.Add(InlineKeyboardButton.WithCallbackData(day, $"{infoDateTime}" + day.ToString()));
                    }
                    else
                    {
                        string textDay = Convert.ToDateTime(day).ToString("M");
                        buttonsRow.Add(InlineKeyboardButton.WithCallbackData(textDay, $"{infoDateTime}" + day.ToString()));
                    }
                }
                inlineButtons.Add(buttonsRow);
            }
            var buttonRowBack = new List<InlineKeyboardButton>();
            buttonRowBack.Add(InlineKeyboardButton.WithCallbackData(text: "◀️", $"back{infoDateTime}"));

            inlineButtons.Add(buttonRowBack);

            return new InlineKeyboardMarkup(inlineButtons);

        }
        public static InlineKeyboardMarkup ShowInlineTableReservation(List<string> tables)
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

        public static InlineKeyboardMarkup ShowFinallyReservationButton()
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "✅","sendBron"),
                InlineKeyboardButton.WithCallbackData(text: "◀️", "backFinally")
            });
            return new InlineKeyboardMarkup(buttonRows);
        }
        public static InlineKeyboardMarkup ShowInlineCheckReservationNoConfirmAdminButton()
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Брони без подтверждения","CheckReserveAdmin")
            });
            return new InlineKeyboardMarkup(buttonRows);
        }

        public static List<string> GetTimeDay(string dayReservation)
        {
            DateTime day = Convert.ToDateTime(dayReservation);
            List<string> hours = new List<string>();

            DateTime startDay = day;

            DateTime finallyHour = startDay.AddDays(1).AddSeconds(-1);

            if (day == DateTime.UtcNow)
            {
                DateTime time = DateTime.UtcNow.AddHours(3);

                for (DateTime currentHour = time; currentHour <= finallyHour;
                currentHour = currentHour.AddHours(1))
                {
                    hours.Add(new DateTime(currentHour.Year, currentHour.Month, currentHour.Day, currentHour.Hour, 0, 0).ToString("t"));
                }
            }
            else
            {
                DateTime time = day.AddHours(9);

                for (DateTime currentHour = time; currentHour <= finallyHour;
               currentHour = currentHour.AddHours(1))
                {
                    hours.Add(new DateTime(currentHour.Year, currentHour.Month, currentHour.Day, currentHour.Hour, 0, 0).ToString("t"));
                }
            }

            
            return hours;
        }
        public static List<string> GetDaysInMonth()
        {
            List<string> days = new List<string>();
            DateTime date = DateTime.UtcNow;
            int lastDayMonth = DateTime.DaysInMonth(date.Year, date.Month);
            int countDays = 0;

                for (int i = date.Day; i <= lastDayMonth; i++)
                {
                    days.Add(date.AddDays(countDays).ToString("f"));
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
