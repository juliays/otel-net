# OpenTelemetry Extensions Samples

This directory contains sample applications demonstrating how to use the OpenTelemetry Extensions with different application types and Azure Application Insights.

## Samples

### Console Application

The [AppInsightsSample](./AppInsightsSample) demonstrates using OpenTelemetry Extensions with a console application.

### Web API

The [WebApiSample](./WebApiSample) demonstrates using OpenTelemetry Extensions with an ASP.NET Core Web API.

### Azure Functions

The [FunctionSample](./FunctionSample) demonstrates using OpenTelemetry Extensions with Azure Functions.

## Configuration

Each sample includes its own configuration files with placeholders for your Application Insights connection string. Before running the samples, update the connection string in the respective configuration files.

## Common Features

All samples demonstrate:

1. Configuring OpenTelemetry with the extensions
2. Creating spans/activities for operations
3. Adding tags and events to spans
4. Sending telemetry to Application Insights
5. Using SeriLog for logging

## Running the Samples

Each sample directory contains a README with specific instructions for running and testing that sample.
