using BackendTemplate.Domain.Entities.Base;
using BackendTemplate.Domain.Events;
using BackendTemplate.Domain.Events.User;
using BackendTemplate.Domain.ValueObjects;

namespace BackendTemplate.Domain.Entities.User;

public class User : AuditableEntity, IAggregateRoot
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required Email Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }
    public bool IsEmailConfirmed { get; private set; }
    public bool IsLocked { get; private set; }
    public DateTime? LockedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int LoginFailedCount { get; private set; }
    public bool IsPasswordExpired { get; private set; }
    public bool IsSuperAdmin { get; private set; }

    public ICollection<UserRole> Roles { get; private set; } = [];
    public ICollection<UserPermission> Permissions { get; private set; } = [];

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public User(
        string firstName,
        string lastName,
        Email email,
        string passwordHash,
        string passwordSalt,
        bool isSuperAdmin = false
    )
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        IsSuperAdmin = isSuperAdmin;
        CreatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail() => IsEmailConfirmed = true;

    public void LockUser()
    {
        if (!IsLocked)
        {
            IsLocked = true;
            LockedAt = DateTime.UtcNow;
            _domainEvents.Add(new UserLockedEvent(Id, LockedAt.Value));
        }
    }

    public void IncrementFailedLoginAttempts()
    {
        LoginFailedCount++;
        if (LoginFailedCount >= 5)
        {
            LockUser();
        }
    }

    public void ChangePassword(string newPasswordHash, string newPasswordSalt)
    {
        PasswordHash = newPasswordHash;
        PasswordSalt = newPasswordSalt;
        IsPasswordExpired = false;
        _domainEvents.Add(new PasswordChangedEvent(Id, DateTime.UtcNow));
    }

    public void SetPasswordExpired()
    {
        if (!IsPasswordExpired)
        {
            IsPasswordExpired = true;
            _domainEvents.Add(new PasswordExpiredEvent(Id, DateTime.UtcNow));
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void AddRole(UserRole role)
    {
        if (!Roles.Contains(role))
        {
            Roles.Add(role);
        }
    }

    public void RemoveRole(UserRole role)
    {
        Roles.Remove(role);
    }

    public void AddPermission(UserPermission permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
        }
    }

    public void RemovePermission(UserPermission permission)
    {
        Permissions.Remove(permission);
    }

    public bool HasPermission(string permissionName)
    {
        if (IsSuperAdmin)
        {
            return true;
        }

        // Check both direct permissions and permissions from roles
        return Permissions.Any(p => p.Name == permissionName)
            || Roles.SelectMany(r => r.Permissions).Any(p => p.Name == permissionName);
    }
}
