using Khodgard.Models;

namespace Khodgard.Data;

public class MapsRepo : RepoBase<Map>
{
    public MapsRepo(AppDbContext ctx) : base(ctx)
    {
    }
}