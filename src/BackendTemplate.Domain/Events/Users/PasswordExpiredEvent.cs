namespace BackendTemplate.Domain.Events.Users;

public sealed record PasswordExpiredEvent(Guid UserId, DateTimeOffset ExpiredAt) : IDomainEvent;
