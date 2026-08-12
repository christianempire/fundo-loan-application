namespace Fundo.Loans.Infrastructure.ExternalService;

public sealed class ExternalServiceOptions
{
    public const string SectionName = "ExternalService";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 10;
}
