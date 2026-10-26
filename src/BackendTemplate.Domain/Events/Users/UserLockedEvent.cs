namespace BackendTemplate.Domain.Events.Users;

public sealed record UserLockedEvent(Guid UserId, DateTimeOffset LockedAt) : IDomainEvent;
