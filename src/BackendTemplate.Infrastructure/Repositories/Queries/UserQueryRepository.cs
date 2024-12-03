using BackendTemplate.Application.Interface.Repositories.Queries;
using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.Infrastructure.Repositories.Queries.Base;

using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Repositories.Queries;

public class UserQueryRepository : QueryRepository<User>, IUserQueryRepository
{
    public UserQueryRepository(IDbContextFactory<MyDbContext> dbContextFactory)
        : base(dbContextFactory) { }
}
