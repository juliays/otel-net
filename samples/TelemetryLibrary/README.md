# Telemetry Library Sample

This is a simple library that demonstrates how to use constructor injection for telemetry components in a .NET library. The library only performs simple print operations but shows how to properly integrate OpenTelemetry tracing and metrics.

## Features

- Constructor injection for telemetry components
- OpenTelemetry Tracer and Meter usage
- Span creation and attribute setting
- Metric recording
- Span linking

## Usage

To use this library in your application:

1. Register the library in your dependency injection container
2. Configure OpenTelemetry using the extensions from the main project
3. Inject the library into your application components

## Example

```csharp
// In Program.cs or Startup.cs
services.AddTransient<TelemetryLibrary.SimpleService>();

// In your application code
public class MyService
{
    private readonly TelemetryLibrary.SimpleService _simpleService;
    
    public MyService(TelemetryLibrary.SimpleService simpleService)
    {
        _simpleService = simpleService;
    }
    
    public void DoSomething()
    {
        _simpleService.PrintMessage("Hello from MyService!");
    }
}
```

The library will automatically create spans, record metrics, and log messages when its methods are called.
