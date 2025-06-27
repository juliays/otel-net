namespace Telemetry;

/// <summary>
/// Factory interface for creating and caching IInstrumentation instances.
/// </summary>
public interface IInstrumentationFactory
{
    /// <summary>
    /// Gets or creates a cached Instrumentation instance for the specified service name and version.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="serviceVersion">The version of the service.</param>
    /// <returns>A cached or new Instrumentation instance.</returns>
    public IInstrumentation Create(string serviceName, string serviceVersion);
}
