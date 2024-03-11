using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Dynamic;


namespace restaurantBot
{
    public class Admin
    { 
        public Admin(ITelegramBotClient bot)
        { 
            this.bot = bot;
        }

        public async Task SendReservationForConfirationToAdmin()
        {
            List<ReservationInfo> infoReservation = await DataBase.GetReservationNoYesConfiration("no");
            

            foreach (var reservation in infoReservation)
            {
                List<string> infoUser =  await DataBase.GetInfoUser(reservation.UserId);

                   await bot.SendTextMessageAsync(
                chatId: userId,
                text: $"<b>Пришла бронь на подтверждение! \n Клиент: {infoUser[0]} \n Телефон: {infoUser[1]} \n  Номер столика: {reservation.IdTable} \n " +
                $"Дата: {reservation.ReserveDate} \n Время: {reservation.ReserveTime} \n Количество мест: {reservation.CountPeople}</b>",
                replyMarkup: ShowInlineResevationToAnswerAdminButtons(reservation.IdReservation.ToString()),
                parseMode: ParseMode.Html
                );

            }
        }
        public async Task SendStatusReservationToClient(ITelegramBotClient bot, string userId, ReservationInfo info, string status)
        {
            switch (status)
            {
                case "cancel":
                    await bot.SendTextMessageAsync(
                chatId: userId,
                text: $"К сожалению, ваша бронь на столик номер {info.IdTable} была отменена. ");
                    break;
                    
                case "change":
                    await bot.SendTextMessageAsync(
                        chatId: userId,
                        text: $"Ваша бронь была изменена администратором. Данные новой брони: \n " +
                        $"Количество человек: {info.CountPeople} \n Дата: {info.ReserveDate} \n " +
                        $"Время: {info.ReserveTime} \n Столик {info.IdTable} ");
                    break;

                case "accept":
                    await bot.SendTextMessageAsync(
                        chatId: userId,
                        text: $"Ваша бронь на столик номер {info.IdTable} была подтверждена администратором. \n Наш адрес:");
                    break;
                default:
                    break;
            }
            

        }

        public async Task HandleCallbackQuearyAdmin(ITelegramBotClient bot, CallbackQuery? callback, long idAdmin, long userId)
        { 
            if (callback.Data == "bron" || callback.Data == "backdays")
            {
                await bot.EditMessageTextAsync(
                    chatId: idAdmin,
                    messageId: callback.Message.MessageId,
                    text: "Выберите на какое количество людей вы хотите забронировать столик ",
                    replyMarkup: Program.ShowInlineCountPeopleButton());
            }

            else if (callback.Data.Contains('-') || callback.Data == "backtime")
            {

                if (callback.Data.Contains('-'))
                {
                    Program.callbackData = callback.Data;
                    await DataBase.AddCountPeopleState(userId.ToString(), Program.callbackData);
                }

                List<string> days = Program.GetDaysInMonth();

                await bot.EditMessageTextAsync(
                    chatId: idAdmin,
                    messageId: callback.Message.MessageId,
                    text: "Выберите дату бронирования",
                    replyMarkup: Program.ShowInlineDateTimeReservation(days, "days"));

                
            }

            else if (callback.Data.StartsWith("days") || callback.Data == "backTable")
            {
                if (callback.Data.Contains("days"))
                {
                    Program.callbackData = callback.Data;
                }
                List<string> hours = new List<string>();

                string dateState = Program.callbackData.Substring(4);
                dateState = Convert.ToDateTime(dateState).ToString("D");
                await DataBase.AddInfoState(userId.ToString(), dateState, "date");
                string dateReservation = Convert.ToDateTime(dateState).ToString("d");
                hours = Program.GetTimeDay(dateReservation);

                await bot.EditMessageTextAsync(
                    chatId: idAdmin,
                    messageId: callback.Message.MessageId,
                    text: "Выберите время для бронирования",
                    replyMarkup: Program.ShowInlineDateTimeReservation(hours, "time"));
            }

            else if (callback.Data.Contains("time") || callback.Data == "backFinally")
            {
                if (callback.Data.Contains("time"))
                {
                    Program.callbackData = callback.Data;
                }

                string timeState = Program.callbackData.Substring(4);
                await DataBase.AddInfoState(userId.ToString(), timeState, "time");

                ReservationInfo infoReresvation = await DataBase.GetAllInfoState(userId.ToString(), "noId");
                if (infoReresvation != null && infoReresvation.CountPeople != null && infoReresvation.ReserveDate != null)
                { 
                    List<string> idsFreeTables = await DataBase.GetFreeIdTables(infoReresvation.CountPeople, infoReresvation.ReserveDate, infoReresvation.ReserveTime);

                    await bot.EditMessageTextAsync(
                        chatId: idAdmin.ToString(),
                        messageId: callback.Message.MessageId,
                        text: "<b>Вот свободные столики на указанное время, дату и количество человек</b>",
                        replyMarkup: Program.ShowInlineTableReservation(idsFreeTables),
                        parseMode: ParseMode.Html);
                }
                else
                {
                    await bot.EditMessageTextAsync(
                        chatId:idAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Произошла ошибка в изменении брони");
                }
            }

            else if (callback.Data.Contains("table"))
            {
                string idTable = callback.Data.Substring(6);
                await DataBase.AddInfoState(userId.ToString(), idTable, "table");

                ReservationInfo infoReservation = await DataBase.GetAllInfoState(userId.ToString(), "id");

                await bot.EditMessageTextAsync(
                    chatId: idAdmin,
                    messageId: callback.Message.MessageId,
                    text: $"Проверьте вашу заявку: \n Количество человек: {infoReservation.CountPeople} " +
                    $"\n Дата: {infoReservation.ReserveDate} \n Время: {infoReservation.ReserveTime} \n Номер столика: {infoReservation.IdTable}",
                    replyMarkup: Program.ShowFinallyReservationButton());
            }

            else if (callback.Data == "sendBron")
            {

                ReservationInfo allInfo = await DataBase.GetAllInfoState(userId.ToString(), "id");
                if (allInfo.IdTable != 0 && allInfo.CountPeople != null && allInfo.ReserveDate != null && allInfo.ReserveTime != null)
                { 
                    await DataBase.AddReservation(
                        allInfo.IdTable,
                        allInfo.ReserveDate,
                        userId.ToString(),
                        allInfo.ReserveTime,
                        allInfo.CountPeople,
                        confirmYesNo: "No"
                        );

                    await bot.EditMessageTextAsync(
                        chatId: idAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Бронь успешно изменена и подтверждена. \n Уведомление о изменении брони отправлена клиенту");

                    await Program.admin.SendStatusReservationToClient(bot, userId.ToString(), allInfo, "change");

                }
                else
                {
                    await bot.EditMessageTextAsync(
                        chatId: idAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Произошла ошибка в изменении брони клиента");
                }
            }

            else if (callback.Data.Contains("cancel"))
            {
                int idResevation = Convert.ToInt32(callback.Data.Substring(7));

                ReservationInfo info = await DataBase.GetAllInfoReservation(idResevation);

                if (info.IdTable != 0 && info.CountPeople != null && info.ReserveDate != null && info.ReserveTime != null && info.Confirmation != "Yes")
                {
                    await bot.EditMessageTextAsync(
                    chatId: Program.userIdAdmin,
                    messageId: callback.Message.MessageId,
                    text: "Вы успешно отменили бронь клиента");
                    await Program.admin.SendStatusReservationToClient(bot, info.UserId, info, "cancel");

                    await DataBase.DeleteReservation(idResevation);
                    await DataBase.DeleteStateReservation(callback.Message.Chat.Id.ToString());
                }
                else
                {
                    await bot.EditMessageTextAsync(
                        chatId: idAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Ошибка в отмене брони");
                }
            }

            else if (callback.Data.Contains("change"))
            {
                await bot.EditMessageTextAsync(
                    chatId: idAdmin,
                    messageId: callback.Message.MessageId,
                    text: "Выберите на какое количество людей вы хотите забронировать столик клиенту",
                    replyMarkup: Program.ShowInlineCountPeopleButton());

                int idReservation = Convert.ToInt32(callback.Data.Substring(7));

                ReservationInfo infoReservations = await DataBase.GetAllInfoReservation(idReservation);

                Program.userIdChangedReservationByAdmin = infoReservations.UserId;

                await DataBase.DeleteReservation(idReservation);
                await DataBase.DeleteStateReservation(infoReservations.UserId);
                
            }

            else if (callback.Data.Contains("accept"))
            {
                int idReservation = Convert.ToInt32(callback.Data.Substring(7));

                ReservationInfo info = await DataBase.GetAllInfoReservation(idReservation);

                if (info.IdTable != 0 && info.CountPeople != null && info.ReserveDate != null && info.ReserveTime != null)
                { 
                    await bot.EditMessageTextAsync(
                        chatId: Program.userIdAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Вы успешно подтвердили бронь клиента");

                    await Program.admin.SendStatusReservationToClient(bot, info.UserId, info, "accept");

                    await DataBase.ConfirmReservation(Convert.ToInt32(callback.Data.Substring(7)));
                }
                else
                {
                    await bot.EditMessageTextAsync(
                        chatId: idAdmin,
                        messageId: callback.Message.MessageId,
                        text: "Ошибка в подтверждении брони");
                }
            }


            else if (callback.Data == "CheckReserveAdmin")
            {
                List<ReservationInfo> infoResevations = await DataBase.GetReservationNoYesConfiration("no");

                if (infoResevations.Count > 0)
                {
                    foreach (var reservation in infoResevations)
                    {
                        await bot.SendTextMessageAsync(
                            chatId: callback.Message.Chat.Id,
                            text: $"<b> Бронь на подтверждение {reservation.IdReservation}!  \n  Номер столика: {reservation.IdTable} \n " +
                            $"Дата: {reservation.ReserveDate} \n Время: {reservation.ReserveTime} Количество мест: {reservation.CountPeople}</b>",
                            replyMarkup: Program.admin.ShowInlineResevationToAnswerAdminButtons(reservation.IdReservation.ToString()),
                            parseMode: ParseMode.Html
                            );
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(idAdmin, "В данный момент заявки требующие подтверждения - отсутствуют");
                }
            }
            else if (callback.Data == "CheckReadyReserve")
            {
                List<ReservationInfo> info = await DataBase.GetReservationNoYesConfiration("yes");

                if (info.Count > 0)
                { 
                    foreach (var reservation in info)
                    {
                        List<string> infoUser = await DataBase.GetInfoUser(reservation.UserId);

                        await bot.SendTextMessageAsync(
                        chatId: idAdmin,
                        text: $" <b>#️⃣ Бронь:</b> {reservation.IdReservation}   \r\n<b>📋 Клиент:</b> {infoUser[0]} \r\n<b>📞 Телефон:</b> {infoUser[1]} \r\n\r\n<i>Описание брони 👇</i>\r\n " +
                        $"<i>• 🗓 Дата:</i> {reservation.ReserveDate} \r\n\r\n <i>• 🕔 Время:</i> {reservation.ReserveTime} \r\n\r\n<i>• 👥 Кол-во персон:</i> {reservation.CountPeople} " +
                        $"\r\n\r\n<i>• 🥃 Номер столика:</i> {reservation.IdTable}",
                        parseMode: ParseMode.Html
                        );
                    }
                }
                else
                {
                    await bot.SendTextMessageAsync(
                        chatId: idAdmin,
                        text: "Брони отсутствуют!");
                }

            }


        }

        public InlineKeyboardMarkup ShowInlineResevationToAnswerAdminButtons(string idReservation)
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Отменить ⛔️ ", $"cancel {idReservation}"),
                InlineKeyboardButton.WithCallbackData(text: "Изменить 🔁", $"change  {idReservation}"),
                InlineKeyboardButton.WithCallbackData(text: "Подтвердить ✅", $"accept {idReservation}")
            });
            return new InlineKeyboardMarkup(buttonRows);
        }
        //public InlineKeyboardMarkup ShowAcceptChoiceAdmin(string idReservation)
        //{
        //    List<InlineKeyboardButton[]> buttonrRows = new List<InlineKeyboardButton[]>();

        //    buttonrRows.Add(new[]
        //    {
        //        InlineKeyboardButton.WithCallbackData(text: "✅ Да", callbackData: $"yes {idReservation}"),
        //        InlineKeyboardButton.WithCallbackData(text: "⛔️ Нет", callbackData: $"no {idReservation}")
        //    });
        //    return new InlineKeyboardMarkup(buttonrRows);
        //}


        private static InlineKeyboardMarkup ShowInlineCheckReservationNoConfirmAdminButton()
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Брони без подтверждения","CheckReserveAdmin")
            });
            return new InlineKeyboardMarkup(buttonRows);
        }

        private readonly long userId = 809666698;
        private ITelegramBotClient bot;
    }
}
