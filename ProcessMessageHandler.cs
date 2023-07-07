using MongoDB.Driver.Core.WireProtocol.Messages;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotWithPayment.CrystalPay;
using TelegramBotWithPayment.MongoDB;
using TelegramBotWithPayment.MongoDB.CollectionSctructures;

namespace TelegramBotWithPayment;

public class ProcessMessageHandler
{
    private string GreetingMessage => ", welcome to our bot! 👋\n\n We're thrilled to have you on board. Allow us to introduce you to our fantastic payment bot, your trusted companion for all your financial needs. 💳💸\n\n Whether you're looking to send or receive payments, manage transactions, or track your expenses, our payment bot is here to simplify your financial life. 💲 \n\nWith its user-friendly interface and robust security measures, you can handle your finances with confidence and ease. Our payment bot offers a range of convenient features, such as seamless integration with popular payment platforms, real-time notifications, and personalized transaction history. It's designed to save you time and effort, so you can focus on what matters most to you. 👨‍💻";
    private string DepositMessage => "Do you really want to continue the payment?";
    private string ResponseMessage { get; set; }
    private MongoBase CurrentMongoBase { get; }
    private CrystalPayApiCommands CurrentApiCommands { get; }
    private ProcessMessageResponse ProcessMessageResponse { get; set; }
    private long UserId { get; set; }

    public ProcessMessageHandler(MongoBase mongoBase, CrystalPayApiCommands apiCommands)
    {
        CurrentMongoBase = mongoBase;
        CurrentApiCommands = apiCommands;
    }
    public ProcessMessageResponse Process(Update update)
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
                        return new ProcessMessageResponse("Error");

                    ProcessMessageResponse = ProcessTextMessage(update.Message.Text, username);
                }
                break;
            default:
                return new ProcessMessageResponse("You have sent incorrect type of message!");
        }

        return ProcessMessageResponse;
    }

    private ProcessMessageResponse ProcessTextMessage(string message, string senderUsername)
    {
        switch (message)
        {
            case "/start":
                AddUserInDatabase();

                SetStage("Start");
                
                ResponseMessage = senderUsername + GreetingMessage;

                ProcessMessageResponse = new ProcessMessageResponse(ResponseMessage);
                
                return ProcessMessageResponse;
            case "Menu":
                ResponseMessage = GetMenu();
                
                SetStage("Menu");
                
                return new ProcessMessageResponse(ResponseMessage);
            case "Make a transfer":
                return new ProcessMessageResponse(senderUsername);
            case "Deposit":
                SetStage("Deposit");
                
                return new ProcessMessageResponse(DepositMessage, InlineButtons.GetStartDepositButtons());
            case "Invite link":
                return new ProcessMessageResponse(senderUsername);
            default:
                switch (GetStage())
                {
                    case "DepositSumEntering":
                        double sumResult = ProcessDepositSum(message);

                        if (sumResult == 0)
                            return new ProcessMessageResponse("The deposit sum should be more than 5 and less than 500💲");
                        if (sumResult == 1)
                            return new ProcessMessageResponse("You have send incorrect type of message❗ \nExample ➜ 23.75");
                        
                        SetStage("ConfirmPayment");
                        
                        ResponseMessage = $"Do you want to continue payment in {sumResult}💲 ❓";

                        return new ProcessMessageResponse(ResponseMessage, InlineButtons.GetConfirmDepositButtons());
                    default:
                        return new ProcessMessageResponse("You have sent incorrect type of message");
                }
        }
    }

    private string GetSenderName(Message message)
    {
        string username = message.From != null ? message.From.FirstName : "User";

        return username;
    }

    private double ProcessDepositSum(string messageValue)
    {
        try
        {
            double value = Convert.ToDouble(messageValue);

            if (value is > 5 and < 500)
                return value;
            
            return 0;
        }
        catch (Exception)
        {
            return 1;
        }
    }
    
    private string GetMenu()
    {
        if (!CurrentMongoBase.Commands.IsValue("userid", Convert.ToString(UserId)))
            return "Error";
        
        UserStructure user = CurrentMongoBase.Commands.GetUser(Convert.ToString(UserId));
        
        string menu = $"⚙️Account ID: {UserId}⚙️\n💰Balance: {user.balance}💰\n💳Active Payment: {user.active_order}💳";

        return menu;
    }

    private string GetStage()
    {
        return CurrentMongoBase.Commands.GetValueFromBase("userid", $"{UserId}", Commands.LongMethodsActions.GetStage);
    }

    private void SetStage(string stageName)
    {
        bool result = CurrentMongoBase.Commands
            .UpdateValue("userid", $"{UserId}", "stage", stageName);

        if (!result)
            throw new Exception("Error with DataBase ( ProcessMessageHandler -> UpdateUserStage() )");
    }
    
    private string GetCurrentValue()
    {
        return CurrentMongoBase.Commands.GetValueFromBase("userid", $"{UserId}", Commands.LongMethodsActions.GetCurrentValue);
    }
    
    private void SetCurrentValue(string currentValue)
    {
        bool result = CurrentMongoBase.Commands
            .UpdateValue("userid", $"{UserId}", "current_value", currentValue);

        if (!result)
            throw new Exception("Error with DataBase ( ProcessMessageHandler -> UpdateUserStage() )");
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
}