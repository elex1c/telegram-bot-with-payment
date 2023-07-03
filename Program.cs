using System.Collections;
using Telegram.Bot;
using TelegramBotWithPayment.MongoDB;

namespace TelegramBotWithPayment
{
    class Program
    {         
        static async Task Main(string[] args)
        {
            string? telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            string? mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
            
            if (telegramBotToken == null)
               throw new NullReferenceException("Telegram token equals null");
            if (mongoConnectionString == null)
                throw new NullReferenceException("Mongo connection string equals null");
            
            TelegramBotClient botClient = new(telegramBotToken);
            TelegramBotHandling telegramBotHandling = new();
            MongoBase mongoBase = new(mongoConnectionString);
            
            telegramBotHandling.StartTelegramBotHandling(botClient);

            Console.ReadKey();
        }
    }
}