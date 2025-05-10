using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

var activitySource = new ActivitySource("WebApiSample");
var meter = new Meter("WebApiSample", "1.0.0");

var requestCounter = meter.CreateCounter<int>("http.requests");
var requestDuration = meter.CreateHistogram<double>("http.request.duration");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(activitySource);
builder.Services.AddSingleton(meter);

builder.AddTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/hello", (ILogger<Program> logger) => 
{
    logger.LogInformation("Processing /hello request");
    
    using var activity = activitySource.StartActivity("HelloEndpoint", ActivityKind.Server);
    activity?.SetTag("endpoint", "/hello");
    activity?.SetTag("request.time", DateTime.UtcNow);
    
    requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/hello"));
    
    var startTime = DateTime.UtcNow;
    
    try
    {
        logger.LogDebug("Generating hello response");
        
        Task.Delay(50).Wait();
        
        logger.LogInformation("Successfully processed /hello request");
        
        return "Hello from OpenTelemetry-enabled Web API!";
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing /hello request");
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        requestCounter.Add(1, new KeyValuePair<string, object?>("result", "error"));
        throw;
    }
    finally
    {
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        requestDuration.Record(duration, new KeyValuePair<string, object?>("endpoint", "/hello"));
        
        activity?.AddEvent(new ActivityEvent("Request completed", 
            DateTime.UtcNow, 
            new ActivityTagsCollection { 
                { "duration_ms", duration }
            }));
    }
});

app.MapGet("/linked-operation", async (ILogger<Program> logger) => 
{
    logger.LogInformation("Processing /linked-operation request");
    
    using var parentActivity = activitySource.StartActivity("LinkedOperationParent", ActivityKind.Server);
    parentActivity?.SetTag("endpoint", "/linked-operation");
    
    requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/linked-operation"));
    
    var startTime = DateTime.UtcNow;
    
    try
    {
        await PerformOperationAsync("FirstOperation", logger);
        
        await PerformLinkedOperationAsync("SecondOperation", parentActivity, logger);
        
        logger.LogInformation("Successfully processed /linked-operation request");
        
        return "Linked operations completed successfully!";
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing /linked-operation request");
        parentActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
    finally
    {
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        requestDuration.Record(duration, new KeyValuePair<string, object?>("endpoint", "/linked-operation"));
    }
});

app.MapGet("/metrics", (ILogger<Program> logger) => 
{
    logger.LogInformation("Processing /metrics request");
    
    var customGauge = meter.CreateObservableGauge("custom.value", () => 
    {
        return new[] { new Measurement<double>(42.0) };
    });
    
    var operationCounter = meter.CreateCounter<int>("custom.operations");
    
    operationCounter.Add(1, new KeyValuePair<string, object?>("operation", "read"));
    operationCounter.Add(2, new KeyValuePair<string, object?>("operation", "write"));
    
    logger.LogInformation("Metrics recorded successfully");
    
    return "Metrics recorded successfully!";
});

async Task PerformOperationAsync(string operationName, ILogger logger)
{
    using var childActivity = activitySource.StartActivity(
        operationName, 
        ActivityKind.Internal, 
        parentContext: Activity.Current?.Context);
        
    childActivity?.SetTag("operation.name", operationName);
    
    logger.LogInformation("Performing operation: {OperationName}", operationName);
    
    await Task.Delay(100);
    
    logger.LogDebug("Operation completed: {OperationName}", operationName);
}

async Task PerformLinkedOperationAsync(string operationName, Activity? parentActivity, ILogger logger)
{
    var links = new List<ActivityLink>();
    if (parentActivity != null)
    {
        links.Add(new ActivityLink(parentActivity.Context));
    }
    
    using var linkedActivity = activitySource.StartActivity(
        operationName,
        ActivityKind.Internal,
        parentContext: Activity.Current?.Context,
        links: links);
        
    linkedActivity?.SetTag("operation.name", operationName);
    linkedActivity?.SetTag("has_link", "true");
    
    logger.LogInformation("Performing linked operation: {OperationName}", operationName);
    
    await Task.Delay(150);
    
    linkedActivity?.AddEvent(new ActivityEvent("Processing step completed"));
    
    logger.LogDebug("Linked operation completed: {OperationName}", operationName);
}

app.Run();
