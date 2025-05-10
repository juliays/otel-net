namespace OpenTelemetryExtensions.Configuration
{
    public class ExporterConfig
    {
        public ConsoleExporterConfig Console { get; set; } = new();
        
        public AppInsightsExporterConfig AppInsights { get; set; } = new();
        
        public DatadogExporterConfig Datadog { get; set; } = new();
    }
    
    public class ConsoleExporterConfig
    {
        public bool Enabled { get; set; } = true;
        
        public bool IncludeScopes { get; set; } = true;
    }
    
    public class AppInsightsExporterConfig
    {
        public bool Enabled { get; set; } = false;
        
        public string ConnectionString { get; set; } = string.Empty;
    }
    
    public class DatadogExporterConfig
    {
        public bool Enabled { get; set; } = false;
        
        public string Endpoint { get; set; } = "https://api.datadoghq.com";
        
        public string ApiKey { get; set; } = string.Empty;
    }
}
