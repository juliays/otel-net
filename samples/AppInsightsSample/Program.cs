using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace AppInsightsSample
{
    public class Program
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("AppInsightsSample");
        private static readonly Meter _meter = new Meter("AppInsightsSample", "1.0.0");
        
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<SampleService>();
                    
                    services.AddSingleton(_activitySource);
                    services.AddSingleton(_meter);
                })
                .AddTelemetry()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application starting up");
            
            try
            {
                var startupCounter = _meter.CreateCounter<int>("application.startup");
                startupCounter.Add(1, new KeyValuePair<string, object?>("success", true));
                
                var sampleService = host.Services.GetRequiredService<SampleService>();
                await sampleService.RunAsync();
                
                logger.LogInformation("Application shutting down normally");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during application execution");
                throw;
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public class SampleService
    {
        private readonly ILogger<SampleService> _logger;
        private readonly ActivitySource _activitySource;
        private readonly Meter _meter;
        
        public SampleService(ILogger<SampleService> logger, ActivitySource activitySource, Meter meter)
        {
            _logger = logger;
            _activitySource = activitySource;
            _meter = meter;
        }
        
        public async Task RunAsync()
        {
            _logger.LogInformation("Starting sample operations");
            
            var operationCounter = _meter.CreateCounter<int>("sample.operations");
            var operationDuration = _meter.CreateHistogram<double>("sample.operation.duration");
            
            using var parentActivity = _activitySource.StartActivity("ParentOperation", ActivityKind.Internal);
            parentActivity?.SetTag("operation.type", "parent");
            parentActivity?.SetTag("operation.id", Guid.NewGuid().ToString());
            
            _logger.LogDebug("Parent operation started with ID: {ActivityId}", parentActivity?.Id);
            
            var startTime = DateTime.UtcNow;
            
            try
            {
                await PerformChildOperationAsync("ChildOperation1");
                
                await PerformLinkedOperationAsync("ChildOperation2", parentActivity);
                
                operationCounter.Add(1, new KeyValuePair<string, object?>("result", "success"));
                
                _logger.LogInformation("All operations completed successfully");
            }
            catch (Exception ex)
            {
                operationCounter.Add(1, new KeyValuePair<string, object?>("result", "failure"));
                
                _logger.LogError(ex, "An error occurred during sample operations");
                
                parentActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            }
            finally
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                operationDuration.Record(duration);
                
                parentActivity?.AddEvent(new ActivityEvent("Operation completed", 
                    DateTime.UtcNow, 
                    new ActivityTagsCollection { 
                        { "duration_ms", duration },
                        { "success", true }
                    }));
                
                _logger.LogDebug("Parent operation completed in {DurationMs}ms", duration);
            }
        }
        
        private async Task PerformChildOperationAsync(string operationName)
        {
            using var childActivity = _activitySource.StartActivity(
                operationName, 
                ActivityKind.Internal, 
                parentContext: Activity.Current?.Context);
                
            childActivity?.SetTag("operation.name", operationName);
            
            _logger.LogInformation("Performing child operation: {OperationName}", operationName);
            
            await Task.Delay(500);
            
            _logger.LogDebug("Child operation completed: {OperationName}", operationName);
        }
        
        private async Task PerformLinkedOperationAsync(string operationName, Activity? parentActivity)
        {
            var links = new List<ActivityLink>();
            if (parentActivity != null)
            {
                links.Add(new ActivityLink(parentActivity.Context));
            }
            
            using var linkedActivity = _activitySource.StartActivity(
                operationName,
                ActivityKind.Internal,
                parentContext: Activity.Current?.Context,
                links: links);
                
            linkedActivity?.SetTag("operation.name", operationName);
            linkedActivity?.SetTag("has_link", "true");
            
            _logger.LogInformation("Performing linked operation: {OperationName}", operationName);
            
            await Task.Delay(300);
            
            linkedActivity?.AddEvent(new ActivityEvent("Processing step completed"));
            
            _logger.LogDebug("Linked operation completed: {OperationName}", operationName);
        }
    }
}
