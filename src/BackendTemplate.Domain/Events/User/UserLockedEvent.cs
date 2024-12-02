using BackendTemplate.Domain.Events;

namespace BackendTemplate.Domain.Events.User;

public class UserLockedEvent : IDomainEvent
{
    public Guid UserId { get; }
    public DateTime LockedAt { get; }

    public UserLockedEvent(Guid userId, DateTime lockedAt)
    {
        UserId = userId;
        LockedAt = lockedAt;
    }
}
