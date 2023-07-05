using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotWithPayment;

public class ProcessMessageResponse
{
    public string ResponseMessage { get; set; }
    public IReplyMarkup? InlineButtons { get; set; }

    public ProcessMessageResponse(string responseMessage, IReplyMarkup? inlineButtons = null)
    {
        ResponseMessage = responseMessage;
        InlineButtons = inlineButtons;
    }
}