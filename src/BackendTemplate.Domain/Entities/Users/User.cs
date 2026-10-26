using System.Text.Json.Serialization;

using BackendTemplate.Domain.Entities.Base;
using BackendTemplate.Domain.Events;
using BackendTemplate.Domain.Events.Users;
using BackendTemplate.Domain.ValueObjects;

namespace BackendTemplate.Domain.Entities.Users;

public sealed class User : AuditableEntity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private User() { }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    [JsonIgnore]
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; }
    public DateTimeOffset? LockoutEnd { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public int LoginFailedCount { get; private set; }
    public bool IsPasswordExpired { get; private set; }
    public bool IsSuperAdmin { get; private set; }
    public long Version { get; private set; }
    public ICollection<Role> Roles { get; private set; } = [];
    public ICollection<Permission> Permissions { get; private set; } = [];
    [JsonIgnore]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static User Create(string firstName, string lastName, Email email, string passwordHash, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = now
        };
        user._domainEvents.Add(new UserCreatedEvent(user.Id, user.FirstName, email.Value));
        return user;
    }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEnd > now;

    public void RecordFailedLogin(DateTimeOffset now)
    {
        if (LockoutEnd <= now)
        {
            LoginFailedCount = 0;
            LockoutEnd = null;
        }

        LoginFailedCount++;
        Version++;
        LastModifiedAt = now;
        if (LoginFailedCount >= 5)
        {
            LockoutEnd = now.AddMinutes(15);
            _domainEvents.Add(new UserLockedEvent(Id, now));
        }
    }

    public void RecordSuccessfulLogin(DateTimeOffset now)
    {
        LoginFailedCount = 0;
        LockoutEnd = null;
        LastLoginAt = now;
        LastModifiedAt = now;
        Version++;
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        Version++;
    }

    public void ChangePassword(string passwordHash, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        IsPasswordExpired = false;
        LastModifiedAt = now;
        Version++;
        _domainEvents.Add(new PasswordChangedEvent(Id, now));
    }

    public void SetPasswordExpired(DateTimeOffset now)
    {
        if (!IsPasswordExpired)
        {
            IsPasswordExpired = true;
            LastModifiedAt = now;
            Version++;
            _domainEvents.Add(new PasswordExpiredEvent(Id, now));
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void AddRole(Role role)
    {
        if (!Roles.Contains(role)) Roles.Add(role);
    }

    public void RemoveRole(Role role) => Roles.Remove(role);

    public void AddPermission(Permission permission)
    {
        if (!Permissions.Contains(permission)) Permissions.Add(permission);
    }

    public void RemovePermission(Permission permission) => Permissions.Remove(permission);

    public bool HasPermission(string name) => IsSuperAdmin
        || Permissions.Any(p => p.Name == name)
        || Roles.SelectMany(r => r.Permissions).Any(p => p.Name == name);
}
