using Khodgard.Models;

namespace Khodgard.Data;

public class OrdersRepo : RepoBase<Order>
{
    public OrdersRepo(AppDbContext ctx) : base(ctx)
    {
    }

    public IEnumerable<Order> GetOrdersOfLine(Line line) => Context.Set<Order>().Where(_ => _.Line.Id == line.Id).ToList();
}