using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBotWithPayment.CrystalPay;
using TelegramBotWithPayment.MongoDB;

namespace TelegramBotWithPayment;

public class TelegramBotHandling
{
    private CancellationToken CancellationToken { get; } = new CancellationToken();
    private MongoBase CurrentMongoBase { get; }
    private CrystalPayApiCommands CurrentApiCommands { get; }
    
    public TelegramBotHandling(MongoBase mongoBase, CrystalPayApiCommands currentApiCommands)
    {
        CurrentMongoBase = mongoBase;
        CurrentApiCommands = currentApiCommands;
    }
    public void StartTelegramBotHandling(ITelegramBotClient botClient)
    {
        botClient.StartReceiving(UpdateHandler, PollingErrorHandler, cancellationToken: CancellationToken);
    }

    async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            ProcessMessageHandler processMessageHandler = new ProcessMessageHandler(CurrentMongoBase, CurrentApiCommands);
        
            ProcessMessageResponse processMessageResponse = processMessageHandler.Process(update);

            if(processMessageResponse.InlineButtons == null)
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    processMessageResponse.ResponseMessage,
                    replyMarkup: StaticButtons.GetButtons(),
                    cancellationToken: CancellationToken);
            else
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    processMessageResponse.ResponseMessage,
                    replyMarkup: processMessageResponse.InlineButtons,
                    cancellationToken: CancellationToken);
        } 
        else if (update.CallbackQuery != null)
        {
            ProcessCallbackQueryData processCallbackQuery = 
                new ProcessCallbackQueryData(botClient, update, CurrentMongoBase, CurrentApiCommands);

            ProcessMessageResponse processMessageResponse = await processCallbackQuery.ProcessCallbackQuery(update.CallbackQuery);
            
            if(processMessageResponse.InlineButtons == null)
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.From.Id,
                    processMessageResponse.ResponseMessage,
                    replyMarkup: StaticButtons.GetButtons(),
                    cancellationToken: CancellationToken);
            else
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.From.Id,
                    processMessageResponse.ResponseMessage,
                    replyMarkup: processMessageResponse.InlineButtons,
                    cancellationToken: CancellationToken);
        }
    }

    Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);

        return Task.CompletedTask;
    }
}