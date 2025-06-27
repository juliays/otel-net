using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using Telemetry.Constants;
using Microsoft.Extensions.Logging;

namespace Telemetry;

/// <summary>
/// Provides functionality for tracing and metrics instrumentation.
/// </summary>
public class Instrumentation : IInstrumentation
{
    /// <summary>
    /// The activity source used for tracing.
    /// </summary>
    private readonly ActivitySource? _activitySource;

    /// <summary>
    /// The meter used for metrics.
    /// </summary>
    private readonly Meter? _meter;

    /// <summary>
    /// The counter used for metrics.
    /// </summary>
    private readonly Dictionary<string, Counter<long>> _counters = [];

    /// <summary>
    /// The histogram used for metrics.
    /// </summary>
    private readonly Dictionary<string, Histogram<long>> _histograms = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Instrumentation"/> class with a new activity source and meter.
    /// </summary>
    public Instrumentation(string component, string version)
    {
        _activitySource = new ActivitySource(component, version);
        _meter = new Meter(component, version);
    }

    /// <inheritdoc/>
    public ActivitySource GetTracer()
    {
        return _activitySource ?? throw new InvalidOperationException(TelemetryConstants.TracerNotInitializedError);
    }

    /// <inheritdoc/>
    public Meter GetMeter()
    {
        return _meter ?? throw new InvalidOperationException(TelemetryConstants.MeterNotInitializedError);
    }

    /// <inheritdoc/>
    public Activity? StartSpan(string name, ActivityKind kind, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<string>? linkedActivity = null)
    {
        if (linkedActivity == null && tags == null)
        {
            return _activitySource?.StartActivity(name, kind);
        }
        else if (linkedActivity != null)
        {
            var links = new List<ActivityLink>();

            // TODO: remove the section to add links to tags when converted to Datadog exporter
            var extraTags = new Dictionary<string, object>();
            var i = 0;
            foreach (var value in linkedActivity)
            {
                var key = string.Format(CultureInfo.InvariantCulture, TelemetryConstants.LinkedActivityKeyTemplate, i);
                extraTags.Add(key, value);
                if (ActivityContext.TryParse(value, null, out var context))
                {
                    links.Add(new ActivityLink(context));
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, TelemetryConstants.TraceParentFormatErrorTemplate, value));
                }
                i++;
            }
            if (tags != null)
            {
                foreach (var kvp in tags)
                {
                    extraTags.Add(kvp.Key, kvp.Value ?? string.Empty);
                }
            }
            var currentContext = Activity.Current?.Context ?? default;
            var tagPairs = extraTags.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value));
            return _activitySource?.StartActivity(name, kind, currentContext, tagPairs, links);
        }
        else
        {
            return _activitySource?.StartActivity(name, kind, Activity.Current?.Context ?? default, tags);
        }
    }

    /// <inheritdoc/>
    public Counter<long> GetCounter(string name, IEnumerable<KeyValuePair<string, object?>>? tags = null, string unit = TelemetryConstants.DefaultCounterUnit, string description = "counter")
    {
        if (_meter == null)
        {
            throw new InvalidOperationException(TelemetryConstants.MeterNotInitializedError);
        }
        if (!_counters.TryGetValue(name, out var counter) || counter == null)
        {
            counter = (tags == null || !tags.Any())
                ? _meter.CreateCounter<long>(name, unit, description)
                : _meter.CreateCounter<long>(name, unit, description, tags.ToArray());
            _counters[name] = counter;
        }
        return counter;
    }

    /// <inheritdoc/>
    public Histogram<long> GetHistogram(string name, IEnumerable<KeyValuePair<string, object?>>? tags = null, string unit = TelemetryConstants.DefaultHistogramUnit, string description = "histogram")
    {
        if (_meter == null)
        {
            throw new InvalidOperationException(TelemetryConstants.MeterNotInitializedError);
        }
        if (!_histograms.TryGetValue(name, out var histogram) || histogram == null)
        {
            histogram = (tags == null || !tags.Any())
                ? _meter.CreateHistogram<long>(name, unit, description)
                : _meter.CreateHistogram<long>(name, unit, description, tags.ToArray());
            _histograms[name] = histogram;
        }
        return histogram;
    }

    /// <inheritdoc/>
    public void IncreaseCounter(string name, IEnumerable<KeyValuePair<string, object?>>? tags = null, long value = 1)
    {
        if (!_counters.TryGetValue(name, out var counter) || counter == null)
        {
            counter = GetCounter(name, null, TelemetryConstants.DefaultCounterUnit, TelemetryConstants.DefaultCounterDescription);
        }
        if (tags == null || !tags.Any())
        {
            counter.Add(value);
        }
        else
        {
            counter.Add(value, tags.ToArray());
        }
    }

    /// <inheritdoc/>
    public void RecordDuration(string name, long value, IEnumerable<KeyValuePair<string, object?>>? tags = null)
    {
        if (!_histograms.TryGetValue(name, out var histogram) || histogram == null)
        {
            histogram = GetHistogram(name, null, TelemetryConstants.DefaultHistogramUnit, TelemetryConstants.DefaultHistogramDuration);
        }
        if (tags == null || !tags.Any())
        {
            histogram.Record(value);
        }
        else
        {
            histogram.Record(value, tags.ToArray());
        }
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="Instrumentation"/> class.
    /// </summary>
    public void Dispose()
    {
        _counters.Clear();
        _histograms.Clear();
        _activitySource?.Dispose();
        _meter?.Dispose();
        GC.SuppressFinalize(this);
    }
}
