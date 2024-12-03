using BackendTemplate.Application.Interface.Repositories.Commands;
using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.Infrastructure.Repositories.Commands.Base;

using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Repositories.Commands;

public class PermissionCommandRepository
    : CommandRepository<Permission>,
        IPermissionCommandRepository
{
    public PermissionCommandRepository(IDbContextFactory<MyDbContext> dbContextFactory)
        : base(dbContextFactory) { }
}
