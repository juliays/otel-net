# Azure Functions Sample

This sample demonstrates how to use the OpenTelemetry Extensions with an Azure Functions application and Azure Application Insights.

## Configuration

The sample is configured to use Azure Application Insights as the telemetry exporter. You need to update the connection string in `local.settings.json`:

```json
"APPLICATIONINSIGHTS_CONNECTION_STRING": "YOUR_APP_INSIGHTS_CONNECTION_STRING"
```

And in the telemetry configuration:

```json
"appInsights": {
  "enabled": true,
  "connectionString": "YOUR_APP_INSIGHTS_CONNECTION_STRING"
}
```

Also update the Serilog ApplicationInsights sink configuration:

```json
"Name": "ApplicationInsights",
"Args": {
  "connectionString": "YOUR_APP_INSIGHTS_CONNECTION_STRING",
  "telemetryConverter": "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
}
```

## Running the Sample

To run the sample:

```bash
func start
```

Or using the .NET CLI:

```bash
dotnet run
```

The sample will:
1. Configure OpenTelemetry with the extensions
2. Start an Azure Function with an HTTP trigger
3. Create a span for each function execution
4. Send the telemetry to Application Insights

## Testing the Function

Once the application is running, you can test it by navigating to:

```
http://localhost:7071/api/Hello
```

Or using curl:

```bash

```

## Viewing Telemetry

After running the sample and making requests, you can view the telemetry in the Azure Portal:
1. Navigate to your Application Insights resource
2. Go to "Transaction Search" or "Application Map" to see the traces
3. Check "Logs" to query the telemetry data
curl http://localhost:7071/api/Hello