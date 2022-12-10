using Khodgard.Enumerations;
using Khodgard.Exceptions;
using Khodgard.Models;
using Microsoft.EntityFrameworkCore;

namespace Khodgard.Data;

public class LinesRepo : RepoBase<Line>
{
    public LinesRepo(AppDbContext ctx) : base(ctx)
    {
    }

    public Line GetLineByPrice(Map map, decimal price, OrderSide? side = null)
    {
        try
        {
            IQueryable<Line> query = Context.Set<Line>().Where(_ => _.Map == map && _.Price == price).AsQueryable();
            if (side is not null)
                query = query.Where(_ => _.Side == side);

            return query.Single();
        }
        catch (InvalidOperationException)
        {
            throw new LineNotFoundException();
        }
    }
    public IEnumerable<Line> GetLinesByAge(Map map, int maxAge, int maxDeleteLines)
    {
        DateTime minAge = DateTime.UtcNow.AddMinutes(-maxAge);
        return Context.Set<Line>().Where(_ => _.Map.Id == map.Id && _.Updated < minAge).OrderBy(_ => _.Updated).Take(maxDeleteLines).ToList();
    }
    public IEnumerable<Line> GetLinesByCount(Map map, int maxLines, int maxDeleteLines)
    {
        try
        {
            var askLines = Context.Set<Line>().Where(_ => _.Map == map && _.Side == OrderSide.Ask)
                                                .OrderBy(_ => _.Price)
                                                .Skip(maxLines)
                                                .Take(maxDeleteLines / 2);

            var bidLines = Context.Set<Line>().Where(_ => _.Map.Id == map.Id && _.Side == OrderSide.Bid)
                                                .OrderByDescending(_ => _.Price)
                                                .Skip(maxLines)
                                                .Take(maxDeleteLines / 2);


            List<Line> lines = new();
            lines.AddRange(askLines);
            lines.AddRange(bidLines);

            return lines;
        }
        catch
        {
            return Enumerable.Empty<Line>();
        }
    }
    public async Task<int> ClearLineDeleteAvoidanceFlagAsync()
    {
        using var transaction = Context.Database.BeginTransaction();
        try
        {
            string sql = "UPDATE Line SET AvoidDelete = 0 WHERE 1";
            var result = await Context.Database.ExecuteSqlRawAsync(sql);

            await transaction.CommitAsync();

            return result;
        }
        catch
        { }

        return default;
    }
}