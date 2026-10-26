namespace BackendTemplate.Domain.Events.Users;

public sealed record UserCreatedEvent(Guid UserId, string FirstName, string Email) : IDomainEvent;
