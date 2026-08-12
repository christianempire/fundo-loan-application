namespace Fundo.Loans.Infrastructure.Persistence.Outbox;

/// <summary>
/// An event waiting to be delivered to the outside world.
/// </summary>
/// <remarks>
/// This row is written in the same transaction as the customer and the application,
/// which is what makes "save both and publish" atomic without a distributed
/// transaction: either all three land, or none of them do. Delivery happens later,
/// in <c>OutboxProcessor</c>.
/// </remarks>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = null!;
        Payload = null!;
    }

    private OutboxMessage(string type, string payload)
    {
        Id = Guid.CreateVersion7();
        Type = type;
        Payload = payload;
        OccurredAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    /// <summary>The event contract name, so the processor knows how to read the payload.</summary>
    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public int AttemptCount { get; private set; }

    /// <summary>When the processor may try again. Null means "immediately".</summary>
    public DateTime? NextAttemptAt { get; private set; }

    public string? LastError { get; private set; }

    public static OutboxMessage For(string type, string payload) => new(type, payload);

    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        AttemptCount++;
        NextAttemptAt = null;
        LastError = null;
    }

    /// <summary>
    /// Records a failed delivery and schedules the retry. The caller owns the delay so
    /// the backoff policy stays in one place, in the processor. Giving up is not a state
    /// here: the processor stops picking a message up once its attempts run out.
    /// </summary>
    public void MarkFailed(string error, TimeSpan retryIn)
    {
        AttemptCount++;
        LastError = Truncate(error, maxLength: 1000);
        NextAttemptAt = DateTime.UtcNow.Add(retryIn);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
