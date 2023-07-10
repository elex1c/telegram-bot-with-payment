using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotWithPayment.MongoDB;
using TelegramBotWithPayment.MongoDB.CollectionSctructures;

namespace TelegramBotWithPayment;

public class PaymentTimer
{
    private Timer Timer { get; set; }
    public MongoBase MongoBase { get; }
    private ITelegramBotClient BotClient { get; set; }
    private long UserId { get; set; }
    private int Ticks { get; set; }
    private MongoBase CurrentMongoBase { get; set; }
    
    public PaymentTimer(MongoBase mongoBase, ITelegramBotClient botClient, long userId)
    {
        MongoBase = mongoBase;
        BotClient = botClient;
        UserId = userId;
        Timer = new Timer(Callback, null, 0, 3600000);
    }
    
    private void Callback(object? state)
    {
        Ticks++;

        if (Ticks == 1)
            return;

        if (Ticks == 2)
        {
            CancelOrder();
            
            BotClient.SendTextMessageAsync(new ChatId(UserId), "Order has been canceled");

            Timer.Dispose();
        }
    }

    private void CancelOrder()
    {
        Commands.MongoCollectionSkull userSkull = new Commands.MongoCollectionSkull(
            Commands.MongoCollectionSkull.CollectionNames.UserCollection, new UserStructure());
        
        MongoBase.Commands.UpdateValue(
            "userid", $"{UserId}", "active_order", "false", userSkull);
    }
}