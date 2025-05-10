using System.Collections.Generic;

namespace OpenTelemetryExtensions.Configuration
{
    public class SerilogConfig
    {
        public MinimumLevelConfig MinimumLevel { get; set; } = new();
        
        public List<SinkConfig> WriteTo { get; set; } = new();
        
        public List<string> Enrich { get; set; } = new();
    }
    
    public class MinimumLevelConfig
    {
        public string Default { get; set; } = "Information";
        
        public Dictionary<string, string> Override { get; set; } = new();
    }
    
    public class SinkConfig
    {
        public string Name { get; set; } = string.Empty;
        
        public Dictionary<string, string> Args { get; set; } = new();
    }
}
