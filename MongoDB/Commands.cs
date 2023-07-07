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
    private IMongoCollection<BsonDocument> UserCollection { get; }
    private IMongoCollection<BsonDocument> PaymentReceiptCollection { get; }
    private const string DataBaseName = "TelegramBotPaymentBase";
    private const string UsersCollectionName = "Users";
    private const string PaymentReceiptCollectionName = "UserPaymentReceipt";

    public Commands(MongoClient client)
    {
        Client = client;
        
        UserCollection = Client.GetDatabase(DataBaseName).GetCollection<BsonDocument>(UsersCollectionName);
        PaymentReceiptCollection = Client.GetDatabase(DataBaseName).GetCollection<BsonDocument>(PaymentReceiptCollectionName);
    }

    public string GetValueFromBase(string valueName, string value, LongMethodsActions methodsAction)
    {
        FilterDefinition<BsonDocument> filter = new FilterDefinitionBuilder<BsonDocument>().Eq(valueName, value);
        
        try
        {
            string json = UserCollection.Find(filter).First().ToJson();

            json = ClearJsonObjectId(json);
            
            switch (methodsAction)
            {
                case LongMethodsActions.GetUserId:
                    return Methods.GetUserId(json);
                case LongMethodsActions.GetStage:
                    return Methods.GetSage(json);
                case LongMethodsActions.GetCurrentValue:
                    return Methods.GetCurrentValue(json);
                default:
                    return "";
            }
        }
        catch (Exception)
        {
            return "";   
        }
    }

    public bool UpdateValue<T>(string valueName, T searchingValue, string updatedValueName, T insertedValue)
    {
        FilterDefinition<BsonDocument> filter =
            new FilterDefinitionBuilder<BsonDocument>().Eq(valueName, searchingValue);

        UpdateDefinition<BsonDocument> update = 
            new UpdateDefinitionBuilder<BsonDocument>().Set(updatedValueName, insertedValue);

        try
        {
            UserCollection.UpdateOne(filter, update);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool IsValue<T>(string valueName, T checkingValue)
    {
        FilterDefinition<BsonDocument>
            filter = new FilterDefinitionBuilder<BsonDocument>().Eq(valueName, checkingValue);

        try
        {
            UserCollection.Find(filter).First();

            return true;
        }
        catch (Exception e)
        {
            if (e.Message.Contains("Sequence contains no elements"))
                return false;

            throw new Exception(e.Message);
        }
    }

    public void AddUser(string userId)
    {
        if (IsValue("userid", userId))
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

    public UserStructure GetUser(string userId)
    {
        FilterDefinition<BsonDocument> filter = new FilterDefinitionBuilder<BsonDocument>().Eq("userid", userId);

        try
        {
            string json = UserCollection.Find(filter).First().ToJson();

            json = ClearJsonObjectId(json);

            UserStructure? user = JsonConvert.DeserializeObject<UserStructure>(json);

            if (user == null)
                throw new Exception("User eqauls null");

            return user;
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
        json = json.Replace("_", "");
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
    }

    private class MongoCollectionSkull
    {
        public IMongoCollection<BsonDocument> Collection { get; }
    }
    
    public enum LongMethodsActions
    {
        GetUserId,
        GetStage,
        GetCurrentValue
    }
}