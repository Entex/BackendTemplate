using BackendTemplate.Domain.Entities.Users;
using BackendTemplate.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Persistence;

public abstract class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).ValueGeneratedNever();
        user.Property(x => x.FirstName).HasMaxLength(100);
        user.Property(x => x.LastName).HasMaxLength(100);
        user.Property(x => x.Email).HasConversion(x => x.Value, value => (Email)value).HasMaxLength(254);
        user.HasIndex(x => x.Email).IsUnique();
        user.Property(x => x.PasswordHash).HasMaxLength(1024);
        user.Property(x => x.Version).IsConcurrencyToken();
        user.Ignore(x => x.DomainEvents);
        user.HasMany(x => x.Roles).WithMany(x => x.Users).UsingEntity(x => x.ToTable("UserRoles"));
        user.HasMany(x => x.Permissions).WithMany(x => x.Users).UsingEntity(x => x.ToTable("UserPermissions"));

        modelBuilder.Entity<Role>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Role>().Property(x => x.Name).HasMaxLength(100);
        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Role>().Property(x => x.Description).HasMaxLength(500);
        modelBuilder.Entity<Role>().HasMany(x => x.Permissions).WithMany(x => x.Roles)
            .UsingEntity(x => x.ToTable("RolePermissions"));

        modelBuilder.Entity<Permission>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Permission>().Property(x => x.Name).HasMaxLength(100);
        modelBuilder.Entity<Permission>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Permission>().Property(x => x.Description).HasMaxLength(500);
    }
}

public sealed class SqliteDbContext(DbContextOptions<SqliteDbContext> options) : AppDbContext(options);

public sealed class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : AppDbContext(options);
