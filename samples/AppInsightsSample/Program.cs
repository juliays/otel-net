using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppInsightsSample
{
    public class Program
    {
        private static readonly string ServiceName = "AppInsightsSample";
        
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
                })
                .AddTelemetry()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application starting up");
            
            try
            {
                var meterProvider = host.Services.GetRequiredService<MeterProvider>();
                var meter = meterProvider.GetMeter(ServiceName);
                var startupCounter = meter.CreateCounter<long>("application.startup");
                startupCounter.Add(1, new KeyValuePair<string, object>("success", true));
                
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
        private readonly TracerProvider _tracerProvider;
        private readonly MeterProvider _meterProvider;
        private readonly Tracer _tracer;
        private readonly OpenTelemetry.Metrics.Meter _meter;
        
        public SampleService(ILogger<SampleService> logger, TracerProvider tracerProvider, MeterProvider meterProvider)
        {
            _logger = logger;
            _tracerProvider = tracerProvider;
            _meterProvider = meterProvider;
            _tracer = _tracerProvider.GetTracer("AppInsightsSample");
            _meter = _meterProvider.GetMeter("AppInsightsSample");
        }
        
        public async Task RunAsync()
        {
            _logger.LogInformation("Starting sample operations");
            
            var operationCounter = _meter.CreateCounter<long>("sample.operations");
            var operationDuration = _meter.CreateHistogram<double>("sample.operation.duration");
            
            var parentSpan = _tracer.StartSpan("ParentOperation", SpanKind.Internal);
            var parentContext = parentSpan.Context;
            
            using (parentSpan)
            {
                parentSpan.SetAttribute("operation.type", "parent");
                parentSpan.SetAttribute("operation.id", Guid.NewGuid().ToString());
                
                _logger.LogDebug("Parent operation started with ID: {SpanId}", parentSpan.Context.SpanId);
                
                var startTime = DateTime.UtcNow;
                
                try
                {
                    await PerformChildOperationAsync("ChildOperation1", parentContext);
                    
                    await PerformLinkedOperationAsync("ChildOperation2", parentContext);
                    
                    operationCounter.Add(1, new KeyValuePair<string, object>("result", "success"));
                    
                    _logger.LogInformation("All operations completed successfully");
                }
                catch (Exception ex)
                {
                    operationCounter.Add(1, new KeyValuePair<string, object>("result", "failure"));
                    
                    _logger.LogError(ex, "An error occurred during sample operations");
                    
                    parentSpan.SetStatus(Status.Error.WithDescription(ex.Message));
                }
                finally
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    operationDuration.Record(duration);
                    
                    parentSpan.AddEvent("Operation completed", new SpanAttributes
                    {
                        { "duration_ms", duration },
                        { "success", true }
                    });
                    
                    _logger.LogDebug("Parent operation completed in {DurationMs}ms", duration);
                }
            }
        }
        
        private async Task PerformChildOperationAsync(string operationName, SpanContext parentContext)
        {
            using var scope = parentContext.IsValid ? Baggage.Current.Activate() : default;
            
            var childSpan = _tracer.StartSpan(operationName, SpanKind.Internal);
            using (childSpan)
            {
                childSpan.SetAttribute("operation.name", operationName);
                
                _logger.LogInformation("Performing child operation: {OperationName}", operationName);
                
                await Task.Delay(500);
                
                _logger.LogDebug("Child operation completed: {OperationName}", operationName);
            }
        }
        
        private async Task PerformLinkedOperationAsync(string operationName, SpanContext parentContext)
        {
            var linkedSpan = _tracer.StartSpan(
                operationName,
                SpanKind.Internal,
                new SpanContext(parentContext.TraceId, SpanId.CreateRandom(), parentContext.TraceFlags, parentContext.IsRemote));
                
            using (linkedSpan)
            {
                linkedSpan.SetAttribute("operation.name", operationName);
                linkedSpan.SetAttribute("has_link", true);
                
                _logger.LogInformation("Performing linked operation: {OperationName}", operationName);
                
                await Task.Delay(300);
                
                linkedSpan.AddEvent("Processing step completed");
                
                _logger.LogDebug("Linked operation completed: {OperationName}", operationName);
            }
        }
    }
}
