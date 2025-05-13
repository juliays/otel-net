using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Lmp.Telemetry.Interfaces;

namespace Lmp.Telemetry
{
    public class HistogramWrapper<T> : IHistogram<T> where T : struct
    {
        private readonly Histogram<T> _histogram;

        public string Name => _histogram.Name;

        public string Description => _histogram.Description;

        public string Unit => _histogram.Unit;

        public HistogramWrapper(Histogram<T> histogram)
        {
            _histogram = histogram ?? throw new ArgumentNullException(nameof(histogram));
        }

        public void Record(T value, params KeyValuePair<string, object>[] tags)
        {
            _histogram.Record(value, tags);
        }

        public void Record(T value, ReadOnlySpan<KeyValuePair<string, object>> tags)
        {
            _histogram.Record(value, tags);
        }
    }
}
