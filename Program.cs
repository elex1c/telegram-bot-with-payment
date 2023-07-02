using Telegram.Bot;

namespace TelegramBotWithPayment
{
    class Program
    {         
        static async Task Main(string[] args)
        {
            TelegramBotClient botClient = new TelegramBotClient("{YOUR_ACCESS_TOKEN_HERE}");

            TelegramBotHandling telegramBotHandling = new();

            telegramBotHandling.StartTelegramBotHandling(botClient);

            Console.ReadKey();
        }
    }
}