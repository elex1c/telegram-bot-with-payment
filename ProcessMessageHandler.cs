using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotWithPayment.CrystalPay;
using TelegramBotWithPayment.MongoDB;
using TelegramBotWithPayment.MongoDB.CollectionSctructures;

namespace TelegramBotWithPayment;

public class ProcessMessageHandler
{
    private string GreetingMessage => ", welcome to our bot! 👋\n\n We're thrilled to have you on board. Allow us to introduce you to our fantastic payment bot, your trusted companion for all your financial needs. 💳💸\n\n Whether you're looking to send or receive payments, manage transactions, or track your expenses, our payment bot is here to simplify your financial life. 💲 \n\nWith its user-friendly interface and robust security measures, you can handle your finances with confidence and ease. Our payment bot offers a range of convenient features, such as seamless integration with popular payment platforms, real-time notifications, and personalized transaction history. It's designed to save you time and effort, so you can focus on what matters most to you. 👨‍💻";
    private string DepositMessage => "";
    private string ResponseMessage { get; set; }
    private MongoBase CurrentMongoBase { get; }
    private CrystalPayApiCommands CurrentApiCommands { get; }
    private long UserId { get; set; }

    public ProcessMessageHandler(MongoBase mongoBase, CrystalPayApiCommands apiCommands)
    {
        CurrentMongoBase = mongoBase;
        CurrentApiCommands = apiCommands;
    }
    public string Process(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
                {
                    string username = GetSenderName(update.Message);

                    if (update.Message.From != null)
                        UserId = update.Message.From.Id;
                    else
                        return "Error";

                    ResponseMessage = ProcessTextMessage(update.Message.Text, username);
                }
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
                AddUserInDatabase();

                UpdateUserStage("Start");
                
                ResponseMessage = senderUsername + GreetingMessage;
                
                return ResponseMessage;
            case "Menu":
                ResponseMessage = GetMenu();
                
                UpdateUserStage("Menu");
                
                return ResponseMessage;
            case "Make a transfer":
                return senderUsername;
            case "Deposit":
                UpdateUserStage("Deposit");
                
                return senderUsername;
            case "Invite link":
                return senderUsername;
            default:
                return "You have sent incorrect type of message";
        }
    }

    private string GetSenderName(Message message)
    {
        string username = message.From != null ? message.From.FirstName : "User";

        return username;
    }
    
    private string GetMenu()
    {
        if (!CurrentMongoBase.Commands.IsValue("userid", Convert.ToString(UserId)))
            return "Error";
        
        UserStructure user = CurrentMongoBase.Commands.GetUser(Convert.ToString(UserId));
        
        string menu = $"⚙️Account ID: {UserId}⚙️\n💰Balance: {user.balance}💰\n💳Active Payment: {user.activeorder}💳";

        return menu;
    }

    private void AddUserInDatabase()
    {
        try
        {
            CurrentMongoBase.Commands.AddUser(Convert.ToString(UserId));
        }
        catch (Exception)
        {
            throw new Exception("Error with DataBase ( ProcessMessageHandler -> AddUserInDatabase() )");
        }
    }

    private void UpdateUserStage(string stageName)
    {
        bool result = CurrentMongoBase.Commands
            .UpdateValue("userid", $"{UserId}", "stage", stageName);

        if (!result)
            throw new Exception("Error with DataBase ( ProcessMessageHandler -> UpdateUserStage() )");
    }
}