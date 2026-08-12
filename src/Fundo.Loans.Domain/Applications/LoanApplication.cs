namespace Fundo.Loans.Domain.Applications;

/// <summary>
/// An approved request for money. A customer has at most one, updated in place
/// when they apply again.
/// </summary>
public sealed class LoanApplication
{
    private LoanApplication()
    {
        // Rehydration by EF Core.
    }

    private LoanApplication(Guid customerId, decimal requestedAmount)
    {
        Id = Guid.CreateVersion7();
        CustomerId = customerId;
        RequestedAmount = requestedAmount;
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = SubmittedAt;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public decimal RequestedAmount { get; private set; }

    public DateTime SubmittedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static LoanApplication Open(Guid customerId, decimal requestedAmount)
    {
        if (requestedAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedAmount), requestedAmount, "The requested amount must be positive.");
        }

        return new LoanApplication(customerId, requestedAmount);
    }

    public void UpdateRequestedAmount(decimal requestedAmount)
    {
        if (requestedAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedAmount), requestedAmount, "The requested amount must be positive.");
        }

        RequestedAmount = requestedAmount;
        UpdatedAt = DateTime.UtcNow;
    }
}
