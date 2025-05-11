using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionSample
{
    public class TimerFunction
    {
        private readonly ILogger _logger;
        
        public TimerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerFunction>();
        }
        
        [Function("ProcessTimer")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
        {
            _logger.LogInformation("Timer trigger function executed at: {Time}", DateTime.UtcNow);
            
            var activitySource = new ActivitySource("FunctionSample.TimerTriggers");
            
            using var activity = activitySource.StartActivity("TimerTriggeredOperation", ActivityKind.Server);
            
            if (activity != null)
            {
                activity.SetTag("function.name", "ProcessTimer");
                activity.SetTag("execution.time", DateTime.UtcNow.ToString("o"));
                activity.SetTag("timer.schedule", "0 */5 * * * *");
                activity.SetTag("timer.last.executed", timerInfo.ScheduleStatus?.Last.ToString() ?? "unknown");
                
                try
                {
                    _logger.LogInformation("Performing timer-triggered operation");
                    
                    using var childActivity = activitySource.StartActivity("TimerSubOperation", ActivityKind.Internal);
                    if (childActivity != null)
                    {
                        childActivity.SetTag("operation.name", "SubOperation");
                        
                        await Task.Delay(100); // Simulate work
                        
                        childActivity.AddEvent(new ActivityEvent("SubOperation completed"));
                    }
                    
                    await Task.Delay(200); // Simulate more work
                    
                    _logger.LogInformation("Timer-triggered operation completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in timer-triggered operation");
                    
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                }
                finally
                {
                    activity.AddEvent(new ActivityEvent("Timer function completed"));
                }
            }
            else
            {
                _logger.LogWarning("Activity could not be created for timer trigger");
                await PerformOperationWithoutActivity();
            }
        }
        
        private async Task PerformOperationWithoutActivity()
        {
            _logger.LogInformation("Performing operation without activity tracking");
            await Task.Delay(300);
            _logger.LogInformation("Operation completed without activity tracking");
        }
    }
    
    public class TimerInfo
    {
        public TimerScheduleStatus? ScheduleStatus { get; set; }
        public bool IsPastDue { get; set; }
    }
    
    public class TimerScheduleStatus
    {
        public DateTime Last { get; set; }
        public DateTime Next { get; set; }
    }
}
