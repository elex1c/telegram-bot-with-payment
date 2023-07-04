using MongoDB.Bson;
using MongoDB.Driver;

namespace TelegramBotWithPayment.MongoDB;

public class MongoBase
{
    private MongoClient MongoClient { get; }
    public Commands Commands { get; }

    public MongoBase(string connectionString)
    {
        MongoClient = new MongoClient(connectionString);

        Commands = new Commands(MongoClient);
    }
}