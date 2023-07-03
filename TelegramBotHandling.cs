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
        
    }

    Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}