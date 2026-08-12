namespace Fundo.Loans.Infrastructure.Persistence.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int PollingIntervalSeconds { get; set; } = 2;

    public int BatchSize { get; set; } = 20;

    /// <summary>Deliveries to attempt before the message is left alone for a human.</summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>First retry delay; it doubles with each failed attempt.</summary>
    public int BaseRetryDelaySeconds { get; set; } = 5;
}
