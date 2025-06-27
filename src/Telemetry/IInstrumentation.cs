using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Telemetry.Constants;

namespace Telemetry
{
    /// <summary>
    /// Defines the contract for tracing and metrics instrumentation, including methods for starting spans and linked spans
    /// of various kinds (server, client, internal), and for retrieving tracer and meter instances. Implementations of this
    /// interface provide the ability to create and manage distributed tracing activities and metrics in a consistent way
    /// across different application components.
    /// </summary>
    public interface IInstrumentation : IDisposable
    {
        /// <summary>
        /// Gets the tracer (activity source) for distributed tracing.
        /// </summary>
        /// <returns>The tracer instance.</returns>
        public ActivitySource GetTracer();

        /// <summary>
        /// Gets the meter for metrics collection.
        /// </summary>
        /// <returns>The meter instance.</returns>
        public Meter GetMeter();

        /// <summary>
        /// Starts a span for tracing with the specified kind and tags.
        /// </summary>
        /// <param name="name">The name of the span.</param>
        /// <param name="kind">The kind of the span (e.g., Server, Client, Internal).</param>
        /// <param name="tags">Optional tags to associate with the span.</param>
        /// <param name="linkedActivity">The linked activity contexts.</param>
        /// <returns>The started activity, or null if tracing is not enabled.</returns>
        public Activity? StartSpan(string name, ActivityKind kind, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<string>? linkedActivity = null);

        /// <summary>
        /// Creates a counter metric with the specified name, description, unit, and optional tags.
        /// only need to call this method if you were to change the default unit. Otherwise, you can use `IncreaseCounter` directly.
        /// It will create a counter with the default unit of "1" and the name passed in if not already created.
        /// </summary>
        /// <param name="name">The name of the counter.</param>
        /// <param name="tags">Tags associated with the counter.</param>
        /// <param name="unit">The unit of measurement for the counter.</param>
        /// <param name="description">The description of the counter.</param>
        public Counter<long> GetCounter(string name, IEnumerable<KeyValuePair<string, object?>>? tags = null, string unit = TelemetryConstants.DefaultCounterUnit, string description = "counter");

        /// <summary>
        /// Creates a histogram metric with the specified name, description, unit, and optional tags.
        /// only need to call this method if you were to change the default unit. Otherwise, you can use `RecordDuration` directly.
        /// It will create a counter with the default unit of "ms" and the name passed in if not already created.
        /// </summary>
        /// <param name="name">The name of the histogram.</param>
        /// <param name="tags">Tags associated with the histogram.</param>
        /// <param name="unit">The unit of measurement for the histogram.</param>
        /// <param name="description">The description of the histogram.</param>
        public Histogram<long> GetHistogram(string name, IEnumerable<KeyValuePair<string, object?>>? tags = null, string unit = TelemetryConstants.DefaultHistogramUnit, string description = "histogram");

        /// <summary>
        /// Increases the value of a counter with additional tags.
        /// </summary>
        /// <param name="name">The name of the counter.</param>
        /// <param name="tags">The tags to associate with this counter increment.</param>
        /// <param name="value">The value to add to the counter.</param>
        void IncreaseCounter(string name, IEnumerable<KeyValuePair<string, object?>>? tags = null, long value = 1);

        /// <summary>
        /// Records a value in a histogram with additional tags.
        /// </summary>
        /// <param name="name">The name of the histogram.</param>
        /// <param name="value">The value to record in the histogram.</param>
        /// <param name="tags">The tags to associate with this histogram record.</param>
        void RecordDuration(string name, long value, IEnumerable<KeyValuePair<string, object?>>? tags = null);
    }
}
