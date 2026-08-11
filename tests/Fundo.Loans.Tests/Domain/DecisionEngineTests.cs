using Fundo.Loans.Domain.Customers;
using Fundo.Loans.Domain.Decisions;
using Fundo.Loans.Domain.Decisions.Rules;

namespace Fundo.Loans.Tests.Domain;

public class DecisionEngineTests
{
    private static DecisionEngine EngineWith(params IDenialRule[] rules) => new(rules);

    private static DecisionEngine ProductionEngine(params string[] blacklisted) =>
        EngineWith(
            new RestrictedStateRule(["NY"]),
            new BlacklistedSsnRule(new FakeBlacklist(blacklisted)));

    [Fact]
    public void Approves_when_no_rule_objects()
    {
        var decision = ProductionEngine().Evaluate(ApplicantFactory.Create());

        Assert.True(decision.IsApproved);
        Assert.Null(decision.Denial);
    }

    [Fact]
    public void Denies_an_applicant_from_a_restricted_state()
    {
        var decision = ProductionEngine().Evaluate(ApplicantFactory.Create(state: "NY"));

        Assert.False(decision.IsApproved);
        Assert.Equal(RestrictedStateRule.Code, decision.Denial!.Code);
    }

    [Theory]
    [InlineData("ny")]
    [InlineData("Ny")]
    public void Compares_states_without_caring_about_case(string state)
    {
        var decision = ProductionEngine().Evaluate(ApplicantFactory.Create(state: state));

        Assert.Equal(RestrictedStateRule.Code, decision.Denial!.Code);
    }

    [Fact]
    public void Denies_a_blacklisted_ssn()
    {
        var engine = ProductionEngine("999-88-7777");

        var decision = engine.Evaluate(ApplicantFactory.Create(ssn: "999-88-7777"));

        Assert.False(decision.IsApproved);
        Assert.Equal(BlacklistedSsnRule.Code, decision.Denial!.Code);
    }

    [Fact]
    public void Matches_a_blacklisted_ssn_however_it_was_typed()
    {
        var engine = ProductionEngine("999887777");

        var decision = engine.Evaluate(ApplicantFactory.Create(ssn: "999-88-7777"));

        Assert.Equal(BlacklistedSsnRule.Code, decision.Denial!.Code);
    }

    [Fact]
    public void Does_not_leak_the_blacklist_in_the_denial_reason()
    {
        var engine = ProductionEngine("999-88-7777");

        var decision = engine.Evaluate(ApplicantFactory.Create(ssn: "999-88-7777"));

        Assert.DoesNotContain("7777", decision.Denial!.Reason);
        Assert.DoesNotContain("blacklist", decision.Denial.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Stops_at_the_first_rule_that_objects()
    {
        var second = new SpyRule();
        var engine = EngineWith(new AlwaysDeniesRule(), second);

        engine.Evaluate(ApplicantFactory.Create());

        Assert.False(second.WasEvaluated);
    }

    [Fact]
    public void Picks_up_a_new_rule_without_any_change_to_the_existing_ones()
    {
        var engine = EngineWith(
            new RestrictedStateRule(["NY"]),
            new BlacklistedSsnRule(new FakeBlacklist()),
            new MinimumAmountRule(10_000m));

        var decision = engine.Evaluate(ApplicantFactory.Create());

        Assert.Equal("AMOUNT_TOO_LOW", decision.Denial!.Code);
    }

    private sealed class FakeBlacklist(params string[] ssns) : ISsnBlacklist
    {
        private readonly HashSet<Ssn> _ssns = [.. ssns.Select(Ssn.Parse)];

        public bool Contains(Ssn ssn) => _ssns.Contains(ssn);
    }

    private sealed class AlwaysDeniesRule : IDenialRule
    {
        public Denial? Evaluate(Applicant applicant) => new("ALWAYS", "Denied.");
    }

    private sealed class SpyRule : IDenialRule
    {
        public bool WasEvaluated { get; private set; }

        public Denial? Evaluate(Applicant applicant)
        {
            WasEvaluated = true;
            return null;
        }
    }

    /// <summary>A rule that does not exist in production, added here to prove the engine is open for extension.</summary>
    private sealed class MinimumAmountRule(decimal minimum) : IDenialRule
    {
        public Denial? Evaluate(Applicant applicant) => applicant.RequestedAmount < minimum
            ? new Denial("AMOUNT_TOO_LOW", $"We lend from {minimum:C} upwards.")
            : null;
    }
}
