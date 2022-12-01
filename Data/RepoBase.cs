namespace Khodgard.Data;

public abstract class RepoBase<TEntity> where TEntity : class
{
    protected readonly AppDbContext Context;

    public RepoBase(AppDbContext ctx)
    {
        Context = ctx;
    }

    public void Create(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }
}