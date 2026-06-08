using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CustomCodeFramework.Observability.Logging;

public static class SerilogExtensions
{
    public static IServiceCollection AddCustomCodeSerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        services.AddSingleton<LogEnricher>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(Log.Logger, dispose: true);
        });

        services.AddSingleton<ILogEventEnricherAdapter, LogEventEnricherAdapter>();

        return services;
    }
}

public interface ILogEventEnricherAdapter { }

internal sealed class LogEventEnricherAdapter : ILogEventEnricherAdapter { }
