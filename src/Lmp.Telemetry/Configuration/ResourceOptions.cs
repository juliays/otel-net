using System;

namespace Lmp.Telemetry.Configuration
{
    public class ResourceOptions
    {
        public string Component { get; set; } = "DefaultComponent";

        public string ServiceName { get; set; } = "DefaultService";

        public string ServiceVersion { get; set; } = "1.0.0";

        public string ServiceInstanceId { get; set; } = Guid.NewGuid().ToString();

        public string Environment { get; set; } = "Development";
    }
}
