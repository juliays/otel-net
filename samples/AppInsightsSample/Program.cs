using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AppInsightsSample
{
    public class Program
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("AppInsightsSample");
        
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

            var sampleService = host.Services.GetRequiredService<SampleService>();
            await sampleService.RunAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public class SampleService
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("AppInsightsSample.SampleService");
        
        public async Task RunAsync()
        {
            using var activity = _activitySource.StartActivity("SampleOperation");
            activity?.SetTag("operation.name", "SampleOperation");
            activity?.SetTag("operation.value", 42);
            
            Console.WriteLine("Running sample operation...");
            
            await Task.Delay(1000);
            
            activity?.AddEvent(new ActivityEvent("Operation completed"));
            
            Console.WriteLine("Sample operation completed.");
        }
    }
}
