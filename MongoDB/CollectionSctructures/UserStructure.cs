namespace TelegramBotWithPayment.MongoDB.CollectionSctructures;

public class UserStructure
{
    public string userid { get; init; }
    public string balance { get; init; }
    public string activeorder { get; init; }
    public string stage { get; init; }
}