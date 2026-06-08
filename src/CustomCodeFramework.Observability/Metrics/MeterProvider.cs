using System.Diagnostics.Metrics;

namespace CustomCodeFramework.Observability.Metrics;

public static class CustomCodeMeterProvider
{
    public const string DefaultMeterName = "CustomCodeFramework";

    public static readonly Meter Default = new(DefaultMeterName);

    public static Meter Create(string meterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(meterName);

        return new Meter(meterName);
    }
}
