using Telegram.Bot.Types;
using TelegramBotWithPayment.CrystalPay;
using TelegramBotWithPayment.MongoDB;
using TelegramBotWithPayment.MongoDB.CollectionSctructures;

namespace TelegramBotWithPayment;

public class ProcessCallbackQueryData
{
    private Update CurrentUpdate { get; }
    private MongoBase CurrentMongoBase { get; }
    private long UserId { get; set; }
    private CrystalPayApiCommands CrystalPayApiCommands { get; }
    private string ResponseMessage { get; set; }

    public ProcessCallbackQueryData(Update currentUpdate, MongoBase currentMongoBase, CrystalPayApiCommands crystalPayApiCommands)
    {
        CurrentUpdate = currentUpdate;
        CurrentMongoBase = currentMongoBase;
        CrystalPayApiCommands = crystalPayApiCommands;
    }
    
    public ProcessMessageResponse ProcessCallbackQuery(CallbackQuery callbackQuery)
    {
        UserId = callbackQuery.From.Id;
        
        switch (callbackQuery.Data)
        {
            case "BackToMenu":
                ResponseMessage = GetMenu();

                UpdateUserStage("Menu");

                return new ProcessMessageResponse(ResponseMessage);
            case "ContinuePayment":
                ResponseMessage = GetDepositMessage();
                
                UpdateUserStage("DepositSumEntering");
                
                return new ProcessMessageResponse(ResponseMessage);
            case "ConfirmPayment":
                //
            default:
                return new ProcessMessageResponse("Something with callback query data went worng");
            
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

    private string GetDepositMessage()
    {
        if (!CurrentMongoBase.Commands.IsValue("userid", Convert.ToString(UserId)))
            return "Error";

        string depositMessage = "Enter a sum in USD💲 you want to deposit. The deposit sum should be more than 5💲 and less than 500💲. \nExample: 23.75";

        return depositMessage;
    }
    
    private void UpdateUserStage(string stageName)
    {
        bool result = CurrentMongoBase.Commands
            .UpdateValue("userid", $"{UserId}", "stage", stageName);

        if (!result)
            throw new Exception("Error with DataBase ( ProcessMessageHandler -> UpdateUserStage() )");
    }
}