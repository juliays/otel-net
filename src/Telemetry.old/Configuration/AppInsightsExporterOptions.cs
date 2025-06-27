namespace Telemetry.Configuration;

/// <summary>
/// Represents the Application Insights exporter options for telemetry.
/// </summary>
public class AppInsightsExporterOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Application Insights exporter is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the connection string for Application Insights.
    /// </summary>
    public string? ConnectionString { get; set; } = string.Empty;
}
