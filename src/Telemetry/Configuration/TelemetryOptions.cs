namespace Telemetry.Configuration;

/// <summary>
/// Represents the configuration options for telemetry.
/// This only covers configuration for metrics and traces.
/// Logs related configuration is handled by Serilog section and is read by Serilog Library.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Gets or sets the resource options for telemetry.
    /// </summary>
    public ResourceOptions Resource { get; set; } = new();

    /// <summary>
    /// Gets or sets the exporter options for telemetry.
    /// </summary>
    public ExportersOptions Exporters { get; set; } = new();

    /// <summary>
    /// Gets or sets the tracer options for telemetry.
    /// </summary>
    public TracerOptions Tracer { get; set; } = new();
}
