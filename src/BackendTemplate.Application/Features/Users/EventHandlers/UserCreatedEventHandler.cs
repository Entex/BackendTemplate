using BackendTemplate.Application.Events;
using BackendTemplate.Domain.Events;
using BackendTemplate.Domain.Events.Users;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Application.Features.Users.EventHandlers;

public sealed class UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger) : IDomainEventHandler
{
    public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent is UserCreatedEvent created)
            logger.LogInformation("User {UserId} registered", created.UserId);
        return Task.CompletedTask;
    }
}
