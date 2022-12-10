namespace Khodgard.Models;

public abstract class Exchange
{
    public Exchange()
    {
        Name = string.Empty;
        Url = string.Empty;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string? ApiKey { get; set; }
    public string? Secret { get; set; }
    public bool Enabled { get; set; }

    public abstract void Init();
    public abstract Task<IEnumerable<Line>> GetDepthAsync(Market market, int limit, Map? map = null);
    public abstract Task<IEnumerable<Trade>> GetTradesAsync(Market market, int limit);
    public abstract Task<bool> CreateOrderAsync(Order order, int pricePrecision, int amountPrecision);
    public abstract Task<bool> CancelOrderAsync(Order order);
}