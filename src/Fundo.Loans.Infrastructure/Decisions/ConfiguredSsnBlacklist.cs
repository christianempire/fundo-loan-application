using Fundo.Loans.Domain.Customers;
using Fundo.Loans.Domain.Decisions.Rules;
using Microsoft.Extensions.Options;

namespace Fundo.Loans.Infrastructure.Decisions;

/// <summary>
/// Reads the blacklist from configuration.
/// </summary>
/// <remarks>
/// A table would be the obvious next step once someone other than an engineer needs
/// to edit it; the rule does not change when that happens, only this adapter does.
/// Entries are normalized through <see cref="Ssn"/> so the list can be written with
/// or without dashes, and a malformed entry is ignored rather than crashing start-up.
/// </remarks>
internal sealed class ConfiguredSsnBlacklist : ISsnBlacklist
{
    private readonly HashSet<Ssn> _blacklisted;

    public ConfiguredSsnBlacklist(IOptions<DecisionRulesOptions> options) =>
        _blacklisted = [.. options.Value.BlacklistedSsns
            .Select(entry => Ssn.TryParse(entry, out var ssn) ? ssn : null)
            .OfType<Ssn>()];

    public bool Contains(Ssn ssn) => _blacklisted.Contains(ssn);
}
