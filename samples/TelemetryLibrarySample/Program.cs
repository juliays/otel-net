using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Threading.Tasks;
using TelemetryLibrary;

namespace TelemetryLibrarySample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<SimpleService>();
                    
                    services.AddTransient<SampleService>();
                })
                .AddTelemetry()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application starting up");
            
            try
            {
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
        private readonly SimpleService _simpleService;
        
        public SampleService(ILogger<SampleService> logger, SimpleService simpleService)
        {
            _logger = logger;
            _simpleService = simpleService;
        }
        
        public async Task RunAsync()
        {
            _logger.LogInformation("Starting sample operations");
            
            _simpleService.PrintMessage("Hello from TelemetryLibrarySample!");
            _simpleService.PrintMessage("This is a demonstration of constructor injection for telemetry");
            
            await _simpleService.ProcessWithLinkAsync(Guid.NewGuid().ToString());
            
            _logger.LogInformation("All operations completed successfully");
        }
    }
}
