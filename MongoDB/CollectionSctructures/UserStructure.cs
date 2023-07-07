namespace TelegramBotWithPayment.MongoDB.CollectionSctructures;

public class UserStructure : ICollectionStructures
{
    public string userid { get; init; }
    public string balance { get; init; }
    public string active_order { get; init; }
    public string stage { get; init; }
    public string? current_value { get; init; }
}