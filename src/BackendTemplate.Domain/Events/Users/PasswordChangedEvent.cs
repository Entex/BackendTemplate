namespace BackendTemplate.Domain.Events.Users;

public sealed record PasswordChangedEvent(Guid UserId, DateTimeOffset ChangedAt) : IDomainEvent;
