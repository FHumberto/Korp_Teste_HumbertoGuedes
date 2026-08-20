namespace Korp.Faturamento.Api.Settings;

public sealed class RateLimiterSettings
{
    public int PermitLimit { get; set; } = 100;
    public int WindowInMinutes { get; set; } = 1;
    public int QueueLimit { get; set; } = 2;
}
