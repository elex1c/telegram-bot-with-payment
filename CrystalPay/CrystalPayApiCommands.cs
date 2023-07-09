using System.Data;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using TelegramBotWithPayment.CrystalPay.CrystalPayStructures;

namespace TelegramBotWithPayment.CrystalPay;

public class CrystalPayApiCommands
{
    private string AuthorizationLogin { get; }
    private string AuthorizationSecret { get; }
    private const string CreateInvoiceUrl = "https://api.crystalpay.io/v2/invoice/create/";
    private const string CheckInvoiceInfo = "https://api.crystalpay.io/v2/invoice/info/";
    
    public CrystalPayApiCommands(string authorizationLogin, string authorizationSecret)
    {
        AuthorizationLogin = authorizationLogin;
        AuthorizationSecret = authorizationSecret;
    }
    
    public async Task<InvoiceStructure> CreatePaymentInvoice(double invoiceAmount)
    {
        string doubleAmountInStr = Convert.ToString(invoiceAmount, CultureInfo.CurrentCulture);
        doubleAmountInStr = doubleAmountInStr.Replace(',', '.');
        
        string json = "{\"auth_login\":\"" + AuthorizationLogin + "\","
            + "\"auth_secret\":\"" + AuthorizationSecret + "\","
            + "\"amount\":" + doubleAmountInStr + ","
            + "\"amount_currency\":\"USD\","
            + "\"type\":\"purchase\","
            + "\"lifetime\":60}";

        using HttpClient client = new HttpClient();

        var response = await client.PostAsync(CreateInvoiceUrl,
            new StringContent(json, Encoding.UTF8, "application/json"));

        string responseJson = await response.Content.ReadAsStringAsync();

        dynamic? responseObject = JsonConvert.DeserializeObject(responseJson);

        string checkerResult = DynamicErrorChecker(responseObject);

        if (checkerResult.ToLower().Contains("error"))
            return new InvoiceStructure() { Erros = true };

        InvoiceStructure invoice = new()
        {
            InvoiceId = responseObject?.id,
            InvoiceUrl = responseObject?.url
        };

        return invoice;
    }

    public async Task<string> GetInvoiceInfo(string invoiceId)
    {
        string json = "{\"auth_login\":\"" + AuthorizationLogin + "\","
                      + "\"auth_secret\":\"" + AuthorizationSecret + "\","
                      + "\"id\":\"" + invoiceId + "\"}";

        using HttpClient client = new HttpClient();

        var response = await client.PostAsync(CheckInvoiceInfo,
            new StringContent(json, Encoding.UTF8, "application/json"));

        string responseJson = await response.Content.ReadAsStringAsync();

        dynamic? responseObject = JsonConvert.DeserializeObject(responseJson);
        
        string checkerResult = DynamicErrorChecker(responseObject);
        
        if (checkerResult.ToLower().Contains("error"))
            return checkerResult;

        string state = responseObject.state;

        return state == "payed" ? "payed" : "notpayed";
    }

    private string DynamicErrorChecker(dynamic? d)
    {
        if (d == null)
            return "ERROR: response json object equals null";

        if (d.error == true)
            return "ERROR: crystalpay invoice contains error";

        return "SUCCESS";
    }
}