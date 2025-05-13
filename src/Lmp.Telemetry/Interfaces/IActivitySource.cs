using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lmp.Telemetry.Interfaces
{
    public interface IActivitySource : IDisposable
    {
        Activity? StartActivity(string name, ActivityKind kind);

        Activity? StartActivity(
            string name,
            ActivityKind kind,
            ActivityContext parentContext,
            IEnumerable<KeyValuePair<string, object?>>? tags = null,
            IEnumerable<ActivityLink>? links = null,
            DateTimeOffset startTime = default);
    }
}
