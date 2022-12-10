using Khodgard.Enumerations;
using Khodgard.Exceptions;
using Khodgard.Extensions;
using Khodgard.Models;
using Microsoft.EntityFrameworkCore;

namespace Khodgard.Data;

public class MapsRepo : RepoBase<Map>
{
    public MapsRepo(AppDbContext ctx) : base(ctx)
    {
    }

    public Map GetAMapToRun()
    {
        try
        {
            return Context.Set<Map>()
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
            return Context.Set<Map>().Where(_ => _.Id == mapId && _.Enabled)
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
        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            int result = await Context.Database.ExecuteSqlInterpolatedAsync($"UPDATE `Map` SET IsRunning = 1 WHERE Id = {map.Id} AND IsRunning = 0 LIMIT 1");

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
        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            int result = await Context.Database.ExecuteSqlInterpolatedAsync($"UPDATE `Map` SET IsRunning = 0, LockedSync = 0, LockedClearAge = 0, LockedClearCount = 0  WHERE Id = {map.Id} LIMIT 1");

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
        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            string columnName = lockType.ToColumnName();
            string sql = string.Format("UPDATE `Map` SET {0} = 1 WHERE Id = {1} AND {2} = 0 LIMIT 1", columnName, map.Id, columnName);

            int result = await Context.Database.ExecuteSqlRawAsync(sql);

            await transaction.CommitAsync();

            return result == 1;
        }
        catch
        {
            await transaction.RollbackAsync();
        }

        return false;
    }
    public async Task<bool> UnlockMap(Map map, LockMapType lockType)
    {
        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            string columnName = lockType.ToColumnName();
            string sql = string.Format("UPDATE `Map` SET {0} = 0 WHERE Id = {1}  LIMIT 1", columnName, map.Id);

            int result = await Context.Database.ExecuteSqlRawAsync(sql);

            await transaction.CommitAsync();

            return result != default;
        }
        catch
        {
            await transaction.RollbackAsync();
        }

        return false;
    }
    public async Task ClearAllLocksAsync()
    {
        using var transaction = Context.Database.BeginTransaction();
        try
        {
            Context.Database.ExecuteSqlInterpolated($"UPDATE `Map` SET LockedSync = 0, LockedClearAge = 0, LockedClearCount = 0, IsRunning = 0 WHERE 1");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
        }
    }
}