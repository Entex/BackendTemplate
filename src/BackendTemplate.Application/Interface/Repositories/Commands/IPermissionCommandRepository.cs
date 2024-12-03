using BackendTemplate.Application.Interface.Repositories.Commands.Base;
using BackendTemplate.Domain.Entities.Users;

namespace BackendTemplate.Application.Interface.Repositories.Commands;

public interface IPermissionCommandRepository : ICommandRepository<Permission> { }
