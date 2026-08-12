using Fundo.Loans.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Fundo.Loans.Tests.Integration;

/// <summary>
/// Hosts the real API over a private in-memory SQLite database.
/// </summary>
/// <remarks>
/// SQLite rather than a fake persistence layer because the behaviour under test is
/// transactional, and a substitute that cannot roll back would prove nothing. The
/// connection is held open for the lifetime of the factory: a shared-cache in-memory
/// database exists only while someone is connected to it.
///
/// The outbox processor is removed so tests stay deterministic — they assert on what
/// the transaction wrote, not on when a background loop happened to run.
/// </remarks>
internal sealed class LoansApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"loans-tests-{Guid.NewGuid():N}";
    private readonly Action<IServiceCollection>? _configureServices;
    private SqliteConnection? _keepAlive;

    public LoansApiFactory(Action<IServiceCollection>? configureServices = null) =>
        _configureServices = configureServices;

    private string ConnectionString => $"Data Source={_databaseName};Mode=Memory;Cache=Shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _keepAlive = new SqliteConnection(ConnectionString);
        _keepAlive.Open();

        builder.UseSetting("ConnectionStrings:Loans", ConnectionString);
        builder.UseSetting("SsnHashing:Key", "test-key");
        builder.UseSetting("DecisionRules:RestrictedStates:0", "NY");
        builder.UseSetting("DecisionRules:BlacklistedSsns:0", "111-11-1111");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            _configureServices?.Invoke(services);
        });
    }

    /// <summary>Reads the database back the way the application wrote it.</summary>
    public async Task<T> QueryAsync<T>(Func<LoansDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<LoansDbContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _keepAlive?.Dispose();
        }
    }
}
