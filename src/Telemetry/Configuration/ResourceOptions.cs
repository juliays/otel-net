using System.ComponentModel;

namespace Telemetry.Configuration;

/// <summary>
/// Represents the resource-specific options for telemetry.
/// </summary>
public class ResourceOptions
{
    /// <summary>
    /// Gets or sets the component version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the component name.
    /// </summary>
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host type name.
    /// </summary>
    public string HostType { get; set; } = "WebApp";

    /// <summary>
    /// Gets or sets the region.
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the website name.
    /// </summary>
    public string WebsiteName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the website instance.
    /// </summary>
    public string WebsiteInstance { get; set; } = string.Empty;
}
