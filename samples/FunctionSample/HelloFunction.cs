using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FunctionSample
{
    public class HelloFunction
    {
        private readonly ILogger _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("FunctionSample");
        private static readonly Meter _meter = new Meter("FunctionSample", "1.0.0");
        
        private readonly Counter<int> _requestCounter;
        private readonly Histogram<double> _requestDuration;

        public HelloFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HelloFunction>();
            
            _requestCounter = _meter.CreateCounter<int>("function.requests");
            _requestDuration = _meter.CreateHistogram<double>("function.request.duration");
        }

        [Function("Hello")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing HTTP trigger function request");
            
            using var activity = _activitySource.StartActivity("HelloFunction", ActivityKind.Server);
            activity?.SetTag("function.name", "Hello");
            activity?.SetTag("request.method", req.Method);
            activity?.SetTag("request.url", req.Url.ToString());
            activity?.SetTag("request.time", DateTime.UtcNow);
            
            _requestCounter.Add(1, new KeyValuePair<string, object?>("function", "Hello"));
            
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
                
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                
                _requestCounter.Add(1, new KeyValuePair<string, object?>("result", "error"));
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.WriteString("An error occurred processing your request.");
                return errorResponse;
            }
            finally
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _requestDuration.Record(duration, new KeyValuePair<string, object?>("function", "Hello"));
                
                activity?.AddEvent(new ActivityEvent("Function completed", 
                    DateTime.UtcNow, 
                    new ActivityTagsCollection { 
                        { "duration_ms", duration }
                    }));
                
                _logger.LogDebug("Function execution completed in {DurationMs}ms", duration);
            }
        }
        
        [Function("ProcessWithLinks")]
        public async Task<HttpResponseData> ProcessWithLinks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing linked operations function request");
            
            using var parentActivity = _activitySource.StartActivity("ProcessWithLinks", ActivityKind.Server);
            parentActivity?.SetTag("function.name", "ProcessWithLinks");
            parentActivity?.SetTag("request.method", req.Method);
            
            _requestCounter.Add(1, new KeyValuePair<string, object?>("function", "ProcessWithLinks"));
            
            var startTime = DateTime.UtcNow;
            
            try
            {
                await PerformOperationAsync("FirstOperation");
                
                await PerformLinkedOperationAsync("SecondOperation", parentActivity);
                
                _logger.LogInformation("Successfully processed linked operations");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                response.WriteString("Linked operations completed successfully!");
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing linked operations");
                parentActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.WriteString("An error occurred processing your request.");
                return errorResponse;
            }
            finally
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _requestDuration.Record(duration, new KeyValuePair<string, object?>("function", "ProcessWithLinks"));
            }
        }
        
        [Function("RecordMetrics")]
        public HttpResponseData RecordMetrics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Processing metrics recording function request");
            
            var customGauge = _meter.CreateObservableGauge("custom.value", () => 
            {
                return new[] { new Measurement<double>(42.0) };
            });
            
            var operationCounter = _meter.CreateCounter<int>("custom.operations");
            
            operationCounter.Add(1, new KeyValuePair<string, object?>("operation", "read"));
            operationCounter.Add(2, new KeyValuePair<string, object?>("operation", "write"));
            
            _logger.LogInformation("Metrics recorded successfully");
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Metrics recorded successfully!");
            
            return response;
        }
        
        private async Task PerformOperationAsync(string operationName)
        {
            using var childActivity = _activitySource.StartActivity(
                operationName, 
                ActivityKind.Internal, 
                parentContext: Activity.Current?.Context);
                
            childActivity?.SetTag("operation.name", operationName);
            
            _logger.LogInformation("Performing operation: {OperationName}", operationName);
            
            await Task.Delay(100);
            
            _logger.LogDebug("Operation completed: {OperationName}", operationName);
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
            
            await Task.Delay(150);
            
            linkedActivity?.AddEvent(new ActivityEvent("Processing step completed"));
            
            _logger.LogDebug("Linked operation completed: {OperationName}", operationName);
        }
    }
}
