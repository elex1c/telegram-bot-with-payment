using System.Collections;
using Telegram.Bot;
using TelegramBotWithPayment.CrystalPay;
using TelegramBotWithPayment.MongoDB;

namespace TelegramBotWithPayment
{
    class Program
    {         
        static void Main(string[] args)
        {
            string? telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            string? mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
            string? crystalPaySecret = Environment.GetEnvironmentVariable("CRYSTALPAY_SECRET");
            string? crystalPayLogin = Environment.GetEnvironmentVariable("CRYSTALPAY_LOGIN");
            
            if (telegramBotToken == null)
               throw new NullReferenceException("Telegram token equals null");
            if (mongoConnectionString == null)
                throw new NullReferenceException("Mongo connection string equals null");
            if (crystalPaySecret == null)
                throw new NullReferenceException("CrystalPay secret equals null");
            if (crystalPayLogin == null)
                throw new NullReferenceException("CrystalPay login equals null");
            
            TelegramBotClient botClient = new(telegramBotToken);
            CrystalPayApiCommands crystalPayApiCommands = new CrystalPayApiCommands(crystalPayLogin, crystalPaySecret);
            MongoBase mongoBase = new(mongoConnectionString);
            TelegramBotHandling telegramBotHandling = new(mongoBase, crystalPayApiCommands);
            
            telegramBotHandling.StartTelegramBotHandling(botClient);

            Console.ReadKey();
        }
    }
}