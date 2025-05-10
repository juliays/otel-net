using System.Text.Json.Serialization;

namespace OpenTelemetryExtensions.Configuration
{
    public class TelemetryConfig
    {
        public const string SectionName = "telemetry";
        
        /// Resource attributes configuration
        public ResourceConfig Resource { get; set; } = new();
        
        /// Serilog configuration
        public SerilogConfig Serilog { get; set; } = new();
        
        /// Exporters configuration
        public ExporterConfig Exporters { get; set; } = new();
        
        /// Tracer configuration
        public TracerConfig Tracer { get; set; } = new();
    }
}
