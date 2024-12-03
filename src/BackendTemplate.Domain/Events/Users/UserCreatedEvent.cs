using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Domain.Events;
using BackendTemplate.Domain.ValueObjects;

namespace BackendTemplate.Domain.Events.Users;

public class UserCreatedEvent : IDomainEvent
{
    public User User { get; }

    public UserCreatedEvent(User user)
    {
        User = user;
    }
}
