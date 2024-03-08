using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;


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
            List<ReservationInfo> infoReservation = await DataBase.GetReservationNoConfiration();

            foreach (var reservation in infoReservation)
            { 
                   await bot.SendTextMessageAsync(
                chatId: userId,
                text: $"<b>Пришла бронь на подтверждение! \n  Номер столика: {reservation.IdTable} \n " +
                $"Дата: {reservation.RegDate} \n Время: {reservation.ReserveTime} Количество мест: {reservation.CountPeople}</b>",
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
                await bot.SendTextMessageAsync(
                    chatId: idAdmin,
                    text: "Выберите на какое количество людей вы хотите забронировать столик ",
                    replyMarkup: Program.ShowInlineCountPeopleButton());
            }

            else if (callback.Data.Contains('-') || callback.Data == "backtime")
            {

                if (callback.Data.Contains('-'))
                {
                    Program.callbackData = callback.Data;
                }

                List<string> days = Program.GetDaysInMonth();

                await bot.SendTextMessageAsync(
                    chatId: idAdmin,
                    text: "Выберите дату бронирования",
                    replyMarkup: Program.ShowInlineDateTimeReservation(days, "days"));

                await DataBase.AddCountPeopleState(userId.ToString(), Program.callbackData);
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
                await DataBase.AddInfoState(callback.Message.Chat.Id.ToString(), dateState, "date");
                string dateReservation = Convert.ToDateTime(dateState).ToString("d");
                hours = Program.GetTimeDay(dateReservation);

                await bot.SendTextMessageAsync(
                    chatId: idAdmin,
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
                List<string> idsFreeTables = await DataBase.GetFreeIdTables(infoReresvation.CountPeople, infoReresvation.ReserveDate, infoReresvation.ReserveTime);

                await bot.SendTextMessageAsync(
                    chatId: idAdmin.ToString(),
                    text: "<b>Вот свободные столики на указанное время, дату и количество человек</b>",
                    replyMarkup: Program.ShowInlineTableReservation(idsFreeTables),
                    parseMode: ParseMode.Html);
            }

            else if (callback.Data.Contains("table"))
            {
                string idTable = callback.Data.Substring(6);
                await DataBase.AddInfoState(userId.ToString(), idTable, "table");

                ReservationInfo infoReservation = await DataBase.GetAllInfoState(userId.ToString(), "id");

                await bot.SendTextMessageAsync(
                    chatId: idAdmin,
                    text: $"Проверьте вашу заявку: \n Количество человек: {infoReservation.CountPeople} " +
                    $"\n Дата: {infoReservation.ReserveDate} \n Время: {infoReservation.ReserveTime} \n Номер столика: {infoReservation.IdTable}",
                    replyMarkup: Program.ShowFinallyReservationButton());
            }

            else if (callback.Data == "sendBron")
            {

                ReservationInfo allInfo = await DataBase.GetAllInfoState(userId.ToString(), "id");

                await DataBase.AddReservation(
                    allInfo.IdTable,
                    allInfo.ReserveDate,
                    userId.ToString(),
                    allInfo.ReserveTime,
                    allInfo.CountPeople
                    );

                await bot.EditMessageTextAsync(
                    chatId: idAdmin,
                    messageId: callback.Message.MessageId,
                    text: "Бронь успешно изменена и подтверждена. \n Уведомление о изменении брони отправлена клиенту");

                await Program.admin.SendStatusReservationToClient(bot, userId.ToString(), allInfo, "change");

                await DataBase.ConfirmReservation(allInfo.IdReservation);
                
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
