# Web API Sample

This sample demonstrates how to use the OpenTelemetry Extensions with a Web API application and Azure Application Insights.

## Configuration

The sample is configured to use Azure Application Insights as the telemetry exporter. You need to update the connection string in `appsettings.json`:

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
dotnet run
```

The sample will:
1. Configure OpenTelemetry with the extensions
2. Start a Web API with a simple `/hello` endpoint
3. Create a span for each request to the endpoint
4. Send the telemetry to Application Insights

## Testing the API

Once the application is running, you can test it by navigating to:

```
https://localhost:5001/hello
```

Or using curl:

```bash
curl -k https://localhost:5001/hello
```

## Viewing Telemetry

After running the sample and making requests, you can view the telemetry in the Azure Portal:
1. Navigate to your Application Insights resource
2. Go to "Transaction Search" or "Application Map" to see the traces
3. Check "Logs" to query the telemetry data
