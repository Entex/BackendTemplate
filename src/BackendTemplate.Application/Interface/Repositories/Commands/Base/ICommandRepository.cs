using System.Threading.Tasks;

using BackendTemplate.Domain.Entities.Base;

namespace BackendTemplate.Application.Interface.Repositories.Commands.Base;

public interface ICommandRepository<T>
    where T : Entity
{
    Task<T> AddAsync(T entity);

    Task<T> UpdateAsync(T entity);

    Task<bool> DeleteAsync(T entity);

    Task<bool> DeleteAsync(Guid id);

    Task<int> DeleteRangeAsync(IEnumerable<T> entities);

    Task<int> DeleteRangeAsync(params T[] entities);

    Task<int> DeleteRangeAsync(IEnumerable<Guid> ids);

    Task<int> DeleteRangeAsync(params Guid[] ids);

    Task<int> DeleteAllAsync();
}
