# Telemetry Library for .NET

This library simplifies the integration of OpenTelemetry into your .NET applications, providing a unified approach to collect traces, metrics, and logs. It utilizes appSettings.json to configure telemetry pipeline and supports various application types, including Console Apps, Web APIs, Class Libraries, and Azure Functions (Isolated Process). It follows the pattern and tools defined in [Activity Telemtery Design ADR](../../../../docs/adr/2025-03-12-activity-telemetry-design.md) and [Metrics Telemetry Design ADR](../../../../docs/adr/2025-03-24-metrics-telemetry-design.md).

## Table of Contents

- [Telemetry Library for .NET](#telemetry-library-for-net)
  - [Table of Contents](#table-of-contents)
  - [Goals](#goals)
  - [Build](#build)
    - [Common Issues](#common-issues)
  - [Unit Test](#unit-test)
  - [Components](#components)
    - [Telemetry](#telemetry)
  - [Telemetry Samples Section](#telemetry-samples-section)
    - [Available Samples](#available-samples)
  - [Limitations](#limitations)
  - [Additional Resources](#additional-resources)
  - [command to look for MRs merged within a date range.](#command-to-look-for-mrs-merged-within-a-date-range)

## Goals

1. **Consistent Integration of Telemetry Across Application Types**:
   Integrate with the telemetry library for Console Apps, WebAPIs, Class Libraries, and Azure Functions (Isolated Process).

2. **Flexible Configuration**:
   Support for either AppInsights or Datadog exporters (future implementation) with optional Console exporter. You can also configure any OpenTelemetry Protocol (OTLP) exporters by updating the library only and provide transparency to client applications.

   Configuration to update telemetry collection in general can be limited by updating this library.

3. **Dependency Injection (DI)**:
   The library supports .NET DI object creation.

4. **Comprehensive Telemetry**:
   Includes traces, metrics, and logs through Serilog integration.

5. **Rich Context**:
   By configure and use SerilogProvider enricher, we can easily correlate the log with traces or resources as the client applications define.

## Build

To build the library, navigate to the `Telemetry` folder and run the following command:

```bash
dotnet build
```

This will generate the DLL file in the `bin` directory. Ensure you have .NET 8.0 or later installed.

### Common Issues

- **Error: Missing Dependencies**: Ensure all required NuGet packages are installed by running `dotnet restore`.

- **Error: Unsupported .NET Version**: Verify that your .NET SDK version is compatible with the library.

## Unit Test

Navigate to the `Telemetry.Tests` folder and run:

```bash
dotnet test
```

Because the nature of the library is to configure telemetry, only limited unit tests were created to validate parsing of configuration files and some basic program logic. Functional tests and usage samples are provided for each application type it supports in the `Telemetry.Samples` folder.

## Components

### Telemetry

- **TelemetryExtensions.cs**

  The `ConfigureLmpTelemetry()` builder extension method centralizes the setup of OpenTelemetry and Serilog for all supported host types (Web API, Azure Function, Console Apps, and Library Apps). It configures resource metadata, logger, tracing, and metrics exporters based on provided options. It currently supports configuration to export to both Console and App Insights. Going forward, we can update the configuration and export telemetry data to Datadog with client application updates.

  There are two overloads of this method, one that accepts a `TelemetryConfig` (provided by this library) and another that accepts a generic `IConfiguration` object.
  The `TelemetryConfig` overload offers simpler configuration with the tradeoff of less granular control, enforcing the same telemetry provider options across tracing, logging, and metrics. For instance, enabling Console and Application Insights means both will be used across all traces, logs, and metrics.
  Using the `IConfiguration` overload, the configuration is much more complex, but it supports scenarios such as logging only to console but sending traces and metrics only to Applications Insights (and much more).

  The recommended "default" is to use the simpler `TelemetryConfig` overload when possible.

  **Note**: We currently export telemetry to Application Insights. Due to SDK limitations, we had to implement workarounds because the existing SDK drops Resource and Span Links information. The code is clearly marked in comments with `TODO` for future updates when direct export to Datadog is available.

  **LoggerExtensions.cs**

  This class provides a fast, maintainable, and type-safe way to log messages, especially in high-throughput or production scenarios. It replaces older, slower, and more error-prone logging patterns.

  It leverages .NET's source generator infrastructure to generate efficient logging code. Source generators creates static logging delegates at compile time, avoiding the runtime overhead of string formatting, boxing, and reflection that comes with traditional logging methods.

  It does not support event id in the conventional sense. Filtering should no not be based on event id but rather on other ways.

- **telemetry.json**

  This static JSON file represents the underlying configuration structure the library requires in order to set up the underlying components (Serilog, OpenTelemetry, etc.). If the client opts to provide a `TelemetryConfig` argument instead of their own fully populated `IConfiguration` object, this static JSON file is used as the configuration "base" structure. This configuration base is empty, so the provided `TelemetryConfig` values are then used to overwrite it and create a fully populated `IConfiguration`.

  If, however, they opt to provide a fully populated `IConfiguration` object, the structure must match that of the provided `appsettings.json.template` file. One straightforward way to achieve this would be to simply copy that template file and include the same exact fields in the client's own `appsettings.json`.

- **Instrumentation.cs**

  This is a lightweight wrapper that bundles `ActivitySource` and `Meter`, providing a unified interface and helper methods for tracing and metrics. It simplifies dependency injection in libraries by reducing telemetry-related constructor parameters. Separating `ILogger` from `ActivitySource` and `Meter` makes it easier to integrate with existing .NET patterns.

  **Note**: We opted for `ActivitySource` instead of `TelemetrySpan` because it is the idiomatic way to create spans in .NET and is fully supported by the OpenTelemetry SDK, .NET runtime, and ecosystem libraries like ASP.NET Core and Azure Functions. .NET integrates `Activity` directly with the diagnostics infrastructure and native instrumentation, making `ActivitySource` the most compatible and future-proof approach for distributed tracing.

- **Configuration Folder**

  These are classes that support configuration auto-binding of service-level tags. For traces, the resource information are currently being added as tags to trace records. When moving to OTLP exporter, this work around is no longer necessary.

  It's expected that either otel collector or the azure function will handle cross service/platform level tags required by LSEG.

## Telemetry Samples Section

The `Telemetry.Samples` folder contains sample applications demonstrating how to integrate the library into various application types. Each sample includes a `README.md` file with detailed instructions.

### Available Samples

- **ConsoleAppSample**: Demonstrates telemetry integration in a console application.

- **FunctionAppSample**: Shows how to use the library in an Azure Function (Isolated Process).

- **LibrarySample**: Provides an example of integrating telemetry into a class library.

- **WebApiSample**: Explains how to set up telemetry for an ASP.NET Core Web API. This is the most extensive example that shows how to add custom tags, specify span link and record metrics.

Refer to README.MD in each sample folder to see how to execute the sample.

```bash
dotnet run
```

## Limitations

- Host metrics are not supported by Azure Functions at the time of implementation.

- There are some limitations in OpenTelemetry output support and testing as listed in the [documentation](https://learn.microsoft.com/en-us/azure/azure-functions/opentelemetry-howto?tabs=app-insights&pivots=programming-language-csharp#considerations-for-opentelemetry).

- Open Telemetry AzureMonitor Exporter has a list of data drop that are listed here. We implemented workaround where possible as specified below:

1. `Resource` is ignored when exporting to `dependencies` table. An activityListener is added to add Resource information as a tag so it gets recorded in customDimensions.

2. `ActivityLink` is dropped. Again, work around is to add tags when record the span so it shows up in customDimensions

3. `Resource` and `span` related information are dropped for exception logging to `exceptions` table.

4. `Resource` and `examplars` are dropped when recording metrics. All `resource` and `span` related information need to be added when making the record metrics call and pass in as tags.

## Additional Resources

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/zero-code/dotnet/custom/)
- [Sealed Class Unit Testing](https://github.com/dotnet/runtime/issues/100813)
  
## command to look for MRs merged within a date range.

``` bash

# This script will retrieve all MRs merged bewteen the two dates in descending order.git 
git log main --merges --since="2025-05-15" --until="2025-06-18" \
--pretty=format:"--ENTRY--%nDate: %ad%nAuthor: %an%nHash: %h%n%n%B" --date=short | awk '
BEGIN {
  RS="--ENTRY--"
  FS="\n"
}
{
  date = ""; author = ""; hash = ""; mr = "N/A"; branch = "N/A"
  for (i = 1; i <= NF; i++) {
    line = $i
    if (line ~ /^Date: /) {
      date = substr(line, 7)
    } else if (line ~ /^Author: /) {
      author = substr(line, 9)
    } else if (line ~ /^Hash: /) {
      hash = substr(line, 7)
    } else if (line ~ /merge request .*![0-9]+/) {
      n = split(line, parts, "!")
      mr = parts[n]
      gsub(/[^0-9].*/, "", mr)
    } else if (line ~ /Merge branch '\''/) {
      # Extract branch between single quotes safelysplit(line, s, "'\''")
      if (length(s) > 1) {
        branch = s[2]      }    }  }  if (date != "" && author != "" && hash != "")
    print "Date: " date " | Author: " author " | Hash: " hash " | MR: " mr " | Branch: " branch
}
'
 

```
