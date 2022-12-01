using System.Text.Json.Serialization;

namespace Khodgard.Exchanges.Bankdex;

public class BankdexDepthItem
{
    public BankdexDepthItem()
    {
        PriceAsStr = string.Empty;
        AmountAsStr = string.Empty;
    }

    [JsonPropertyOrder(1)]
    public string PriceAsStr { get; set; }

    [JsonIgnore]
    public decimal Price => decimal.TryParse(PriceAsStr, out decimal price) ? price : default;

    [JsonPropertyOrder(2)]
    public string AmountAsStr { get; set; }

    [JsonIgnore]
    public double Amount => double.TryParse(AmountAsStr, out double amount) ? amount : default;
}