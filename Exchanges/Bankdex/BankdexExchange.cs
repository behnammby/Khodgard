using Khodgard.Enumerations;
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
                return false;

            order.Uid = response.Data.Id;

            return true;
        }

        return false;
    }

    public override async Task<IEnumerable<Line>> GetDepthAsync(Market market, Map? map)
    {
        List<Line> lines = new();

        RestRequest request = new($"/peatio/public/markets/{market.ToString()}/depth");
        request.AddQueryParameter("limit", market.DepthLimit);

        var response = await _client.ExecuteAsync<BankdexDepth>(request);
        if (!response.IsSuccessful || response.Data is null)
            return lines;

        BankdexDepth depth = response.Data;

        foreach (var ask in depth.Asks)
            if (decimal.TryParse(ask[0], out decimal price))
                if (double.TryParse(ask[1], out double amount))
                    lines.Add(new(price, amount, OrderSide.Sell));

        foreach (var bid in depth.Bids)
            if (decimal.TryParse(bid[0], out decimal price))
                if (double.TryParse(bid[1], out double amount))
                    lines.Add(new(price, amount, OrderSide.Sell));

        return lines;
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