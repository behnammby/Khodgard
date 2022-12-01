using Khodgard.Enumerations;
using Khodgard.Exceptions;
using Khodgard.Models;

namespace Khodgard.Data;

public class LinesRepo : RepoBase<Line>
{
    public LinesRepo(AppDbContext ctx) : base(ctx)
    {
    }

    public Line GetLineByPrice(Map map, decimal price)
    {
        try
        {
            return Context.Set<Line>().Single(_ => _.Map.Id == map.Id && _.Price == price);
        }
        catch (InvalidOperationException)
        {
            throw new LineNotFoundException();
        }
    }
    public IEnumerable<Line> GetLinesByAge(Map map, int maxAge)
    {
        DateTime minAge = DateTime.UtcNow.AddMinutes(-maxAge);
        return Context.Set<Line>().Where(_ => _.Map.Id == map.Id && _.Updated < minAge).ToList();
    }
    public IEnumerable<Line> GetLinesByCount(Map map, int maxLines)
    {
        var askLines = Context.Set<Line>().Where(_ => _.Map.Id == map.Id && _.Side == OrderSide.Sell)
                                            .OrderBy(_ => (int)_.Price)
                                            .Skip(maxLines);

        var bidLines = Context.Set<Line>().Where(_ => _.Map.Id == map.Id && _.Side == OrderSide.Buy)
                                            .OrderByDescending(_ => (int)_.Price)
                                            .Skip(maxLines);

        List<Line> lines = new();
        lines.AddRange(askLines);
        lines.AddRange(bidLines);

        return lines;
    }
}