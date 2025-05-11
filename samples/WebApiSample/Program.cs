using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OpenTelemetry;
using OpenTelemetry.Context;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading.Tasks;
using TelemetryLibrary;

const string ServiceName = "WebApiSample";

var builder = WebApplication.CreateBuilder(args);


Console.WriteLine("Configuration sources loaded:");
foreach (var provider in ((IConfigurationRoot)builder.Configuration).Providers)
{
    Console.WriteLine($" - {provider.GetType().Name}");
}

Console.WriteLine("IMPORTANT: To see traces in App Insights, make sure to set a valid App Insights connection string");
Console.WriteLine("in appsettings.json under telemetry:exporters:appInsights:connectionString");

var telemetryConfig = builder.Configuration.GetSection("telemetry");
if (telemetryConfig.Exists())
{
    Console.WriteLine("Telemetry configuration found in appsettings.json");
    Console.WriteLine($" - Environment: {telemetryConfig.GetValue<string>("resource:environment")}");
    Console.WriteLine($" - Component: {telemetryConfig.GetValue<string>("resource:component")}");
}
else
{
    Console.WriteLine("No telemetry configuration found in appsettings.json");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<SimpleService>();

builder.AddTelemetry(builder.Configuration);

var app = builder.Build();

var tracerProvider = app.Services.GetRequiredService<TracerProvider>();
var meterProvider = app.Services.GetRequiredService<MeterProvider>();

var tracer = tracerProvider.GetTracer(ServiceName);
var meter = new Meter(ServiceName);

var requestCounter = meter.CreateCounter<long>("http.requests");
var requestDuration = meter.CreateHistogram<double>("http.request.duration");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/hello", (ILogger<Program> logger, SimpleService simpleService) => 
{
    logger.LogInformation("Processing /hello request");
    
    var span = tracer.StartSpan("HelloEndpoint", SpanKind.Server);
    using (span)
    {
        span.SetAttribute("endpoint", "/hello");
        span.SetAttribute("request.time", DateTime.UtcNow.ToString("o"));
        
        requestCounter.Add(1, new KeyValuePair<string, object>("endpoint", "/hello"));
        
        var startTime = DateTime.UtcNow;
        
        try
        {
            logger.LogDebug("Generating hello response");
            
            simpleService.PrintMessage("Processing endpoint: /hello");
            
            Task.Delay(50).Wait();
            
            logger.LogInformation("Successfully processed /hello request");
            
            return "Hello from OpenTelemetry-enabled Web API!";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing /hello request");
            span.SetStatus(Status.Error.WithDescription(ex.Message));
            requestCounter.Add(1, new KeyValuePair<string, object>("result", "error"));
            throw;
        }
        finally
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            requestDuration.Record(duration, new KeyValuePair<string, object>("endpoint", "/hello"));
            
            span.AddEvent("Request completed");
        }
    }
});

app.MapGet("/linked-operation", async (ILogger<Program> logger, SimpleService simpleService) => 
{
    logger.LogInformation("Processing /linked-operation request");
    
    var parentSpan = tracer.StartSpan("LinkedOperationParent", SpanKind.Server);
    var parentContext = parentSpan.Context;
    
    using (parentSpan)
    {
        parentSpan.SetAttribute("endpoint", "/linked-operation");
        
        requestCounter.Add(1, new KeyValuePair<string, object>("endpoint", "/linked-operation"));
        
        var startTime = DateTime.UtcNow;
        
        try
        {
            simpleService.PrintMessage("Processing endpoint: /linked-operation");
            
            await PerformOperationAsync("FirstOperation", logger, tracer, parentContext);
            
            await PerformLinkedOperationAsync("SecondOperation", logger, tracer, parentContext);
            
            logger.LogInformation("Successfully processed /linked-operation request");
            
            return "Linked operations completed successfully!";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing /linked-operation request");
            parentSpan.SetStatus(Status.Error.WithDescription(ex.Message));
            throw;
        }
        finally
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            requestDuration.Record(duration, new KeyValuePair<string, object>("endpoint", "/linked-operation"));
        }
    }
});

app.MapGet("/metrics", (ILogger<Program> logger, SimpleService simpleService) => 
{
    logger.LogInformation("Processing /metrics request");
    
    simpleService.PrintMessage("Processing endpoint: /metrics");
    
    meter.CreateObservableGauge("custom.value", () => 
    {
        return new[] { new Measurement<double>(42.0) };
    });
    
    var operationCounter = meter.CreateCounter<long>("custom.operations");
    
    operationCounter.Add(1, new KeyValuePair<string, object>("operation", "read"));
    operationCounter.Add(2, new KeyValuePair<string, object>("operation", "write"));
    
    logger.LogInformation("Metrics recorded successfully");
    
    return "Metrics recorded successfully!";
});

async Task PerformOperationAsync(string operationName, ILogger logger, Tracer tracer, SpanContext parentContext)
{
    using var scope = new Activity("OperationScope").Start();
    
    var childSpan = tracer.StartSpan(operationName, SpanKind.Internal);
    using (childSpan)
    {
        childSpan.SetAttribute("operation.name", operationName);
        
        logger.LogInformation("Performing operation: {OperationName}", operationName);
        
        await Task.Delay(100);
        
        logger.LogDebug("Operation completed: {OperationName}", operationName);
    }
}

async Task PerformLinkedOperationAsync(string operationName, ILogger logger, Tracer tracer, SpanContext parentContext)
{
    var linkedSpan = tracer.StartSpan(operationName, SpanKind.Internal);
        
    using (linkedSpan)
    {
        linkedSpan.SetAttribute("operation.name", operationName);
        linkedSpan.SetAttribute("has_link", true);
        
        logger.LogInformation("Performing linked operation: {OperationName}", operationName);
        
        await Task.Delay(150);
        
        linkedSpan.AddEvent("Processing step completed");
        
        logger.LogDebug("Linked operation completed: {OperationName}", operationName);
    }
}

app.MapGet("/with-baggage", (HttpRequest request, ILogger<Program> logger, SimpleService simpleService) =>
{
    logger.LogInformation("Processing /with-baggage request");
    
    var span = tracer.StartSpan("BaggageEndpoint", SpanKind.Server);
    using (span)
    {
        span.SetAttribute("endpoint", "/with-baggage");
        
        if (request.Headers.TryGetValue("baggage", out StringValues baggageHeader))
        {
            logger.LogInformation("Received baggage header: {BaggageHeader}", baggageHeader.ToString());
            
            var baggageItems = baggageHeader.ToString().Split(',');
            foreach (var item in baggageItems)
            {
                var parts = item.Split('=');
                if (parts.Length == 2)
                {
                    Activity.Current?.AddBaggage(parts[0].Trim(), parts[1].Trim());
                    
                    span.SetAttribute($"baggage.{parts[0].Trim()}", parts[1].Trim());
                    
                    simpleService.PrintMessage($"Baggage item: {parts[0].Trim()}={parts[1].Trim()}");
                }
            }
        }
        else
        {
            logger.LogInformation("No baggage header found");
            simpleService.PrintMessage("No baggage header found in request");
        }
        
        var baggageInfo = new Dictionary<string, string>();
        
        foreach (var item in Activity.Current?.Baggage ?? new List<KeyValuePair<string, string>>())
        {
            baggageInfo[item.Key] = item.Value;
        }
        
        simpleService.PrintMessage($"Processing endpoint: /with-baggage");
        
        return Results.Ok(new
        {
            message = "Baggage processed successfully",
            baggage = baggageInfo
        });
    }
});

app.Run();
