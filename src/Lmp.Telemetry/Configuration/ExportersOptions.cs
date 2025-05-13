namespace Lmp.Telemetry.Configuration
{
    public class ExportersOptions
    {
        public ConsoleExporterOptions? Console { get; set; }

        public AppInsightsExporterOptions? AppInsights { get; set; }

        public DatadogExporterOptions? Datadog { get; set; }
    }

    public class BaseExporterOptions
    {
        public bool Enabled { get; set; }
    }

    public class ConsoleExporterOptions : BaseExporterOptions
    {
    }

    public class AppInsightsExporterOptions : BaseExporterOptions
    {
        public string? ConnectionString { get; set; }
    }

    public class DatadogExporterOptions : BaseExporterOptions
    {
        public string? ApiKey { get; set; }

        public string? Endpoint { get; set; }
    }
}
