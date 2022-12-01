using Khodgard.Models;
using Microsoft.EntityFrameworkCore;

namespace Khodgard.Data;

public class OrdersRepo : RepoBase<Order>
{
    public OrdersRepo(AppDbContext ctx) : base(ctx)
    {
    }
}