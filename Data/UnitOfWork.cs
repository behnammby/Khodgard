using Khodgard.Enumerations;
using Khodgard.Exceptions;
using Khodgard.Extensions;
using Khodgard.Models;
using Microsoft.EntityFrameworkCore;

namespace Khodgard.Data;

public class UnitOfWork
{
    private readonly AppDbContext _ctx;

    public UnitOfWork(AppDbContext ctx)
    {
        _ctx = ctx;

        ExchangesRepo = new(_ctx);
        MarketsRepo = new(_ctx);
        OrdersRepo = new(_ctx);
        LinesRepo = new(_ctx);
        MapsRepo = new(_ctx);
    }

    public ExchangesRepo ExchangesRepo { get; init; }
    public MarketsRepo MarketsRepo { get; init; }
    public OrdersRepo OrdersRepo { get; init; }
    public LinesRepo LinesRepo { get; init; }
    public MapsRepo MapsRepo { get; init; }
    
    public async Task CommitAsync()
    {
        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch { }
    }
}