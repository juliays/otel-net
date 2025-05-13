using System;
using System.Diagnostics.Metrics;

namespace Lmp.Telemetry.Interfaces
{
    public interface IMeter : IDisposable
    {
        string Name { get; }

        string Version { get; }
        
        Counter<T> CreateCounter<T>(string name, string unit = null, string description = null) where T : struct;

        Histogram<T> CreateHistogram<T>(string name, string unit = null, string description = null) where T : struct;

        ObservableCounter<T> CreateObservableCounter<T>(string name, Func<T> observeValue, string unit = null, string description = null) where T : struct;

        ObservableGauge<T> CreateObservableGauge<T>(string name, Func<T> observeValue, string unit = null, string description = null) where T : struct;
    }
}
