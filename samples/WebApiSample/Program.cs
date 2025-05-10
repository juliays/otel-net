using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

const string ServiceName = "WebApiSample";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddTelemetry();

var app = builder.Build();

var tracerProvider = app.Services.GetRequiredService<TracerProvider>();
var meterProvider = app.Services.GetRequiredService<MeterProvider>();

var tracer = tracerProvider.GetTracer(ServiceName);
var meter = meterProvider.GetMeter(ServiceName);

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

app.MapGet("/hello", (ILogger<Program> logger) => 
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
            
            span.AddEvent("Request completed", new SpanAttributes
            {
                { "duration_ms", duration }
            });
        }
    }
});

app.MapGet("/linked-operation", async (ILogger<Program> logger) => 
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

app.MapGet("/metrics", (ILogger<Program> logger) => 
{
    logger.LogInformation("Processing /metrics request");
    
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
    using var scope = parentContext.IsValid ? Baggage.Current.Activate() : default;
    
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
    var linkedSpan = tracer.StartSpan(
        operationName,
        SpanKind.Internal,
        new SpanContext(parentContext.TraceId, SpanId.CreateRandom(), parentContext.TraceFlags, parentContext.IsRemote));
        
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

app.Run();
