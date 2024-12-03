using BackendTemplate.Application.Interface.Repositories.Commands.Base;
using BackendTemplate.Domain.Entities.Base;
using BackendTemplate.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Repositories.Commands.Base;

public class CommandRepository<T> : ICommandRepository<T>
    where T : Entity
{
    protected readonly IDbContextFactory<MyDbContext> _contextFactory;

    public CommandRepository(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<T> AddAsync(T entity)
    {
        await using var context = _contextFactory.CreateDbContext();
        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var context = _contextFactory.CreateDbContext();
        var entity = await context.Set<T>().FindAsync(id);
        if (entity == null)
            return false;
        context.Set<T>().Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Set<T>().Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<int> DeleteRangeAsync(IEnumerable<T> entities)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Set<T>().RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteRangeAsync(params T[] entities)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Set<T>().RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteRangeAsync(IEnumerable<Guid> ids)
    {
        await using var context = _contextFactory.CreateDbContext();
        var entities = await context.Set<T>().Where(x => ids.Contains(x.Id)).ToListAsync();
        context.Set<T>().RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteRangeAsync(params Guid[] ids)
    {
        await using var context = _contextFactory.CreateDbContext();
        var entities = await context.Set<T>().Where(x => ids.Contains(x.Id)).ToListAsync();
        context.Set<T>().RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteAllAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Set<T>().RemoveRange(context.Set<T>());
        return await context.SaveChangesAsync();
    }
}
