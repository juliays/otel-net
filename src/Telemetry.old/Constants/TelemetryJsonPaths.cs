namespace Telemetry.Constants;

/// <summary>
/// Constant string values for telemetry JSON paths.
/// </summary>
public static class TelemetryJsonPaths
{
    /// <summary>
    /// Resource configuration section path.
    /// </summary>
    public const string ResourceSection = "telemetry:resource";

    /// <summary>
    /// Exporters configuration section path.
    /// </summary>
    public const string ExportersSection = "telemetry:exporters";

    /// <summary>
    /// Tracer configuration section path.
    /// </summary>
    public const string TracerSection = "telemetry:tracer";

    /// <summary>
    /// Resource component JSON path.
    /// </summary>
    public const string ResourceComponent = $"{ResourceSection}:component";

    /// <summary>
    /// Resource version JSON path.
    /// </summary>
    public const string ResourceVersion = $"{ResourceSection}:version";

    /// <summary>
    /// Resource website name JSON path.
    /// </summary>
    public const string ResourceWebsiteName = $"{ResourceSection}:websiteName";

    /// <summary>
    /// Resource website instance JSON path.
    /// </summary>
    public const string ResourceWebsiteInstance = $"{ResourceSection}:websiteInstance";

    /// <summary>
    /// Resource region JSON path.
    /// </summary>
    public const string ResourceRegion = $"{ResourceSection}:region";

    /// <summary>
    /// Resource environment JSON path.
    /// </summary>
    public const string ResourceEnvironment = $"{ResourceSection}:environment";

    /// <summary>
    /// Enable Console exporter JSON path.
    /// </summary>
    public const string EnableConsoleExporter = $"{ExportersSection}:console:enabled";

    /// <summary>
    /// Enable AppInsights exporter JSON path.
    /// </summary>
    public const string EnableAppInsightsExporter = $"{ExportersSection}:appInsights:enabled";

    /// <summary>
    /// AppInsights connection string JSON path.
    /// </summary>
    public const string AppInsightsConnectionString = $"{ExportersSection}:appInsights:connectionString";

    /// <summary>
    /// Serilog configuration section path.
    /// </summary>
    public const string SerilogSection = "Serilog";

    /// <summary>
    /// Serilog MinimumLevel Default configuration JSON path.
    /// </summary>
    public const string SerilogMinimumLevelDefault = $"{SerilogSection}:MinimumLevel:Default";

    /// <summary>
    /// Serilog Using JSON path.
    /// </summary>
    public const string SerilogUsing = $"{SerilogSection}:Using";

    /// <summary>
    /// Serilog WriteTo JSON path.
    /// </summary>
    public const string SerilogWriteTo = $"{SerilogSection}:WriteTo";

    /// <summary>
    /// Serilog Sink name JSON path.
    /// </summary>
    public const string SerilogWriteToName = "Name";

    /// <summary>
    /// Serilog Sink Args formatter JSON path.
    /// </summary>
    public const string SerilogWriteToArgsFormatter = "Args:formatter";

    /// <summary>
    /// Serilog Sink Args connection string JSON path.
    /// </summary>
    public const string SerilogWriteToArgsConnectionString = "Args:connectionString";

    /// <summary>
    /// Serilog Sink Args telemetry converter JSON path.
    /// </summary>
    public const string SerilogWriteToArgsTelemetryConverter = "Args:telemetryConverter";
}
