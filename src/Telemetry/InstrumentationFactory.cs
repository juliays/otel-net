using System.Collections.Concurrent;

namespace Telemetry;

/// <summary>
/// Factory class for creating and caching Instrumentation instances.
/// This class ensures that only one instance of Instrumentation is created for each unique service name and version.
/// </summary>
public class InstrumentationFactory : IInstrumentationFactory
{
    private static readonly ConcurrentDictionary<(string ServiceName, string ServiceVersion), IInstrumentation> _cache = new();

    /// <inheritdoc />
    public IInstrumentation Create(string serviceName, string serviceVersion)
    {
        return _cache.GetOrAdd((serviceName, serviceVersion), key => new Instrumentation(key.ServiceName, key.ServiceVersion));
    }
}
