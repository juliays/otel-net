using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lmp.Telemetry.Interfaces;

namespace Lmp.Telemetry
{
    public class ActivitySourceWrapper : IActivitySource
    {
        private readonly ActivitySource _activitySource;

        public ActivitySourceWrapper(string name, string? version = null)
        {
            _activitySource = new ActivitySource(name, version);
        }

        public ActivitySourceWrapper(ActivitySource activitySource)
        {
            _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        }

        public Activity? StartActivity(string name, ActivityKind kind)
        {
            return _activitySource.StartActivity(name, kind);
        }

        public Activity? StartActivity(
            string name,
            ActivityKind kind,
            ActivityContext parentContext,
            IEnumerable<KeyValuePair<string, object?>>? tags = null,
            IEnumerable<ActivityLink>? links = null,
            DateTimeOffset startTime = default)
        {
            return _activitySource.StartActivity(name, kind, parentContext, tags, links, startTime);
        }

        public void Dispose()
        {
            _activitySource.Dispose();
        }
    }
}
