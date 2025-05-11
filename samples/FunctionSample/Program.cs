using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetryExtensions.Extensions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddTelemetry(context.Configuration);
        
        Console.WriteLine("IMPORTANT: To see traces in App Insights, make sure to set a valid App Insights connection string");
        Console.WriteLine("in local.settings.json under telemetry:exporters:appInsights:connectionString and Values:APPLICATIONINSIGHTS_CONNECTION_STRING");
    })
    .Build();

await host.RunAsync();
