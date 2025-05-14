using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lmp.Telemetry.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

var host = Host.CreateDefaultBuilder(args)
    .AddOpenTelemetry(new ConfigurationBuilder().Build())
    .ConfigureServices(services =>
    {
        services.AddHostedService<SampleService>();
    })
    .Build();

await host.RunAsync();

public class SampleService : IHostedService
{
    private readonly ILogger<SampleService> _logger;
    private readonly ActivitySource _activitySource;
    private readonly Meter _meter;
    private readonly Counter<int> _operationCounter;
    private readonly Histogram<double> _operationDuration;
    private Timer? _timer;

    public SampleService(ILogger<SampleService> logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource("ConsoleSample", "1.0.0");
        _meter = new Meter("ConsoleSample", "1.0.0");
        _operationCounter = _meter.CreateCounter<int>("operations.count", "Operations", "The number of operations performed");
        _operationDuration = _meter.CreateHistogram<double>("operations.duration", "ms", "The duration of operations");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sample service started at: {time}", DateTimeOffset.Now);
        
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Performing sample operation at: {time}", DateTimeOffset.Now);
        
        using var activity = _activitySource.StartActivity("SampleOperation", ActivityKind.Internal);
        activity?.SetTag("operation.type", "sample");
        activity?.SetTag("operation.time", DateTimeOffset.Now.ToString());
        
        var startTime = DateTime.UtcNow;
        
        try
        {
            _operationCounter.Add(1, new KeyValuePair<string, object?>("operation.type", "sample"));
            
            Thread.Sleep(100);
            
            _logger.LogInformation("Sample operation completed successfully");
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing sample operation");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _operationCounter.Add(1, new KeyValuePair<string, object?>("result", "error"));
        }
        finally
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _operationDuration.Record(duration, new KeyValuePair<string, object?>("operation.type", "sample"));
            
            activity?.AddEvent(new ActivityEvent("Operation completed", 
                DateTime.UtcNow, 
                new ActivityTagsCollection { 
                    { "duration_ms", duration }
                }));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sample service stopped at: {time}", DateTimeOffset.Now);
        
        _timer?.Change(Timeout.Infinite, 0);
        
        return Task.CompletedTask;
    }
}
