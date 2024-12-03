using System.Linq.Expressions;

using BackendTemplate.Application.Interface.Repositories.Queries.Base;
using BackendTemplate.Domain.Entities.Base;
using BackendTemplate.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Repositories.Queries.Base;

public class QueryRepository<T> : IQueryRepository<T>
    where T : Entity
{
    protected readonly IDbContextFactory<MyDbContext> _dbContextFactory;

    public QueryRepository(IDbContextFactory<MyDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().CountAsync(predicate);
    }

    public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().FirstAsync(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().FindAsync(id);
    }

    public async Task<T> SingleAsync(Expression<Func<T, bool>> predicate)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().SingleAsync(predicate);
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<T>().SingleOrDefaultAsync(predicate);
    }
}
