using BackendTemplate.Domain.Events;

namespace BackendTemplate.Domain.Events.User;

public class UserRegisteredEvent : IDomainEvent
{
    public Guid UserId { get; }

    public UserRegisteredEvent(Guid userId)
    {
        UserId = userId;
    }
}
