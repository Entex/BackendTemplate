using BackendTemplate.Application.Interface.Repositories.Queries.Base;
using BackendTemplate.Domain.Entities.Users;

namespace BackendTemplate.Application.Interface.Repositories.Queries;

public interface IPermissionQueryRepository : IQueryRepository<Permission>
{
    Task<bool> HasPermissionAsync(Guid userId, string permission);
}
