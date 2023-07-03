using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotWithPayment;

public class Buttons
{
    private const string Button1 = "Menu";
    private const string Button2 = "Make a payment";
    private const string Button3 = "My Profile";
    private const string Button4 = "Invite link";
    
    public static IReplyMarkup GetButtons()
    {
        return new ReplyKeyboardMarkup(new List<List<KeyboardButton>>
        {
            new List<KeyboardButton>
            {
                new KeyboardButton(Button1),
                new KeyboardButton(Button2)
            },
            new List<KeyboardButton>
            {
                new KeyboardButton(Button3),
                new KeyboardButton(Button4)
            }
        });
    }
}