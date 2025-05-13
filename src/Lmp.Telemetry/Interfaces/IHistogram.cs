using System;

namespace Lmp.Telemetry.Interfaces
{
    public interface IHistogram<T> where T : struct
    {
        string Name { get; }

        string Description { get; }

        string Unit { get; }

        void Record(T value, params KeyValuePair<string, object>[] tags);

        void Record(T value, ReadOnlySpan<KeyValuePair<string, object>> tags);
    }
}
