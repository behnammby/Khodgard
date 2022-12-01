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

    public Map GetAMapToRun()
    {
        try
        {
            return _ctx.Set<Map>()
                       .Where(_ => _.Enabled && !_.IsRunning)
                       .Single();
        }
        catch (InvalidOperationException)
        {
            throw new NoMapsToRunException();
        }
    }
    public Map GetMapById(int mapId)
    {
        try
        {
            return _ctx.Set<Map>().Where(_ => _.Id == mapId && _.Enabled)
                    .Include(_ => _.Source)
                    .ThenInclude(_ => _.Exchange)
                    .Include(_ => _.Target)
                    .ThenInclude(_ => _.Exchange)
                    .Single();
        }
        catch (InvalidOperationException)
        {
            throw new NoMapsToRunException();
        }
    }
    public async Task<bool> MarkMapAsRunning(Map map)
    {
        using var transaction = await _ctx.Database.BeginTransactionAsync();
        try
        {
            int result = await _ctx.Database.ExecuteSqlInterpolatedAsync($"UPDATE Map SET IsRunning = 1 WHERE Id = {map.Id} AND IsRunning = 0");

            await transaction.CommitAsync();

            return result == 1;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
        }

        return false;
    }
    public async Task<bool> ReleaseMap(Map map)
    {
        using var transaction = await _ctx.Database.BeginTransactionAsync();
        try
        {
            int result = await _ctx.Database.ExecuteSqlInterpolatedAsync($"UPDATE Map SET IsRunning = 0, LockedForSync = 0, LockedForClearByAge = 0, LockedForClearByCount = 0  WHERE Id = {map.Id}");

            await transaction.CommitAsync();

            return result != default;
        }
        catch
        {
            await transaction.RollbackAsync();
        }

        return false;
    }
    public async Task<bool> LockMap(Map map, LockMapType lockType)
    {
        using var transaction = await _ctx.Database.BeginTransactionAsync();
        try
        {
            string columnName = lockType.ToColumnName();
            string sql = string.Format("UPDATE Map SET {0} = 1 WHERE Id = {1} AND {2} = 0", columnName, map.Id, columnName);

            int result = await _ctx.Database.ExecuteSqlRawAsync(sql);

            await transaction.CommitAsync();

            return result == 1;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
        }

        return false;
    }
    public async Task<bool> UnlockMap(Map map, LockMapType lockType)
    {
        using var transaction = await _ctx.Database.BeginTransactionAsync();
        try
        {
            string columnName = lockType.ToColumnName();
            string sql = string.Format("UPDATE Map SET {0} = 0 WHERE Id = {1}", columnName, map.Id);

            int result = await _ctx.Database.ExecuteSqlRawAsync(sql);

            await transaction.CommitAsync();

            return result != default;
        }
        catch
        {
            await transaction.RollbackAsync();
        }

        return false;
    }
    public IEnumerable<Order> GetOrdersOfLine(Line line) => _ctx.Set<Order>().Where(_ => _.Line.Id == line.Id).ToList();
    public async Task ClearAllLocksAsync()
    {
        using var transaction = _ctx.Database.BeginTransaction();
        try
        {
            _ctx.Database.ExecuteSqlInterpolated($"UPDATE Map SET LockedForSync = 0, LockedForClearByAge = 0, LockedForClearByCount = 0, IsRunning = 0 WHERE 1");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
        }
    }
    public async Task CommitAsync() => await _ctx.SaveChangesAsync();
}