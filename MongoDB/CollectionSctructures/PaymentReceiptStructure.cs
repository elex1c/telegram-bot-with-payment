namespace TelegramBotWithPayment.MongoDB.CollectionSctructures;

public class PaymentReceiptStructure : ICollectionStructures
{
    public string userid { get; init; }
    public string date { get; init; }
    public string amount { get; init; }
    public string receipt_id { get; init; }
    public string receipt_url { get; init; }
    public string completed { get; init; }
}