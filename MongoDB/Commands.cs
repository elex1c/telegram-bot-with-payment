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
    private IMongoCollection<BsonDocument> BaseCollection { get; set; }
    private const string DataBaseName = "TelegramBotPaymentBase";
    private const string UsersCollectionName = "Users";

    public Commands(MongoClient client)
    {
        Client = client;
        
        BaseCollection = Client.GetDatabase(DataBaseName).GetCollection<BsonDocument>(UsersCollectionName);
    }

    public string GetValueFromBase(string valueName, string value, LongMethodsActions methodsAction)
    {
        FilterDefinition<BsonDocument> filter = new FilterDefinitionBuilder<BsonDocument>().Eq(valueName, value);
        
        try
        {
            string json = BaseCollection.Find(filter).First().ToJson();

            json = ClearJsonObjectId(json);
            
            switch (methodsAction)
            {
                case LongMethodsActions.GetUserId:
                    return Methods.GetUserId(json);
                default:
                    return "0";
            }
        }
        catch (Exception e)
        {
            return "0";   
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
            BaseCollection.UpdateOne(filter, update);

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
            BaseCollection.Find(filter).First();

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
            string json = JsonConvert.SerializeObject(new UserStructure() { userid = userId, balance = "0", activeorder = "false", stage = "Start"});
            
            BaseCollection.InsertOne(BsonDocument.Parse(json));
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
            string json = BaseCollection.Find(filter).First().ToJson();

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
    
    private static class Methods
    {
        public static string GetUserId(string json)
        {
            UserStructure? user = JsonConvert.DeserializeObject<UserStructure>(json);

            if (user == null)
                throw new NullReferenceException("User equals null");

            return user.userid;
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
    
    public enum LongMethodsActions
    {
        GetUserId
    }
}