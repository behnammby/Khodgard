using Khodgard.Enumerations;

namespace Khodgard.Models;

public class Order
{
    public Order()
    {
        Line = new();
        Market = new();
    }

    public Order(decimal price, double amount, OrderSide side, Market market, Line line)
    {
        Price = price;
        Amount = amount;
        Side = side;
        Market = market;
        Line = line;
    }

    public int Id { get; set; }
    public int Uid { get; set; }
    public double Amount { get; set; }
    public decimal Price { get; set; }
    public OrderSide Side { get; set; }
    public Line Line { get; set; }
    public Market Market { get; set; }
    public bool Enabled { get; set; }
}