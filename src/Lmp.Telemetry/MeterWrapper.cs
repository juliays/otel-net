using System;
using System.Diagnostics.Metrics;
using Lmp.Telemetry.Interfaces;

namespace Lmp.Telemetry
{
    public class MeterWrapper : IMeter
    {
        private readonly Meter _meter;

        public string Name => _meter.Name;

        public string Version => _meter.Version;

        public MeterWrapper(string name, string? version = null)
        {
            _meter = new Meter(name, version);
        }

        public MeterWrapper(Meter meter)
        {
            _meter = meter ?? throw new ArgumentNullException(nameof(meter));
        }

        public Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null) where T : struct
        {
            return _meter.CreateCounter<T>(name, unit, description);
        }

        public Histogram<T> CreateHistogram<T>(string name, string? unit = null, string? description = null) where T : struct
        {
            return _meter.CreateHistogram<T>(name, unit, description);
        }

        public ObservableCounter<T> CreateObservableCounter<T>(string name, Func<T> observeValue, string? unit = null, string? description = null) where T : struct
        {
            return _meter.CreateObservableCounter<T>(name, observeValue, unit, description);
        }

        public ObservableGauge<T> CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit = null, string? description = null) where T : struct
        {
            return _meter.CreateObservableGauge<T>(name, observeValue, unit, description);
        }

        public void Dispose()
        {
            _meter.Dispose();
        }
    }
}
