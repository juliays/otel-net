# OpenTelemetry Extensions

A .NET 8 library that provides extension methods for adding OpenTelemetry traces, logs, and metrics to Azure Functions, Web APIs, and Console apps using SeriLog as the logging provider.

## Features

- Works with Azure Functions, Web APIs, and Console apps
- Adds OpenTelemetry traces, logs, and metrics
- Uses Azure.Monitor.OpenTelemetry.AspNetCore for AppInsights exporter
- Uses OpenTelemetry.Extensions.Hosting for Datadog exporter
- Uses SeriLog as the logging provider
- Supports configuration binding

## Installation

```bash
dotnet add package OpenTelemetryExtensions
```

## Usage

### Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "telemetry": {
    "resource": {
      "environment": "development",
      "component": "web-api",
      "workspaceId": "1a84f41a-a0d6-474e-9f57-449692607035",
      "notebookId": "a12bf41a-a0d6-474e-9f57-449692607035",
      "livyId": "bb503baf-395b-4b10-b1b4-beee019192d9",
      "region": "us-east-1",
      "websiteName": "a0b-51982-dev-app-mds-eus2-002-linux",
      "websiteInstance": "4c49e6772844dbc3bf81feba04f36b8c00a81d9a9c90e9290f6e09fa75b870d6",
      "mnd-applicationid": "app-51982",
      "cloud_provider": "azure",
      "opt-dora": "false",
      "opt-service-id": "srv-70673"
    },
    "Serilog": {
      "MinimumLevel": {
        "Default": "Information",
        "Override": {
          "Microsoft": "Warning",
          "System": "Warning"
        }
      },
      "WriteTo": [
        {
          "Name": "Console",
          "Args": {
            "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
          }
        },
        {
          "Name": "ApplicationInsights",
          "Args": {
            "connectionString": "your-connection-string",
            "telemetryConverter": "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
          }
        }
      ],
      "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
    },
    "exporters": {
      "console": {
        "enabled": true,
        "includeScopes": true
      },
      "appInsights": {
        "enabled": true,
        "connectionString": "your-instrumentation-key"
      },
      "datadog": {
        "enabled": false,
        "endpoint": "https://api.datadoghq.com",
        "apiKey": ""
      }
    },
    "tracer": {
      "sampleRate": 1.0
    }
  }
}
```

### Web API

```csharp
using OpenTelemetryExtensions.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add telemetry
builder.AddTelemetry();

// Add services
builder.Services.AddControllers();

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

### Azure Functions

```csharp
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using OpenTelemetryExtensions.Extensions;

[assembly: FunctionsStartup(typeof(MyNamespace.Startup))]
namespace MyNamespace
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddTelemetry();
        }
    }
}
```

### Console App

```csharp
using Microsoft.Extensions.Hosting;
using OpenTelemetryExtensions.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .AddTelemetry()
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<MyService>();
    })
    .Build();

await host.RunAsync();
```

### Custom Configuration

You can also customize the OpenTelemetry configuration:

```csharp
builder.AddTelemetry(telemetryBuilder =>
{
    telemetryBuilder.WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder.AddSource("MySource");
    });
});
```

## License

MIT
