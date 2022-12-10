using System.Text.Json.Serialization;
using Khodgard.Enumerations;
using Khodgard.Models;
using Khodgard.Utils;

namespace Khodgard.Exchanges.Bankdex;

public class BankdexOrder
{
    public BankdexOrder(Order order, int pricePrecision, int amountPrecision)
    {
        Price = (decimal)NumberHelper.ModifyDecimalPlaces(order.Price, pricePrecision);
        Amount = (double)NumberHelper.ModifyDecimalPlaces(order.Amount, amountPrecision);
        Side = order.Side == OrderSide.Ask ? "sell" : "buy";
        Market = order.Market.ToString();
    }

    [JsonPropertyName("market")]
    public string Market { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("volume")]
    public double Amount { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}