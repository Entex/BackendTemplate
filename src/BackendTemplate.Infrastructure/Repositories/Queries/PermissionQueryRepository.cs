using BackendTemplate.Application.Interface.Repositories.Queries;
using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.Infrastructure.Repositories.Queries.Base;

using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Repositories;

public class PermissionQueryRepository : QueryRepository<Permission>, IPermissionQueryRepository
{
    public PermissionQueryRepository(IDbContextFactory<MyDbContext> dbContextFactory)
        : base(dbContextFactory) { }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Permissions.AnyAsync(
            p => p.Users.Any(u => u.Id == userId) && p.Name == permission
        );
    }
}
