using System.Collections;
using Telegram.Bot;

namespace TelegramBotWithPayment
{
    class Program
    {         
        static async Task Main(string[] args)
        {
            string? telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            
            if (telegramBotToken == null)
               throw new NullReferenceException("Telegram token equals null");
            
            TelegramBotClient botClient = new TelegramBotClient(telegramBotToken);

            TelegramBotHandling telegramBotHandling = new();

            telegramBotHandling.StartTelegramBotHandling(botClient);

            Console.ReadKey();
        }
    }
}