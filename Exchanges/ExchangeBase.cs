using Khodgard.Models;

namespace Khodgard.Exchanges;

public class ExchangeBase : Exchange
{
    public override Task<bool> CancelOrderAsync(Order order)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> CreateOrderAsync(Order order, int pricePrecision, int volumePrecision)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Line>> GetDepthAsync(Market market, int limit, Map? map = null)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Trade>> GetTradesAsync(Market market, int limit)
    {
        throw new NotImplementedException();
    }

    public override void Init()
    {
        throw new NotImplementedException();
    }
}