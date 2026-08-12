using Fundo.Loans.Application.Abstractions;
using Fundo.Loans.Application.LoanApplications;
using Fundo.Loans.Domain.Decisions;
using Fundo.Loans.Domain.Decisions.Rules;
using Fundo.Loans.Infrastructure.Decisions;
using Fundo.Loans.Infrastructure.ExternalService;
using Fundo.Loans.Infrastructure.Persistence;
using Fundo.Loans.Infrastructure.Persistence.Outbox;
using Fundo.Loans.Infrastructure.Persistence.Repositories;
using Fundo.Loans.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fundo.Loans.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLoansInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<LoansDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Loans")));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIntegrationEventPublisher, OutboxEventPublisher>();

        services.AddOptions<SsnHashingOptions>()
            .Bind(configuration.GetSection(SsnHashingOptions.SectionName));
        services.AddSingleton<ISsnHasher, HmacSsnHasher>();

        services.AddDecisionRules(configuration);
        services.AddOutboxDelivery(configuration);

        services.AddScoped<SubmitLoanApplicationHandler>();

        return services;
    }

    /// <summary>
    /// The delivery half of the outbox: the client that talks to the external service
    /// and the background service that drains the table into it.
    /// </summary>
    private static IServiceCollection AddOutboxDelivery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName));

        var externalService = configuration
            .GetSection(ExternalServiceOptions.SectionName)
            .Get<ExternalServiceOptions>() ?? new ExternalServiceOptions();

        services.AddHttpClient<ICustomerSyncClient, HttpCustomerSyncClient>(client =>
        {
            client.BaseAddress = new Uri(externalService.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(externalService.TimeoutSeconds);
        });

        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    /// <summary>
    /// Registers the rule engine. A new rule is added here and nowhere else: the engine
    /// takes whatever <see cref="IDenialRule"/> implementations are registered.
    /// </summary>
    private static IServiceCollection AddDecisionRules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DecisionRulesOptions>()
            .Bind(configuration.GetSection(DecisionRulesOptions.SectionName));

        services.AddSingleton<ISsnBlacklist, ConfiguredSsnBlacklist>();

        services.AddSingleton<IDenialRule>(provider =>
            new RestrictedStateRule(
                provider.GetRequiredService<IOptions<DecisionRulesOptions>>().Value.RestrictedStates));
        services.AddSingleton<IDenialRule, BlacklistedSsnRule>();

        services.AddSingleton<DecisionEngine>();

        return services;
    }
}
