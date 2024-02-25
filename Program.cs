using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace restaurantBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var bot = new TelegramBotClient("6717902573:AAFllwaelabWcpQyJI6_BjO8PUOQ1aNWhT4");

            bot.StartReceiving(Update,Error);
        }

        private static async Task Update(ITelegramBotClient bot, Update update, CancellationToken cts)
        {


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

    }
}
