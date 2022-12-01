using Khodgard.Enumerations;

namespace Khodgard.Models;

public class Line
{
    public Line()
    {
        Map = new();
    }

    public Line(decimal price, double amount, OrderSide side, Map? map = null) : this()
    {
        Side = side;
        Price = price;
        Amount = amount;

        Created = DateTime.UtcNow;
        Updated = DateTime.UtcNow;

        if (map is not null)
            Map = map;
    }

    public int Id { get; set; }
    public OrderSide Side { get; set; }
    public decimal Price { get; set; }
    public double Amount { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public Map Map { get; set; }

    public void ApplyRatio(double ratio) => Amount *= ratio;
}