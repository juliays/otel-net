using System;
using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace OpenTelemetryExtensions.Enrichers
{
    public class TraceContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            
            if (activity != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "TraceId", activity.TraceId.ToString()));
                
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "SpanId", activity.SpanId.ToString()));
                
                if (activity.ParentSpanId != default)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "ParentSpanId", activity.ParentSpanId.ToString()));
                }
                
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "TraceFlags", activity.ActivityTraceFlags.ToString()));
                
                var traceParent = $"00-{activity.TraceId}-{activity.SpanId}-{(activity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00")}";
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "W3CTraceParent", traceParent));
                
                foreach (var item in activity.Baggage)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        $"Baggage_{item.Key}", item.Value));
                }
                
                foreach (var tag in activity.Tags)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        $"Tag_{tag.Key}", tag.Value));
                }
            }
        }
    }
}
