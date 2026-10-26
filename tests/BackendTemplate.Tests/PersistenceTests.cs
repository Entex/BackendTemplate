using BackendTemplate.Application.Exceptions;
using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Domain.ValueObjects;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BackendTemplate.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public void BothProvidersHaveCurrentMigrationSnapshots()
    {
        using var sqlite = new SqliteDbContext(new DbContextOptionsBuilder<SqliteDbContext>()
            .UseSqlite("Data Source=:memory:").Options);
        using var sqlServer = new SqlServerDbContext(new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseSqlServer("Server=localhost;Database=Unused;Integrated Security=true").Options);
        Assert.False(sqlite.Database.HasPendingModelChanges());
        Assert.False(sqlServer.Database.HasPendingModelChanges());
        Assert.NotEmpty(sqlite.Database.GetMigrations());
        var script = sqlServer.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
        Assert.Contains("CREATE TABLE [Users]", script);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Users_Email]", script);
    }

    [Fact]
    public async Task ConcurrentAccountUpdatesCannotOverwriteLoginAttempts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SqliteDbContext>().UseSqlite(connection).Options;
        await using var first = new SqliteDbContext(options);
        await first.Database.MigrateAsync();
        var firstRepository = new UserRepository(first, []);
        var now = DateTimeOffset.UtcNow;
        var user = User.Create("Alice", "Example", (Email)"alice@example.com", "test-hash", now);
        await firstRepository.AddAsync(user);

        await using var second = new SqliteDbContext(options);
        var secondRepository = new UserRepository(second, []);
        var staleUser = await secondRepository.GetByIdAsync(user.Id);
        user.RecordFailedLogin(now);
        staleUser!.RecordFailedLogin(now);
        await firstRepository.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => secondRepository.SaveChangesAsync());

        await using var verification = new SqliteDbContext(options);
        Assert.Equal(1, (await verification.Users.SingleAsync()).LoginFailedCount);
    }
}
