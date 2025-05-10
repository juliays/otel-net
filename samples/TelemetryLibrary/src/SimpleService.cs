using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelemetryLibrary
{
    public class SimpleService
    {
        private readonly ILogger<SimpleService> _logger;
        private readonly Tracer _tracer;
        private readonly OpenTelemetry.Metrics.Meter _meter;
        private readonly Counter<long> _operationCounter;
        
        public SimpleService(
            ILogger<SimpleService> logger, 
            TracerProvider tracerProvider, 
            MeterProvider meterProvider)
        {
            _logger = logger;
            _tracer = tracerProvider.GetTracer("TelemetryLibrary");
            _meter = meterProvider.GetMeter("TelemetryLibrary");
            
            _operationCounter = _meter.CreateCounter<long>("library.operations");
        }
        
        public void PrintMessage(string message)
        {
            var span = _tracer.StartSpan("PrintMessage", SpanKind.Internal);
            using (span)
            {
                span.SetAttribute("message", message);
                
                _logger.LogInformation("Printing message: {Message}", message);
                
                Console.WriteLine($"SimpleService says: {message}");
                
                _operationCounter.Add(1, new KeyValuePair<string, object>("operation", "print"));
                
                span.AddEvent("Message printed");
            }
        }
        
        public async Task ProcessWithLinkAsync(string operationId)
        {
            var parentSpan = _tracer.StartSpan("ProcessOperation", SpanKind.Internal);
            var parentContext = parentSpan.Context;
            
            using (parentSpan)
            {
                parentSpan.SetAttribute("operation.id", operationId);
                
                _logger.LogInformation("Processing operation: {OperationId}", operationId);
                
                await Task.Delay(100);
                
                var linkedSpan = _tracer.StartSpan(
                    "LinkedOperation",
                    SpanKind.Internal,
                    new SpanContext(parentContext.TraceId, SpanId.CreateRandom(), parentContext.TraceFlags, parentContext.IsRemote));
                    
                using (linkedSpan)
                {
                    linkedSpan.SetAttribute("operation.id", operationId);
                    linkedSpan.SetAttribute("has_link", true);
                    
                    _logger.LogInformation("Performing linked operation for: {OperationId}", operationId);
                    
                    await Task.Delay(50);
                    
                    Console.WriteLine($"Linked operation completed for: {operationId}");
                    
                    _operationCounter.Add(1, new KeyValuePair<string, object>("operation", "linked"));
                }
                
                _logger.LogInformation("Operation completed: {OperationId}", operationId);
            }
        }
    }
}
