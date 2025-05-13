using System;

namespace Lmp.Telemetry.Configuration
{
    public class TelemetryOptions
    {
        public ResourceOptions Resource { get; set; } = new ResourceOptions();

        public ExportersOptions Exporters { get; set; } = new ExportersOptions();

        public TracerOptions Tracer { get; set; } = new TracerOptions();
    }
}
