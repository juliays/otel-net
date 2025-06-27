namespace Telemetry.Configuration;

/// <summary>
/// Represents the exporter-specific options for telemetry.
/// </summary>
public class ExportersOptions
{
    /// <summary>
    /// Gets or sets the Application Insights exporter options.
    /// </summary>
    public AppInsightsExporterOptions? AppInsights { get; set; }

    /// <summary>
    /// Gets or sets the Datadog exporter options.
    /// </summary>
    public DatadogExporterOptions? Datadog { get; set; }

    /// <summary>
    /// Gets or sets the OpenTelemetry Console exporter options.
    /// </summary>
    public ConsoleExporterOptions? Console { get; set; }
}
