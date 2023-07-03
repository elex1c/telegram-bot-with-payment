using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotWithPayment;

public class ProcessMessageHandler
{
    private string GreetingMessage => ", welcome to our bot! 👋\n\n We're thrilled to have you on board. Allow us to introduce you to our fantastic payment bot, your trusted companion for all your financial needs. 💳💸\n\n Whether you're looking to send or receive payments, manage transactions, or track your expenses, our payment bot is here to simplify your financial life. 💲 \n\nWith its user-friendly interface and robust security measures, you can handle your finances with confidence and ease. Our payment bot offers a range of convenient features, such as seamless integration with popular payment platforms, real-time notifications, and personalized transaction history. It's designed to save you time and effort, so you can focus on what matters most to you. 👨‍💻";
    private string ResponseMessage { get; set; }
    
    public string Process(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
                    ResponseMessage = ProcessTextMessage(update.Message.Text, GetSenderName(update.Message));
                break;
            default:
                return "You have sent incorrect type of message!";
        }

        return ResponseMessage;
    }

    private string ProcessTextMessage(string message, string senderUsername)
    {
        switch (message)
        {
            case "/start":
                return senderUsername + GreetingMessage;
            default:
                return "You have sent incorrect type of message";
        }
    }

    private string GetSenderName(Message message)
    {
        string username = message.From != null ? message.From.FirstName : "User";

        return username;
    }
}