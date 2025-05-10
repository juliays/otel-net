using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace FunctionSample
{
    public class HelloFunction
    {
        private readonly ILogger _logger;
        private readonly TracerProvider _tracerProvider;
        private readonly MeterProvider _meterProvider;
        private readonly Tracer _tracer;
        private readonly OpenTelemetry.Metrics.Meter _meter;
        
        private readonly Counter<long> _requestCounter;
        private readonly Histogram<double> _requestDuration;

        public HelloFunction(ILoggerFactory loggerFactory, TracerProvider tracerProvider, MeterProvider meterProvider)
        {
            _logger = loggerFactory.CreateLogger<HelloFunction>();
            _tracerProvider = tracerProvider;
            _meterProvider = meterProvider;
            
            _tracer = _tracerProvider.GetTracer("FunctionSample");
            _meter = _meterProvider.GetMeter("FunctionSample");
            
            _requestCounter = _meter.CreateCounter<long>("function.requests");
            _requestDuration = _meter.CreateHistogram<double>("function.request.duration");
        }

        [Function("Hello")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing HTTP trigger function request");
            
            var span = _tracer.StartSpan("HelloFunction", SpanKind.Server);
            using (span)
            {
                span.SetAttribute("function.name", "Hello");
                span.SetAttribute("request.method", req.Method);
                span.SetAttribute("request.url", req.Url.ToString());
                span.SetAttribute("request.time", DateTime.UtcNow.ToString("o"));
                
                _requestCounter.Add(1, new KeyValuePair<string, object>("function", "Hello"));
                
                var startTime = DateTime.UtcNow;
                
                try
                {
                    _logger.LogDebug("Generating response for HTTP trigger function");
                    
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                    response.WriteString("Hello from OpenTelemetry-enabled Azure Function!");
                    
                    _logger.LogInformation("Successfully processed HTTP trigger function request");
                    
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing HTTP trigger function request");
                    
                    span.SetStatus(Status.Error.WithDescription(ex.Message));
                    
                    _requestCounter.Add(1, new KeyValuePair<string, object>("result", "error"));
                    
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    errorResponse.WriteString("An error occurred processing your request.");
                    return errorResponse;
                }
                finally
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _requestDuration.Record(duration, new KeyValuePair<string, object>("function", "Hello"));
                    
                    span.AddEvent("Function completed", new SpanAttributes
                    {
                        { "duration_ms", duration }
                    });
                    
                    _logger.LogDebug("Function execution completed in {DurationMs}ms", duration);
                }
            }
        }
        
        [Function("ProcessWithLinks")]
        public async Task<HttpResponseData> ProcessWithLinks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing linked operations function request");
            
            var parentSpan = _tracer.StartSpan("ProcessWithLinks", SpanKind.Server);
            var parentContext = parentSpan.Context;
            
            using (parentSpan)
            {
                parentSpan.SetAttribute("function.name", "ProcessWithLinks");
                parentSpan.SetAttribute("request.method", req.Method);
                
                _requestCounter.Add(1, new KeyValuePair<string, object>("function", "ProcessWithLinks"));
                
                var startTime = DateTime.UtcNow;
                
                try
                {
                    await PerformOperationAsync("FirstOperation", parentContext);
                    
                    await PerformLinkedOperationAsync("SecondOperation", parentContext);
                    
                    _logger.LogInformation("Successfully processed linked operations");
                    
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                    response.WriteString("Linked operations completed successfully!");
                    
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing linked operations");
                    parentSpan.SetStatus(Status.Error.WithDescription(ex.Message));
                    
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    errorResponse.WriteString("An error occurred processing your request.");
                    return errorResponse;
                }
                finally
                {
                    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _requestDuration.Record(duration, new KeyValuePair<string, object>("function", "ProcessWithLinks"));
                }
            }
        }
        
        [Function("RecordMetrics")]
        public HttpResponseData RecordMetrics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Processing metrics recording function request");
            
            _meter.CreateObservableGauge("custom.value", () => 
            {
                return new[] { new Measurement<double>(42.0) };
            });
            
            var operationCounter = _meter.CreateCounter<long>("custom.operations");
            
            operationCounter.Add(1, new KeyValuePair<string, object>("operation", "read"));
            operationCounter.Add(2, new KeyValuePair<string, object>("operation", "write"));
            
            _logger.LogInformation("Metrics recorded successfully");
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Metrics recorded successfully!");
            
            return response;
        }
        
        private async Task PerformOperationAsync(string operationName, SpanContext parentContext)
        {
            using var scope = parentContext.IsValid ? Baggage.Current.Activate() : default;
            
            var childSpan = _tracer.StartSpan(operationName, SpanKind.Internal);
            using (childSpan)
            {
                childSpan.SetAttribute("operation.name", operationName);
                
                _logger.LogInformation("Performing operation: {OperationName}", operationName);
                
                await Task.Delay(100);
                
                _logger.LogDebug("Operation completed: {OperationName}", operationName);
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
                
                await Task.Delay(150);
                
                linkedSpan.AddEvent("Processing step completed");
                
                _logger.LogDebug("Linked operation completed: {OperationName}", operationName);
            }
        }
    }
}
