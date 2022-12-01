using Khodgard.Models;

namespace Khodgard.Data;

public class ExchangesRepo : RepoBase<Exchange>
{
    public ExchangesRepo(AppDbContext ctx) : base(ctx)
    {
    }
}