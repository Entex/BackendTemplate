using BackendTemplate.Domain.Events;

namespace BackendTemplate.Domain.Events.Users;

public class PasswordChangedEvent : IDomainEvent
{
    public Guid UserId { get; }
    public DateTime ChangedAt { get; }

    public PasswordChangedEvent(Guid userId, DateTime changedAt)
    {
        UserId = userId;
        ChangedAt = changedAt;
    }
}
