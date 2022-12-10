using Khodgard.Enumerations;
using Khodgard.Exceptions;
using Khodgard.Models;
using Khodgard.Utils;
using RestSharp;

namespace Khodgard.Exchanges.Bankdex;

public class BankdexExchange : Exchange
{
    private RestClient _client;

    public BankdexExchange()
    {
        _client = new();
    }
    public override async Task<bool> CancelOrderAsync(Order order)
    {
        if (order.Uid == default)
            return false;

        RestRequest request = new($"/peatio/market/orders/{order.Uid}/cancel", Method.Post);
        AddRequestHeaders(request);

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
            return true;
        else
            return false;
    }
    public override async Task<bool> CreateOrderAsync(Order order, int pricePrecision, int amountPrecision)
    {
        BankdexOrder bankdexOrder = new(order, pricePrecision, amountPrecision);

        if (bankdexOrder.Price <= 0 || bankdexOrder.Amount <= 0)
            return false;

        RestRequest request = new("/peatio/market/orders", Method.Post);
        request.AddJsonBody(bankdexOrder);
        AddRequestHeaders(request);

        var response = await _client.ExecuteAsync<BankdexCreatedOrder>(request);
        if (response.IsSuccessful)
        {
            if (response.Data is null)
                throw new CreateOrderException($"Response data is empty, Content= {response.Content}");

            order.Uid = response.Data.Id;
            return true;
        }

        throw new CreateOrderException(response.ErrorMessage + ", Content= " + response.Content, response.ErrorException);
    }
    public override async Task<IEnumerable<Line>> GetDepthAsync(Market market, int limit, Map? map)
    {
        List<Line> lines = new();

        RestRequest request = new($"/peatio/public/markets/{market.ToString()}/depth");
        request.AddQueryParameter("limit", limit);

        var response = await _client.ExecuteAsync<BankdexDepth>(request);
        if (!response.IsSuccessful || response.Data is null)
            return lines;

        BankdexDepth depth = response.Data;

        foreach (var ask in depth.Asks)
            if (decimal.TryParse(ask[0], out decimal price))
                if (double.TryParse(ask[1], out double amount))
                    lines.Add(new(price, amount, OrderSide.Ask));

        foreach (var bid in depth.Bids)
            if (decimal.TryParse(bid[0], out decimal price))
                if (double.TryParse(bid[1], out double amount))
                    lines.Add(new(price, amount, OrderSide.Bid));

        return lines;
    }
    public override async Task<IEnumerable<Trade>> GetTradesAsync(Market market, int limit)
    {
        RestRequest request = new($"/peatio/public/markets/{market.ToString()}/trades"); ;
        request.AddQueryParameter("limit", limit);

        var response = await _client.ExecuteAsync<IEnumerable<BankdexTrade>>(request);
        if (!response.IsSuccessful || response.Data is null)
            return Enumerable.Empty<Trade>();

        return response.Data.Select(_ => _.ToTrade(market));
    }
    public override void Init()
    {
        if (ApiKey is null || Url is null)
            return;

        RestClientOptions options = new()
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
            BaseUrl = new(Url)
        };

        _client = new(options);
        _client.AddDefaultHeader("X-Auth-Apikey", ApiKey);
        _client.AddDefaultHeader("Content-Type", "application/json");
    }
    private void AddRequestHeaders(RestRequest request)
    {
        if (ApiKey is null || Secret is null)
            return;

        string nonce = Math.Truncate(DateTimeHelper.ToUnixTimestamp() * 1000).ToString();
        string authSignature = CryptographyHelper.HmacSha256(Secret, nonce + ApiKey);

        request.AddHeader("X-Auth-Nonce", nonce);
        request.AddHeader("X-Auth-Signature", authSignature);
    }
}