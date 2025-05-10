using System.Text.Json.Serialization;

namespace OpenTelemetryExtensions.Configuration
{
    public class ResourceConfig
    {
        public string Environment { get; set; } = "development";
        
        public string Component { get; set; } = "web-api";
        
        public string WorkspaceId { get; set; } = string.Empty;
        
        public string NotebookId { get; set; } = string.Empty;
        
        public string LivyId { get; set; } = string.Empty;
        
        public string Region { get; set; } = string.Empty;
        
        public string WebsiteName { get; set; } = string.Empty;
        
        public string WebsiteInstance { get; set; } = string.Empty;
        
        [JsonPropertyName("mnd-applicationid")]
        public string ApplicationId { get; set; } = string.Empty;
        
        [JsonPropertyName("cloud_provider")]
        public string CloudProvider { get; set; } = string.Empty;
        
        [JsonPropertyName("opt-dora")]
        public string OptDora { get; set; } = string.Empty;
        
        [JsonPropertyName("opt-service-id")]
        public string OptServiceId { get; set; } = string.Empty;
    }
}
