using Binance.Net.Clients;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Khodgard.Enumerations;
using Khodgard.Models;

namespace Khodgard.Exchanges.Binance;

public class BinanceExchange : Exchange
{
    private BinanceClient _client;

    public BinanceExchange()
    {
        _client = new();
    }

    public override Task<bool> CancelOrderAsync(Order order)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> CreateOrderAsync(Order order, int pricePrecision, int amountPrecision)
    {
        throw new NotImplementedException();
    }

    public override async Task<IEnumerable<Line>> GetDepthAsync(Market market, Map? map = null)
    {
        List<Line> lines = new();
        var response = await _client.SpotApi.ExchangeData.GetOrderBookAsync(market.ToUpperString(), market.DepthLimit);
        if (!response.Success)
            return lines;

        var depth = response.Data;
        foreach (var ask in depth.Asks)
            lines.Add(new(ask.Price, (double)ask.Quantity, OrderSide.Sell, map));

        foreach (var bid in depth.Bids)
            lines.Add(new(bid.Price, (double)bid.Quantity, OrderSide.Buy, map));

        return lines;
    }

    public override void Init()
    {
        if (ApiKey is null || Secret is null)
            return;

        BinanceClientOptions options = new();
        ApiCredentials apiCred = new(ApiKey, Secret);

        options.ApiCredentials = apiCred;
    }
}