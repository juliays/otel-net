using System.Diagnostics;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FunctionSample
{
    public class HelloFunction
    {
        private readonly ILogger _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("FunctionSample");

        public HelloFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HelloFunction>();
        }

        [Function("Hello")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            using var activity = _activitySource.StartActivity("HelloFunction");
            activity?.SetTag("function.name", "Hello");
            
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Hello from OpenTelemetry-enabled Azure Function!");

            activity?.AddEvent(new ActivityEvent("Function completed"));

            return response;
        }
    }
}
