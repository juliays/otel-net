namespace Telemetry.Configuration;

/// <summary>
/// Represents the tracer-specific options for telemetry.
/// </summary>
public class TracerOptions
{
    /// <summary>
    /// Gets or sets the sampling rate for the tracer.
    /// </summary>
    public double SampleRate { get; set; } = 1.0;
}
