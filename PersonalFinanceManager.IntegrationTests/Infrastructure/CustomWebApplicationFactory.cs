using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that substitutes SQL Server with an SQLite
/// in-memory database and skips the EF Core migration (uses EnsureCreated instead).
///
/// One persistent SqliteConnection is kept open for the factory's lifetime.
/// SQLite in-memory databases are destroyed when ALL connections close; since
/// each request scope opens and closes its own connection, without the
/// keep-alive connection the schema would vanish between requests.
///
/// xUnit requires IClassFixture to have a parameterless constructor.
/// Each per-class subclass (e.g. AuthFactory) provides a unique DB name via base().
/// </summary>
public abstract class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly string _connectionString;

    protected CustomWebApplicationFactory(string dbName)
    {
        _connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Tell Program.cs not to call MigrateDatabaseAsync (SQL Server DDL).
        builder.UseEnvironment("Testing");

        // Inject test JWT and CORS settings via UseSetting so they are available
        // when Program.cs binds JwtSettings during service registration.
        builder.UseSetting("Jwt:Key",           "pfm-integration-test-super-secret-key-min32");
        builder.UseSetting("Jwt:Issuer",         "pfm-test-issuer");
        builder.UseSetting("Jwt:Audience",       "pfm-test-audience");
        builder.UseSetting("Jwt:ExpiryMinutes",  "60");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost");

        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server DbContextOptions that AddInfrastructure registered.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDBContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Register EF Core with the shared-cache SQLite connection string.
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlite(_connectionString));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Create the schema using the EF model (not SQL Server migration files).
        using var scope = host.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDBContext>();
        ctx.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _keepAliveConnection.Dispose();
        base.Dispose(disposing);
    }
}

// ── Per-test-class factories (each with a unique DB name) ───────────────────

/// <summary>Factory for AuthEndpointsTests — isolated SQLite database.</summary>
public class AuthTestFactory : CustomWebApplicationFactory
{
    public AuthTestFactory() : base("pfm-auth-tests") { }
}

/// <summary>Factory for TransactionConsistencyTests — isolated SQLite database.</summary>
public class TransactionTestFactory : CustomWebApplicationFactory
{
    public TransactionTestFactory() : base("pfm-txn-tests") { }
}

/// <summary>Factory for CrossUserOwnershipTests — isolated SQLite database.</summary>
public class OwnershipTestFactory : CustomWebApplicationFactory
{
    public OwnershipTestFactory() : base("pfm-ownership-tests") { }
}
