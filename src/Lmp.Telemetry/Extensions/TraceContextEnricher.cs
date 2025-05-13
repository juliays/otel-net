using System.Diagnostics; // For Activity
using Serilog.Core; // For ILogEventEnricher
using Serilog.Events; // For LogEvent

namespace Lmp.Telemetry.Extensions;

public class TraceContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;

        if (activity != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ParentSpanId", activity.ParentSpanId.ToString()));

            var traceParent = $"00-{activity.TraceId}-{activity.SpanId}-01";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceParent", traceParent));
        }
    }
}
