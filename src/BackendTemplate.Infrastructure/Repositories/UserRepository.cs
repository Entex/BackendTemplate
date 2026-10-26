using BackendTemplate.Application.Abstractions;
using BackendTemplate.Application.Events;
using BackendTemplate.Application.Exceptions;
using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Domain.ValueObjects;
using BackendTemplate.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext db, IEnumerable<IDomainEventHandler> eventHandlers) : IUserRepository
{
    private IQueryable<User> UsersWithPermissions => db.Users
        .Include(x => x.Permissions).Include(x => x.Roles).ThenInclude(x => x.Permissions).AsSplitQuery();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        UsersWithPermissions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        var email = (Email)normalizedEmail;
        return UsersWithPermissions.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        var email = (Email)normalizedEmail;
        return db.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default) =>
        db.Users.AnyAsync(x => x.Id == userId && (x.IsSuperAdmin
            || x.Permissions.Any(p => p.Name == permission)
            || x.Roles.Any(r => r.Permissions.Any(p => p.Name == permission))), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        db.Users.Add(user);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var users = db.ChangeTracker.Entries<User>()
            .Select(x => x.Entity).Where(x => x.DomainEvents.Count > 0).ToArray();
        var events = users.SelectMany(x => x.DomainEvents).ToArray();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("This account changed during the request. Please retry.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is
            SqliteException { SqliteExtendedErrorCode: 2067 } or SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("A user with this email already exists.");
        }

        foreach (var user in users) user.ClearDomainEvents();
        // These are in-process, post-commit events. External delivery requires an outbox.
        foreach (var domainEvent in events)
            foreach (var handler in eventHandlers)
                await handler.HandleAsync(domainEvent, cancellationToken);
    }
}
