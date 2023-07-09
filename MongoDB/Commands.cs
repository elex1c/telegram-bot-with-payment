using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.CompilerServices;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Telegram.Bot.Requests;
using TelegramBotWithPayment.MongoDB.CollectionSctructures;

namespace TelegramBotWithPayment.MongoDB;

public class Commands
{
    private MongoClient Client { get; }
    private static IMongoCollection<BsonDocument> UserCollection { get; set; }
    private static IMongoCollection<BsonDocument> PaymentReceiptCollection { get; set; }
    private const string DataBaseName = "TelegramBotPaymentBase";
    private const string UsersCollectionName = "Users";
    private const string PaymentReceiptCollectionName = "UserPaymentReceipt";

    public Commands(MongoClient client)
    {
        Client = client;
        
        UserCollection = Client.GetDatabase(DataBaseName).GetCollection<BsonDocument>(UsersCollectionName);
        PaymentReceiptCollection = Client.GetDatabase(DataBaseName).GetCollection<BsonDocument>(PaymentReceiptCollectionName);
    }

    public string GetValueFromBase(string valueName, string value, MongoCollectionSkull skull, StructureMethods methods)
    {
        FilterDefinition<BsonDocument> filter = new FilterDefinitionBuilder<BsonDocument>().Eq(valueName, value);
        
        try
        {
            string json = skull.Collection.Find(filter).First().ToJson();

            json = ClearJsonObjectId(json);
            
            switch (methods)
            {
                case StructureMethods.GetUserId:
                    return Methods.GetUserId(json);
                case StructureMethods.GetStage:
                    return Methods.GetSage(json);
                case StructureMethods.GetCurrentValue:
                    return Methods.GetCurrentValue(json);
                case StructureMethods.GetActiveOrder:
                    return Methods.GetActiveOrder(json);
                case StructureMethods.GetReceiptId:
                    return Methods.GetReceiptId(json);
                case StructureMethods.GetAmount:
                    return Methods.GetPaymentAmount(json);
                default:
                    return "";
            }
        }
        catch (Exception)
        {
            return "";   
        }
    }

    public bool UpdateValue<T>(string searchingValueName, T searchingValue, string updatedValueName, T insertedValue, MongoCollectionSkull skull)
    {
        FilterDefinition<BsonDocument> filter =
            new FilterDefinitionBuilder<BsonDocument>().Eq(searchingValueName, searchingValue);

        UpdateDefinition<BsonDocument> update = 
            new UpdateDefinitionBuilder<BsonDocument>().Set(updatedValueName, insertedValue);

        try
        {
            skull.Collection.UpdateOne(filter, update);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool IsValue<T>(string checkingValueName, T checkingValue, MongoCollectionSkull skull)
    {
        FilterDefinition<BsonDocument>
            filter = new FilterDefinitionBuilder<BsonDocument>().Eq(checkingValueName, checkingValue);

        try
        {
            skull.Collection.Find(filter).First();

            return true;
        }
        catch (Exception e)
        {
            if (e.Message.Contains("Sequence contains no elements"))
                return false;

            throw new Exception(e.Message);
        }
    }

    public void AddUser(string userId, MongoCollectionSkull skull)
    {
        if (IsValue("userid", userId, skull))
            return;
            
        try
        {
            string json = JsonConvert.SerializeObject(new UserStructure() 
                { userid = userId, balance = "0", active_order = "false", stage = "Start", current_value = null });
            
            UserCollection.InsertOne(BsonDocument.Parse(json));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void AddReceipt(PaymentReceiptStructure paymentStructure)
    {
        try
        {
            string json = JsonConvert.SerializeObject(paymentStructure);
            
            PaymentReceiptCollection.InsertOne(BsonDocument.Parse(json));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public ICollectionStructures GetUser(string userId, MongoCollectionSkull skull)
    {
        FilterDefinition<BsonDocument> filter = new FilterDefinitionBuilder<BsonDocument>().Eq("userid", userId);

        try
        {
            string json = skull.Collection.Find(filter).First().ToJson();

            json = ClearJsonObjectId(json);

            skull.Structure = JsonConvert.DeserializeObject<UserStructure>(json);

            if (skull.Structure == null)
                throw new Exception("User eqauls null");

            return skull.Structure;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    private static string ClearJsonObjectId(string json)
    {
        json = json.Replace("(", "");
        json = json.Replace(")", "");
        json = json.Replace("ObjectId", "");

        return json;
    }
    
    private static class Methods
    {
        private static UserStructure GetUserStructure(string json)
        {
            UserStructure? user = JsonConvert.DeserializeObject<UserStructure>(json);
            
            return user ?? throw new NullReferenceException("User equals null");
        }

        private static PaymentReceiptStructure GetPaymentStructure(string json)
        {
            PaymentReceiptStructure? payment = JsonConvert.DeserializeObject<PaymentReceiptStructure>(json);
            
            return payment ?? throw new NullReferenceException("Payment equals null");
        }
        
        public static string GetUserId(string json)
        {
            UserStructure user = GetUserStructure(json);

            return user.userid;
        }
        
        public static string GetSage(string json)
        {
            UserStructure user = GetUserStructure(json);

            return user.stage;
        }
        
        public static string GetCurrentValue(string json)
        {
            UserStructure user = GetUserStructure(json);

            return user.current_value ?? "";
        }
        
        public static string GetActiveOrder(string json)
        {
            UserStructure user = GetUserStructure(json);

            return user.active_order ?? "";
        }

        public static string GetReceiptId(string json)
        {
            PaymentReceiptStructure payment = GetPaymentStructure(json);

            return payment.receipt_id ?? "";
        }

        public static string GetPaymentAmount(string json)
        {
            PaymentReceiptStructure payment = GetPaymentStructure(json);

            return payment.amount ?? "";
        }
    }

    public class MongoCollectionSkull
    {
        public IMongoCollection<BsonDocument> Collection { get; }
        public ICollectionStructures? Structure { get; set; }
        
        public MongoCollectionSkull(CollectionNames name, ICollectionStructures structure)
        {
            switch (name)
            {
                case CollectionNames.UserCollection:
                    Collection = UserCollection;
                    break;
                case CollectionNames.PaymentReceiptCollection:
                    Collection = PaymentReceiptCollection;
                    break;
            }

            Structure = structure;
        }

        public enum CollectionNames
        {
            UserCollection,
            PaymentReceiptCollection
        }
    }

    public enum StructureMethods
    {
        GetUserId,
        GetStage,
        GetCurrentValue,
        GetActiveOrder,
        GetReceiptId,
        GetAmount,
        GetReceiptUrl,
        GetCompleted
    }
}