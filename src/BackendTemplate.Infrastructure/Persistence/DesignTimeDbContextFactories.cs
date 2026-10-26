using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendTemplate.Infrastructure.Persistence;

public sealed class SqliteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqliteDbContext>
{
    public SqliteDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<SqliteDbContext>()
            .UseSqlite(Environment.GetEnvironmentVariable("ConnectionStrings__Database") ?? "Data Source=backend.db")
            .Options);
}

public sealed class SqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__Database")
                ?? "Server=localhost;Database=BackendTemplate;Integrated Security=true;TrustServerCertificate=true")
            .Options);
}
