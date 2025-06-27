namespace Telemetry.Constants;

/// <summary>
/// Constant string values for configuration fields.
/// </summary>
public static class ConfigConstants
{
    /// <summary>
    /// Telemetry configuration file name.
    /// </summary>
    public const string TelemetryConfigFileName = "telemetry.json";

    /// <summary>
    /// Serilog Console Sink name.
    /// </summary>
    public const string SerilogConsoleSinkName = "Console";

    /// <summary>
    /// Serilog AppInsights Sink name.
    /// </summary>
    public const string SerilogAppInsightsSinkName = "ApplicationInsights";

    /// <summary>
    /// Serilog Using Console Sink value.
    /// </summary>
    public const string SerilogUsingConsoleSink = "Serilog.Sinks.Console";

    /// <summary>
    /// Serilog Console Sink formatter value.
    /// </summary>
    public const string SerilogConsoleSinkFormatter = "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact";

    /// <summary>
    /// Serilog Using AppInsights Sink value.
    /// </summary>
    public const string SerilogUsingAppInsightsSink = "Serilog.Sinks.ApplicationInsights";

    /// <summary>
    /// Serilog AppInsights Sink telemetry converter value.
    /// </summary>
    public const string SerilogAppInsightsSinkTelemetryConverter = "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights";

    /// <summary>
    /// Resource component name, used for telemetry.
    /// </summary>
    public const string ResourceComponent = "RESOURCE_COMPONENT";

    /// <summary>
    /// Resource version, used for telemetry.
    /// </summary>
    public const string ResourceVersion = "RESOURCE_VERSION";

    /// <summary>
    /// Resource environment, used for telemetry.
    /// </summary>
    public const string ResourceEnvironment = "RESOURCE_ENVIRONMENT";

    /// <summary>
    /// Resource website name, used for telemetry. Name matches the App Service ENV variable.
    /// </summary>
    public const string ResourceWebsiteName = "WEBSITE_SITE_NAME";

    /// <summary>
    /// Resource website instance, used for telemetry. Name matches the App Service ENV variable.
    /// </summary>
    public const string ResourceWebsiteInstance = "WEBSITE_INSTANCE_ID";

    /// <summary>
    /// Resource region, used for telemetry. Name matches the App Service ENV variable.
    /// </summary>
    public const string ResourceRegion = "REGION_NAME";

    /// <summary>
    /// Enable console exporter setting.
    /// </summary>
    public const string EnableConsoleExporter = "ENABLE_CONSOLE_EXPORTER";

    /// <summary>
    /// Enable Application Insights exporter setting.
    /// </summary>
    public const string EnableAppInsightsExporter = "ENABLE_APPLICATION_INSIGHTS_EXPORTER";

    /// <summary>
    /// Application Insights connection string.
    /// </summary>
    public const string AppInsightsConnectionString = "APPLICATION_INSIGHTS_CONNECTION_STRING";

    /// <summary>
    /// Log level setting for telemetry.
    /// </summary>
    public const string LogLevel = "LOG_LEVEL";
}
