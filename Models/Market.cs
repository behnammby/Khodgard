using Khodgard.Exchanges;

namespace Khodgard.Models;

public class Market
{
    public Market()
    {
        Name = string.Empty;
        BaseUnit = string.Empty;
        QuoteUnit = string.Empty;

        Exchange = new ExchangeBase();
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string BaseUnit { get; set; }
    public string QuoteUnit { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int AmountPrecision { get; set; }
    public int PricePrecision { get; set; }
    public int DepthLimit { get; set; }
    public Exchange Exchange { get; set; }
    public bool Enabled { get; set; }

    public override string ToString() => (BaseUnit + QuoteUnit).ToLower();
    public string ToUpperString() => ToString().ToUpper();
}