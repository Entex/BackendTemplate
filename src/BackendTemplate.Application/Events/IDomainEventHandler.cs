using BackendTemplate.Domain.Events;

namespace BackendTemplate.Application.Events;

public interface IDomainEventHandler
{
    Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
