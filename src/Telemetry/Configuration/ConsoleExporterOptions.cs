namespace Telemetry.Configuration;

/// <summary>
/// Represents the options for the console exporter. if not configured,
/// it's default to false.
/// </summary>
public class ConsoleExporterOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the console exporter is enabled.
    /// </summary>
    public bool Enabled { get; set; }
}
