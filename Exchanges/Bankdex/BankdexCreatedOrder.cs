using System.Text.Json.Serialization;

namespace Khodgard.Exchanges.Bankdex;

public class BankdexCreatedOrder
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}