using BackendTemplate.Domain.Events;

namespace BackendTemplate.Domain.Events.Users;

public class PasswordExpiredEvent : IDomainEvent
{
    public Guid UserId { get; }
    public DateTime ExpiredAt { get; }

    public PasswordExpiredEvent(Guid userId, DateTime expiredAt)
    {
        UserId = userId;
        ExpiredAt = expiredAt;
    }
}
