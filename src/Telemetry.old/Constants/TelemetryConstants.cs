using System.Text;

namespace Telemetry.Constants
{
    /// <summary>
    /// Telemetry constants for the LMP Telemetry library.
    /// </summary>
    public static class TelemetryConstants
    {
        /// <summary>
        /// Constant for ServiceName key.
        /// </summary>
        public const string ServiceName = "service.name";

        /// <summary>
        /// Constant for ServiceVersion key.
        /// </summary>
        public const string ServiceVersion = "service.version";

        /// <summary>
        /// Constant for HostType.
        /// </summary>
        public const string WebApp = "WebApp";

        /// <summary>
        /// Constant for Component tag.
        /// </summary>
        public const string ComponentTag = "component";

        /// <summary>
        /// Constant for Version tag.
        /// </summary>
        public const string VersionTag = "version";

        /// <summary>
        /// Constant for Region tag.
        /// </summary>
        public const string RegionTag = "region";

        /// <summary>
        /// Constant for WebsiteName tag.
        /// </summary>
        public const string WebsiteNameTag = "websiteName";

        /// <summary>
        /// Constant for WebsiteInstance tag.
        /// </summary>
        public const string WebsiteInstanceTag = "websiteInstance";

        /// <summary>
        /// Constant for Environment tag.
        /// </summary>
        public const string EnvironmentTag = "environment";

        /// <summary>
        /// Constant for default unit for counters.
        /// </summary>
        public const string DefaultCounterUnit = "1";

        /// <summary>
        /// Constant for default unit for histogram.
        /// </summary>
        public const string DefaultHistogramUnit = "ms";

        /// <summary>
        /// Constant for default description for histogram.
        /// </summary>
        public const string DefaultHistogramDuration = "duration in ms";

        /// <summary>
        /// Constant for default description for counter.
        /// </summary>
        public const string DefaultCounterDescription = "counter";

        /// <summary>
        /// Constant for default meter error.
        /// </summary>
        public const string MeterNotInitializedError = "Meter is not initialized.";

        /// <summary>
        /// Constant for default activity source error.
        /// </summary>
        public const string TracerNotInitializedError = "Activity source is not initialized.";

        /// <summary>
        /// Constant for telemetry exporter configuration error.
        /// </summary>
        public const string NoTelemetryExportersConfiguredError = "No telemetry exporters configured. At least one exporter must be enabled.";

        /// <summary>
        /// Constant for telemetry configuration file not found error.
        /// </summary>
        public const string TelemetryJson = "Telemetry.telemetry.json";

        /// <summary>
        /// Constant for telemetry configuration file not found error.
        /// </summary>
        public static readonly CompositeFormat TelemetryJsonNotFoundErrorTemplate = CompositeFormat.Parse("Resource {0} not found. Available resource {1}.");

        /// <summary>
        /// Constant for activity tag.
        /// </summary>
        public static readonly CompositeFormat LinkedActivityKeyTemplate = CompositeFormat.Parse("linkedActivity_{0}");

        /// <summary>
        /// Telemetry message template for when processing a job message results in an error.
        /// </summary>
        public static readonly CompositeFormat TraceParentFormatErrorTemplate = CompositeFormat.Parse("(ERROR): Invalid linked activity context: {0}. It needs to be in w3c traceparent format");
    }
}
