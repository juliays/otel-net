using System;

namespace Lmp.Telemetry.Interfaces
{
    public interface ICounter<T> where T : struct
    {
        string Name { get; }

        string Description { get; }

        string Unit { get; }

        void Add(T value, params KeyValuePair<string, object>[] tags);

        void Add(T value, ReadOnlySpan<KeyValuePair<string, object>> tags);
    }
}
