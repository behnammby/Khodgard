using Khodgard.Enumerations;

namespace Khodgard.Models;

public class Trade
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public double Amount { get; set; }
    public double Total { get; set; }
    public Market? Market { get; set; }
    public DateTime Created { get; set; }
    public TakerType TakerType { get; set; }
}