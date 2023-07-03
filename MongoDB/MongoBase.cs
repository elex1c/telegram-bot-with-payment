using MongoDB.Driver;

namespace TelegramBotWithPayment.MongoDB;

public class MongoBase
{
    public MongoClient MongoClient { get; set; }

    public MongoBase(string connectionString)
    {
        MongoClient = new MongoClient(connectionString);
    }
}