using Telemetry.Constants;
using Telemetry.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Telemetry.Utils;

/// <summary>
/// Class to encapsulate static methods for telemetry initialization.
/// Created to resolve SonarQube scan issues about duplicated code.
/// </summary>
public static class TelemetryUtils
{
    /// <summary>
    /// Builds telemetry configuration from config file variables.
    /// This is called by Web Applications (MatchingApi and QueueProcessor) to construct TelemetryConfig.
    /// </summary>
    /// <param name="config">The configuration containing telemetry settings.</param>
    /// <returns>An instance of IInstrumentation.</returns>
    public static TelemetryConfig BuildTelemetryConfig(IConfiguration config)
    {
        var telemetryConfig = new TelemetryConfig
        {
            ResourceComponent = config.GetValue(ConfigConstants.ResourceComponent, string.Empty),
            ResourceVersion = config.GetValue(ConfigConstants.ResourceVersion, string.Empty),
            ResourceWebsiteName = config.GetValue(ConfigConstants.ResourceWebsiteName, string.Empty),
            ResourceWebsiteInstance = config.GetValue(ConfigConstants.ResourceWebsiteInstance, string.Empty),
            ResourceEnvironment = config.GetValue(ConfigConstants.ResourceEnvironment, string.Empty),
            ResourceRegion = config.GetValue(ConfigConstants.ResourceRegion, string.Empty),
            EnableConsoleExporter = config.GetValue(ConfigConstants.EnableConsoleExporter, false),
            EnableAppInsightsExporter = config.GetValue(ConfigConstants.EnableAppInsightsExporter, false),
            AppInsightsConnectionString = config.GetValue(ConfigConstants.AppInsightsConnectionString, string.Empty),
            LogLevel = config.GetValue(ConfigConstants.LogLevel, LogLevel.Information),
        };
        return telemetryConfig;
    }

    /// <summary>
    /// Builds telemetry configuration from environment variables.
    /// This is called by Azure Functions apps (JobMonitor and DLQ) to construct TelemetryConfig.
    /// </summary>
    public static TelemetryConfig BuildTelemetryConfigFromEnvVars()
    {
        var telemetryConfig = new TelemetryConfig
        {
            ResourceComponent = Environment.GetEnvironmentVariable(ConfigConstants.ResourceComponent) ?? string.Empty,
            ResourceVersion = Environment.GetEnvironmentVariable(ConfigConstants.ResourceVersion) ?? string.Empty,
            ResourceWebsiteName = Environment.GetEnvironmentVariable(ConfigConstants.ResourceWebsiteName) ?? string.Empty,
            ResourceWebsiteInstance = Environment.GetEnvironmentVariable(ConfigConstants.ResourceWebsiteInstance) ?? string.Empty,
            ResourceEnvironment = Environment.GetEnvironmentVariable(ConfigConstants.ResourceEnvironment) ?? string.Empty,
            ResourceRegion = Environment.GetEnvironmentVariable(ConfigConstants.ResourceRegion) ?? string.Empty,
            EnableConsoleExporter = Environment.GetEnvironmentVariable(ConfigConstants.EnableConsoleExporter) == "true",
            EnableAppInsightsExporter = Environment.GetEnvironmentVariable(ConfigConstants.EnableAppInsightsExporter) == "true",
            AppInsightsConnectionString = Environment.GetEnvironmentVariable(ConfigConstants.AppInsightsConnectionString) ?? string.Empty,
            LogLevel = Enum.TryParse<LogLevel>(Environment.GetEnvironmentVariable(ConfigConstants.LogLevel) ?? string.Empty, out var logLevel)
                ? logLevel
                : LogLevel.Information,
        };
        return telemetryConfig;
    }

    /// <summary>
    /// Configures the trace provider with the specified trace sources.
    /// </summary>
    public static void ConfigureTraceSource(TracerProviderBuilder provider, string[]? traceSources)
    {
        if (traceSources == null || traceSources.Length == 0)
        {
            return;
        }

        foreach (var source in traceSources)
        {
            provider.AddSource(source);
        }
    }

    /// <summary>
    /// Configures the meter provider with the specified meters.
    /// </summary>
    public static void ConfigureMeter(MeterProviderBuilder provider, string[]? meters)
    {
        if (meters == null || meters.Length == 0)
        {
            return;
        }

        foreach (var meter in meters)
        {
            provider.AddMeter(meter);
        }
    }
}
