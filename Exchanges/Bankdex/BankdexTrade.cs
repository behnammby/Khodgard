using System.Text.Json.Serialization;
using Khodgard.Models;

namespace Khodgard.Exchanges.Bankdex;

public class BankdexTrade
{
    public BankdexTrade()
    {
        Market = string.Empty;
        TakerType = string.Empty;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("total")]
    public double Total { get; set; }

    [JsonPropertyName("market")]
    public string Market { get; set; }

    [JsonPropertyName("created_at")]
    public int CreatedAt { get; set; }

    [JsonPropertyName("taker_type")]
    public string TakerType { get; set; }

    public Trade ToTrade(Market market) => new()
    {
        Id = Id,
        Price = Price,
        Amount = Amount,
        Total = Total,
        Market = market,
        Created = DateTime.UtcNow,
        TakerType = TakerType == "sell" ? Enumerations.TakerType.Sell : Enumerations.TakerType.Buy
    };
}