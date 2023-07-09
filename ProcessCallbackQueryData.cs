using System.Globalization;
using Telegram.Bot.Types;
using TelegramBotWithPayment.CrystalPay;
using TelegramBotWithPayment.CrystalPay.CrystalPayStructures;
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
    
    public async Task<ProcessMessageResponse> ProcessCallbackQuery(CallbackQuery callbackQuery)
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
                string stage = GetUserStage();

                if (HasUserActiveOrder())
                    return new ProcessMessageResponse("You already have an active order! Check your active order");
                if (stage != "ConfirmPayment")
                    return new ProcessMessageResponse("Now you are not at this step!");

                ProcessMessageResponse messageConfirmPaymentResponse = await CreateInvoice();
                
                if (messageConfirmPaymentResponse.InlineButtons != null)
                    UpdateUserStage("WaitingForPayment");

                return messageConfirmPaymentResponse;
            case "CheckPayment":
                if (!HasUserActiveOrder())
                    return new ProcessMessageResponse("You do not have active order!");

                ProcessMessageResponse messageCheckPaymentResponse = await CheckPayment();

                if (messageCheckPaymentResponse.ResponseMessage == "payed")
                {
                    CompletePayedInvoice();

                    return new ProcessMessageResponse("Payment was successful. You can check your current balance 💰💸");
                }
                if (messageCheckPaymentResponse.ResponseMessage == "notpayed")
                {
                    return new ProcessMessageResponse("You have not payed yet!");
                }

                return new ProcessMessageResponse("We have some troubles with checking your order.. You can DM our support");
            default:
                return new ProcessMessageResponse("Something with callback query data went worng");
        }
    }
    
    private string GetMenu()
    {
        Commands.MongoCollectionSkull skull = new Commands.MongoCollectionSkull(Commands.MongoCollectionSkull.CollectionNames.UserCollection,
            new UserStructure());
        
        if (!CurrentMongoBase.Commands.IsValue("userid", Convert.ToString(UserId), skull))
            return "Error";
        
        UserStructure user = (UserStructure)CurrentMongoBase.Commands.GetUser(Convert.ToString(UserId), skull);

        string menu = $"⚙️Account ID: {UserId}⚙️\n💰Balance: {user.balance}💰\n💳Active Payment: {user.active_order}💳";

        return menu;
    }

    private string GetDepositMessage()
    {
        if (!CurrentMongoBase.Commands.IsValue("userid", Convert.ToString(UserId), new Commands.MongoCollectionSkull(Commands.MongoCollectionSkull.CollectionNames.UserCollection,
                new UserStructure())))
            return "Error";

        string depositMessage = "Enter a sum in USD💲 you want to deposit. The deposit sum should be more than 5💲 and less than 500💲. \nExample: 23,75";

        return depositMessage;
    }
    
    private void UpdateUserStage(string stageName)
    {
        Commands.MongoCollectionSkull skull = new Commands.MongoCollectionSkull(Commands.MongoCollectionSkull.CollectionNames.UserCollection,
            new UserStructure());
        
        bool result = CurrentMongoBase.Commands
            .UpdateValue("userid", $"{UserId}", "stage", stageName, skull);

        if (!result)
            throw new Exception("Error with DataBase ( ProcessMessageHandler -> UpdateUserStage() )");
    }

    private string GetUserStage()
    {
        Commands.MongoCollectionSkull skull = new Commands.MongoCollectionSkull(
            Commands.MongoCollectionSkull.CollectionNames.UserCollection,
            new UserStructure());

        string stage = CurrentMongoBase.Commands
            .GetValueFromBase("userid", $"{UserId}", skull, Commands.StructureMethods.GetStage);

        if (string.IsNullOrEmpty(stage))
            throw new Exception("ERROR: Possible error with data base | Check your connection"); 
            
        return stage;
    }

    private bool HasUserActiveOrder()
    {
        Commands.MongoCollectionSkull skull = new Commands.MongoCollectionSkull(
            Commands.MongoCollectionSkull.CollectionNames.UserCollection, new UserStructure());

        try
        {
            string userOrderStatus = CurrentMongoBase.Commands
                .GetValueFromBase("userid", $"{UserId}", skull, Commands.StructureMethods.GetActiveOrder);
            
            return userOrderStatus == "true";
        }
        catch (Exception e)
        {
            throw new Exception("Error with database");
        }
        
    }

    private void CompletePayedInvoice()
    {
        Commands.MongoCollectionSkull paymentSkull = new Commands.MongoCollectionSkull(
            Commands.MongoCollectionSkull.CollectionNames.PaymentReceiptCollection, new PaymentReceiptStructure());
        Commands.MongoCollectionSkull userSkull = new Commands.MongoCollectionSkull(
            Commands.MongoCollectionSkull.CollectionNames.UserCollection, new UserStructure());
        
        string receiptAmount = CurrentMongoBase.Commands
            .GetValueFromBase("userid", $"{UserId}", paymentSkull, Commands.StructureMethods.GetAmount);

        // Set true for "completed" parameter in UserPaymentReceipt
        CurrentMongoBase.Commands.UpdateValue(       
            "userid", 
            $"{UserId}", 
            "completed", 
            "true", 
            paymentSkull);
        // Set false for "active_order" parameter in Users
        CurrentMongoBase.Commands.UpdateValue(
            "userid",
            $"{UserId}",
            "active_order",
            "false",
            userSkull);
        // Set balance amount for "balance" parameter in Users
        CurrentMongoBase.Commands.UpdateValue(
            "userid",
            $"{UserId}",
            "balance",
            receiptAmount,
            userSkull);
    }
    
    private async Task<ProcessMessageResponse> CreateInvoice()
    {
        Commands.MongoCollectionSkull userSkull =
            new Commands.MongoCollectionSkull(Commands.MongoCollectionSkull.CollectionNames.UserCollection, 
                new UserStructure());

        Commands.MongoCollectionSkull paymentReceiptSkull =
            new Commands.MongoCollectionSkull(Commands.MongoCollectionSkull.CollectionNames.UserCollection, 
                new UserStructure());
        
        try
        {
            string userPaymentAmountStr = CurrentMongoBase.Commands.GetValueFromBase("userid", $"{UserId}", userSkull,
                Commands.StructureMethods.GetCurrentValue);
        
            double userPaymentAmount = Convert.ToDouble(userPaymentAmountStr);

            InvoiceStructure invoice = await CrystalPayApiCommands.CreatePaymentInvoice(userPaymentAmount);

            if (invoice.Erros)
                return new ProcessMessageResponse("There are some troubles with invoice creating.. Try it later");
            if (invoice.InvoiceUrl == null || invoice.InvoiceId == null)
                return new ProcessMessageResponse("There are some troubles with invoice creating.. Try it later");

            CurrentMongoBase.Commands.
                UpdateValue("userid", $"{UserId}", "active_order", "true", userSkull);
            
            CurrentMongoBase.Commands.AddReceipt(new PaymentReceiptStructure()
            {
                userid = Convert.ToString(UserId),
                date = Convert.ToString(DateTime.UtcNow, CultureInfo.CurrentCulture),
                amount = userPaymentAmountStr,
                receipt_id = invoice.InvoiceId,
                receipt_url = invoice.InvoiceUrl,
                completed = "false"
            });
            
            return new ProcessMessageResponse("You have to pay it within 60 minutes or another way it will be canceled 🕒",
                InlineButtons.GetPaymentLinkButton(invoice.InvoiceUrl));
        }
        catch (Exception e)
        {
            return new ProcessMessageResponse("There are some troubles with invoice creating..");
        }
    }
    
    private async Task<ProcessMessageResponse> CheckPayment()
    {
        Commands.MongoCollectionSkull skull = new Commands.MongoCollectionSkull(
            Commands.MongoCollectionSkull.CollectionNames.PaymentReceiptCollection, new PaymentReceiptStructure());
        
        string invoiceId = CurrentMongoBase.Commands.GetValueFromBase("userid", $"{UserId}",
            skull, Commands.StructureMethods.GetReceiptId);

        if (string.IsNullOrEmpty(invoiceId))
            return new ProcessMessageResponse("We have some troubles with our service..");

        string invoiceResponse = await CrystalPayApiCommands.GetInvoiceInfo(invoiceId);
        
        if(invoiceResponse.ToLower().Contains("error"))
            return new ProcessMessageResponse("We have some troubles with our service..");

        return new ProcessMessageResponse(invoiceResponse);
    }
}