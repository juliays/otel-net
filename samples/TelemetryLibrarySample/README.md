# Telemetry Library Sample Application

This sample application demonstrates how to use the TelemetryLibrary with constructor injection for telemetry components.

## Features

- Uses the TelemetryLibrary to demonstrate telemetry integration
- Shows how to register the library in the dependency injection container
- Demonstrates how telemetry is automatically propagated through the library

## How It Works

1. The application configures OpenTelemetry using the extension methods from the main project
2. The SimpleService from TelemetryLibrary is registered in the dependency injection container
3. The application's SampleService injects the SimpleService
4. When methods on SimpleService are called, they automatically create spans, record metrics, and log messages

## Key Points

- The application only needs to configure telemetry once at the host level
- The library receives the telemetry components through constructor injection
- The application code doesn't need to know about the telemetry implementation details in the library
- Telemetry context is automatically propagated between the application and library

This demonstrates how to create libraries that are telemetry-aware without requiring the consuming application to manually configure telemetry for the library.
