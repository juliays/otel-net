using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Lmp.Telemetry.Interfaces;
using Microsoft.Extensions.Logging;

namespace Lmp.Telemetry
{
    public class Instrumentation : IDisposable
    {
        private readonly string _component;

        private readonly string _version;

        private readonly IActivitySource? _activitySource;

        private readonly IMeter? _meter;

        private readonly ILogger<Instrumentation> _logger;

        public Instrumentation(string component, string version, ILogger<Instrumentation> logger)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrEmpty(component))
            {
                throw new ArgumentException("Component name cannot be empty", nameof(component));
            }
            _version = version;
            _activitySource = new ActivitySourceWrapper(component, version);
            _meter = new MeterWrapper(component, version);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Instrumentation(string component, ILogger<Instrumentation> logger)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrEmpty(component))
            {
                throw new ArgumentException("Component name cannot be empty", nameof(component));
            }
            _version = "1.0.0"; // Default version
            _activitySource = new ActivitySourceWrapper(component);
            _meter = new MeterWrapper(component);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Instrumentation(string component, ILogger<Instrumentation> logger, IActivitySource? activitySource, IMeter? meter)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrEmpty(component))
            {
                throw new ArgumentException("Component name cannot be empty", nameof(component));
            }
            _version = "1.0.0"; // Default version
            _activitySource = activitySource ?? new ActivitySourceWrapper(component);
            _meter = meter ?? new MeterWrapper(component);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Activity? StartServerSpan(string name, IDictionary<string, object>? tags = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            return StartSpan(name, ActivityKind.Server, tags);
        }

        public Activity? StartClientSpan(string name, IDictionary<string, object>? tags = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            return StartSpan(name, ActivityKind.Client, tags);
        }

        public Activity? StartInternalSpan(string name, IDictionary<string, object>? tags = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            return StartSpan(name, ActivityKind.Internal, tags);
        }

        public Activity? StartSpan(string name, ActivityKind kind, IDictionary<string, object>? tags = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            
            if (tags == null)
            {
                return _activitySource?.StartActivity(name, kind);
            }
            return StartLinkedSpan(name, null, kind, tags);
        }

        public Activity? StartLinkedServerSpan(string name, IEnumerable<string> linkedActivity, IDictionary<string, object>? tags = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            return StartLinkedSpan(name, linkedActivity, ActivityKind.Server, tags);
        }

        public Activity? StartLinkedClientSpan(string name, IEnumerable<string> linkedActivity, IDictionary<string, object>? tags = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            return StartLinkedSpan(name, linkedActivity, ActivityKind.Client, tags);
        }

        public Activity? StartLinkedInternalSpan(string name, IEnumerable<string> linkedActivity, IDictionary<string, object>? tags = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            return StartLinkedSpan(name, linkedActivity, ActivityKind.Internal, tags);
        }

        public Activity? StartLinkedSpan(string name, IEnumerable<string>? linkedActivity, ActivityKind kind, IDictionary<string, object>? tags = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Span name cannot be empty", nameof(name));
            }
            
            var links = new List<ActivityLink>();
            if (linkedActivity != null)
            {
                foreach (var value in linkedActivity)
                {
                    if (ActivityContext.TryParse(value, null, out var context))
                    {
                        links.Add(new ActivityLink(context));
                    }
                }
            }
            var currentContext = Activity.Current?.Context ?? default;

            var activity = _activitySource?.StartActivity(
                name,
                kind,
                currentContext,
                tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)),
                links,
                default(DateTimeOffset));

            if (activity == null)
            {
                throw new InvalidOperationException("Activity could not be started.");
            }

            return activity;
        }

        public void Dispose()
        {
            _activitySource?.Dispose();
            _meter?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
