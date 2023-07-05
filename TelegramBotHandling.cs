using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBotWithPayment.CrystalPay;
using TelegramBotWithPayment.MongoDB;

namespace TelegramBotWithPayment;

public class TelegramBotHandling
{
    private CancellationToken CancellationToken { get; set; } = new CancellationToken();
    private MongoBase CurrentMongoBase { get; }
    private CrystalPayApiCommands CurrentApiCommands { get; }
    
    public TelegramBotHandling(MongoBase mongoBase, CrystalPayApiCommands currentApiCommands)
    {
        CurrentMongoBase = mongoBase;
        CurrentApiCommands = currentApiCommands;
    }
    public Task StartTelegramBotHandling(ITelegramBotClient botClient)
    {
        botClient.StartReceiving(UpdateHandler, PollingErrorHandler, cancellationToken: CancellationToken);

        return Task.CompletedTask;
    }

    async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null)
            return;
        
        ProcessMessageHandler processMessageHandler = new ProcessMessageHandler(CurrentMongoBase, CurrentApiCommands);
        
        string responseMessage = processMessageHandler.Process(update);

        if (responseMessage is "Error" or "Sequence contains no elements")
            return;
        
        await botClient.SendTextMessageAsync(update.Message.Chat.Id, responseMessage, replyMarkup: StaticButtons.GetButtons(), cancellationToken: CancellationToken);
    }

    Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);

        return Task.CompletedTask;
    }
}