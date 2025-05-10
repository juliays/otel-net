using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace OpenTelemetryExtensions.Enrichers
{
    public class TraceContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            
            if (activity != null)
            {
                if (!string.IsNullOrEmpty(activity.TraceId.ToString()))
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "TraceId", activity.TraceId.ToString()));
                }
                
                if (!string.IsNullOrEmpty(activity.SpanId.ToString()))
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "SpanId", activity.SpanId.ToString()));
                }
                
                if (!string.IsNullOrEmpty(activity.ParentSpanId.ToString()))
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "ParentSpanId", activity.ParentSpanId.ToString()));
                }
                
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "TraceFlags", activity.ActivityTraceFlags.ToString()));
                
                var traceParent = activity.Id;
                if (!string.IsNullOrEmpty(traceParent))
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "TraceParent", traceParent));
                }
                
                var traceState = activity.TraceStateString;
                if (!string.IsNullOrEmpty(traceState))
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "TraceState", traceState));
                }
            }
        }
    }
}
