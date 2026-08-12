namespace Fundo.Loans.Infrastructure.Security;

public sealed class SsnHashingOptions
{
    public const string SectionName = "SsnHashing";

    /// <summary>
    /// HMAC key. Committed in appsettings for this exercise so the app runs out of the
    /// box; in production it would come from a secret store, and rotating it would need
    /// a rehash of the stored column.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}
