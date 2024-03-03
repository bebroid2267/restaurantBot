using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace restaurantBot
{
    internal class Admin
    {

        public Admin(long userId, ITelegramBotClient bot) 
        { 
            this.userId = userId;
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
                replyMarkup: ShowInlineResevationToAnswerAdminButton(reservation.IdReservation.ToString()),
                parseMode: ParseMode.Html
                );

            }
        }

        private InlineKeyboardMarkup ShowInlineResevationToAnswerAdminButton(string idReservation)
        {
            List<InlineKeyboardButton[]> buttonRows = new List<InlineKeyboardButton[]>();

            buttonRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Отменить ⛔️ ", $"cancel {idReservation}"),
                InlineKeyboardButton.WithCallbackData(text: "Изменить 🔁", $"change {idReservation}"),
                InlineKeyboardButton.WithCallbackData(text: "Подтвердить ✅", $"accept {idReservation}")
            });
            return new InlineKeyboardMarkup(buttonRows);
        }




        private long userId;
        private ITelegramBotClient bot;
    }
}
