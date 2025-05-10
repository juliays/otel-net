# OpenTelemetry Extensions for .NET 8

This PR implements an extension method that works with Azure Functions, Web APIs, and Console apps to add OpenTelemetry traces, logs, and metrics.

## Features

- Adds OpenTelemetry traces, logs, and metrics to .NET 8 applications
- Works with Azure Functions, Web APIs, and Console apps
- Uses SeriLog as the logging provider
- Supports multiple exporters:
  - AppInsights: Uses Azure.Monitor.OpenTelemetry.AspNetCore.OpenTelemetryBuilderExtensions.UserAzureMonitor()
  - Datadog: Uses OpenTelemetry.Extensions.Hosting
  - Console: For local development and debugging

## Implementation Details

- Configuration binding for telemetry settings
- Resource attributes configuration
- Trace and metric instrumentation
- SeriLog integration
- Extension methods for different application types

## Sample Applications

The PR includes sample applications demonstrating all three scenarios:
- Console Application
- Web API
- Azure Functions

Each sample shows how to integrate with App Insights and includes configuration examples.

## Testing

The implementation includes unit tests for all extension methods and configuration classes.

## Requested by

Yang Song (songy@microsoft.com)

## Link to Devin run

https://app.devin.ai/sessions/d8f5e00a6b2846cbb8e7564c5964dad8
