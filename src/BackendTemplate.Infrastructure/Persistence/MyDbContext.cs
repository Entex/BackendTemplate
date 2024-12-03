using BackendTemplate.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Infrastructure.Persistence;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("UserRoles"));

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Permissions)
            .WithMany(p => p.Users)
            .UsingEntity(j => j.ToTable("UserPermissions"));

        modelBuilder
            .Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity(j => j.ToTable("RolePermissions"));
    }
}
