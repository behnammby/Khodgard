using System.Text.Json.Serialization;

namespace Khodgard.Exchanges.Bankdex;

public class BankdexDepth
{
    public BankdexDepth()
    {
        Asks = Array.Empty<string[]>();
        Bids = Array.Empty<string[]>();
    }

    [JsonPropertyName("asks")]
    public string[][] Asks { get; set; }

    [JsonPropertyName("bids")]
    public string[][] Bids { get; set; }

    [JsonPropertyName("timestamp")]
    public int TimeStamp { get; set; }
}