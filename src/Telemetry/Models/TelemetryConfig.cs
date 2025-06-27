using Microsoft.Extensions.Logging;

namespace Telemetry.Models;

/// <summary>
/// Record to encapsulate configuration for LMP Telemetry.
/// </summary>
public record TelemetryConfig
{
    /// <summary>
    /// Gets the resource component.
    /// </summary>
    public required string ResourceComponent { get; init; }

    /// <summary>
    /// Gets the resource version.
    /// </summary>
    public required string ResourceVersion { get; init; }

    /// <summary>
    /// Gets the resource website name.
    /// </summary>
    public string ResourceWebsiteName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resource website instance.
    /// </summary>
    public string ResourceWebsiteInstance { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resource environment.
    /// </summary>
    public string ResourceEnvironment { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resource region.
    /// </summary>
    public string ResourceRegion { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether to enable the console exporter. Default is false.
    /// </summary>
    public bool EnableConsoleExporter { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable the AppInsights exporter. Default is false.
    /// </summary>
    public bool EnableAppInsightsExporter { get; init; }

    /// <summary>
    /// Gets the AppInsights connection string.
    /// </summary>
    public string? AppInsightsConnectionString { get; init; }

    /// <summary>
    /// Gets the log level for telemetry. Default is Information.
    /// </summary>
    public LogLevel LogLevel { get; init; } = LogLevel.Information;
}
