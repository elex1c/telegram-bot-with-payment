using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBotWithPayment;

public class TelegramBotHandling
{
    private CancellationToken CancellationToken { get; set; } = new CancellationToken();
    
    public Task StartTelegramBotHandling(ITelegramBotClient botClient)
    {
        botClient.StartReceiving(UpdateHandler, PollingErrorHandler, cancellationToken: CancellationToken);

        return Task.CompletedTask;
    }

    async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null)
            return;
        
        ProcessMessageHandler processMessageHandler = new ProcessMessageHandler();
        
        string responseMessage = processMessageHandler.Process(update);

        await botClient.SendTextMessageAsync(update.Message.Chat.Id, responseMessage, replyMarkup: Buttons.GetButtons(), cancellationToken: CancellationToken);
    }

    Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);

        return Task.CompletedTask;
    }
}