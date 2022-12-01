using Khodgard.Models;

namespace Khodgard.Data;

public class MarketsRepo : RepoBase<Market>
{
    public MarketsRepo(AppDbContext ctx) : base(ctx)
    {
    }
}