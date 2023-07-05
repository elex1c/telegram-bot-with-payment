using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotWithPayment;

public class InlineButtons
{
    public static IReplyMarkup GetStartDepositButtons()
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[]
        {
            new InlineKeyboardButton("Back to Menu") { CallbackData = "BackToMenu" },
            new InlineKeyboardButton("Continue payment process") { CallbackData = "ContinuePayment" }
        });

        return inlineKeyboard;
    }
}