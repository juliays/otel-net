using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Lmp.Telemetry.Interfaces;

namespace Lmp.Telemetry
{
    public class CounterWrapper<T> : ICounter<T> where T : struct
    {
        private readonly Counter<T> _counter;

        public string Name => _counter.Name;

        public string Description => _counter.Description;

        public string Unit => _counter.Unit;

        public CounterWrapper(Counter<T> counter)
        {
            _counter = counter ?? throw new ArgumentNullException(nameof(counter));
        }

        public void Add(T value, params KeyValuePair<string, object>[] tags)
        {
            _counter.Add(value, tags);
        }

        public void Add(T value, ReadOnlySpan<KeyValuePair<string, object>> tags)
        {
            _counter.Add(value, tags);
        }
    }
}
