namespace Telemetry.Configuration;

/// <summary>
/// TODO: when ready to implement, verify attributes are sufficient.
/// Represents the Datadog exporter options for telemetry.
/// </summary>
public class DatadogExporterOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Datadog exporter is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the endpoint for the Datadog exporter.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for the Datadog exporter.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
