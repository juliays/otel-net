using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using Telemetry.Configuration;
using Telemetry.Constants;
using Telemetry.Exceptions;
using Telemetry.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;

namespace Telemetry.Extensions;

/// <summary>
/// Extensions for setting up telemetry in a .NET application.
/// This is designed to work with WebAPI, Console app and Azure Functions.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds telemetry services to the provided service collection using a provided LMP TelemetryConfig.
    /// This will use the same provider sinks across tracing, logging, and metrics.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="telemetryConfig">The LMP telemetry configuration to use.</param>
    /// <param name="configureTracing">Optional action to configure tracing.</param>
    /// <param name="configureMetrics">Optional action to configure metrics.</param>
    /// <param name="commonTags">Optional dictionary of common tags to apply to telemetry.</param>
    /// <returns>The configured host builder.</returns>
    public static IHostBuilder ConfigureLmpTelemetry(
        this IHostBuilder builder,
        TelemetryConfig telemetryConfig,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null,
        Dictionary<string, string>? commonTags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(telemetryConfig);

        var telemetryConfiguration = BuildConfiguration(telemetryConfig);
        return builder.ConfigureLmpTelemetry(
            telemetryConfiguration,
            configureTracing,
            configureMetrics,
            commonTags);
    }

    /// <summary>
    /// Adds telemetry services to the provided service collection using a provided IConfiguration.
    /// This is fully configurable and allows for different providers to be used across tracing, logging, and metrics.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="configuration">The IConfiguration to use.</param>
    /// <param name="configureTracing">Optional action to configure tracing.</param>
    /// <param name="configureMetrics">Optional action to configure metrics.</param>
    /// <param name="commonTags">Optional dictionary of common tags to apply to telemetry.</param>
    /// <returns>The configured host builder.</returns>
    public static IHostBuilder ConfigureLmpTelemetry(
        this IHostBuilder builder,
        IConfiguration configuration,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null,
        Dictionary<string, string>? commonTags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var telemetryOptions = BindTelemetryOptions(configuration);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromGlobalLogContext()
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .Enrich.WithExceptionDetails()
            .CreateLogger();

        // Add all tags to the global log context.
        // If any provided tags conflict with these default tags, they will take precedence.
        // Note that Component and Version tags are also part of Resource and can be removed after switching to OTLP exporter.
        var tags = new Dictionary<string, string>
        {
            { TelemetryConstants.ComponentTag, telemetryOptions.Resource.Component },
            { TelemetryConstants.VersionTag, telemetryOptions.Resource.Version },
            { TelemetryConstants.WebsiteNameTag, telemetryOptions.Resource.WebsiteName },
            { TelemetryConstants.WebsiteInstanceTag, telemetryOptions.Resource.WebsiteInstance },
            { TelemetryConstants.EnvironmentTag, telemetryOptions.Resource.Environment },
            { TelemetryConstants.RegionTag, telemetryOptions.Resource.Region },
        };
        if (commonTags != null)
        {
            foreach (var commonTag in commonTags)
            {
                tags[commonTag.Key] = commonTag.Value;
            }
        }

        foreach (var tag in tags)
        {
            GlobalLogContext.PushProperty(tag.Key, tag.Value);
        }
        builder.UseSerilog();

        // NOTE: reevaluate if this is necessary when exporting to Datadog directly.
        // Added the activity listener to add resource attributes to all activities.
        // This is because app insights drops resource attributes when sending telemetry to dependencies table
        // when using open telemetry API/SDK.
        var listener = new ActivityListener
        {
            ShouldListenTo = source => true, // listen to all sources (or filter)
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            },
            ActivityStopped = activity => { },
        };
        ActivitySource.AddActivityListener(listener);

        builder.ConfigureServices((context, services) =>
        {
            ConfigureTraceAndMetrics(services, telemetryOptions, configureTracing, configureMetrics);
        });
        return builder;
    }

    /// <summary>
    /// Binds configuration to a TelemetryOptions object and returns it.
    /// </summary>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <returns>A TelemetryOptions object populated with configuration values.</returns>
    public static TelemetryOptions BindTelemetryOptions(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configuration.GetSection(TelemetryJsonPaths.ResourceSection));
        ArgumentNullException.ThrowIfNull(configuration.GetSection(TelemetryJsonPaths.ExportersSection));
        ArgumentNullException.ThrowIfNull(configuration.GetSection(TelemetryJsonPaths.TracerSection));

        var telemetryOptions = new TelemetryOptions
        {
            Resource = configuration.GetSection(TelemetryJsonPaths.ResourceSection).Get<ResourceOptions>() ?? new ResourceOptions(),
            Exporters = configuration.GetSection(TelemetryJsonPaths.ExportersSection).Get<ExportersOptions>() ?? new ExportersOptions(),
            Tracer = configuration.GetSection(TelemetryJsonPaths.TracerSection).Get<TracerOptions>() ?? new TracerOptions(),
        };

        return telemetryOptions;
    }

    /// <summary>
    /// Reads the configuration from the default telemetry.json file and provided TelemetryConfig.
    /// </summary>
    /// <param name="telemetryConfig">The telemetry configuration to read from.</param>
    /// <returns>The telemetry configuration object.</returns>
    private static IConfiguration BuildConfiguration(TelemetryConfig telemetryConfig)
    {
        // Create a configuration builder that reads the structure and defaults from the static telemetry.json
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(TelemetryConstants.TelemetryJson);
        if (stream == null)
        {
            var error = string.Format(
                CultureInfo.InvariantCulture,
                TelemetryConstants.TelemetryJsonNotFoundErrorTemplate,
                TelemetryConstants.TelemetryJson,
                assembly.GetManifestResourceNames()[0]);
            Console.WriteLine(error);
            throw new TelemetryException(error);
        }
        var builder = new ConfigurationBuilder().AddJsonStream(stream!);

        // Add resource and exporters configuration
        var configurationOverrides = new Dictionary<string, string?>
        {
            { TelemetryJsonPaths.ResourceComponent, telemetryConfig.ResourceComponent },
            { TelemetryJsonPaths.ResourceVersion, telemetryConfig.ResourceVersion },
            { TelemetryJsonPaths.ResourceWebsiteName, telemetryConfig.ResourceWebsiteName },
            { TelemetryJsonPaths.ResourceWebsiteInstance, telemetryConfig.ResourceWebsiteInstance },
            { TelemetryJsonPaths.ResourceEnvironment, telemetryConfig.ResourceEnvironment },
            { TelemetryJsonPaths.ResourceRegion, telemetryConfig.ResourceRegion },
            { TelemetryJsonPaths.EnableConsoleExporter, telemetryConfig.EnableConsoleExporter.ToString() },
            { TelemetryJsonPaths.EnableAppInsightsExporter, telemetryConfig.EnableAppInsightsExporter.ToString() },
            { TelemetryJsonPaths.AppInsightsConnectionString, telemetryConfig.AppInsightsConnectionString },
            { TelemetryJsonPaths.SerilogMinimumLevelDefault, telemetryConfig.LogLevel.ToString() },
        };

        // Incrementing sink index to support multiple sinks
        var sinkIdx = 0;

        // Add Serilog configuration for console exporter
        if (telemetryConfig.EnableConsoleExporter)
        {
            configurationOverrides.Add(
                $"{TelemetryJsonPaths.SerilogUsing}:{sinkIdx}",
                ConfigConstants.SerilogUsingConsoleSink);

            configurationOverrides.Add(
                $"{TelemetryJsonPaths.SerilogWriteTo}:{sinkIdx}:{TelemetryJsonPaths.SerilogWriteToName}",
                ConfigConstants.SerilogConsoleSinkName);

            configurationOverrides.Add(
                $"{TelemetryJsonPaths.SerilogWriteTo}:{sinkIdx}:{TelemetryJsonPaths.SerilogWriteToArgsFormatter}",
                ConfigConstants.SerilogConsoleSinkFormatter);

            sinkIdx++;
        }

        // Add Serilog configuration for AppInsights exporter
        if (telemetryConfig.EnableAppInsightsExporter)
        {
            configurationOverrides.Add(
                $"{TelemetryJsonPaths.SerilogUsing}:{sinkIdx}",
                ConfigConstants.SerilogUsingAppInsightsSink);

            configurationOverrides.Add(
                $"{TelemetryJsonPaths.SerilogWriteTo}:{sinkIdx}:{TelemetryJsonPaths.SerilogWriteToName}",
                ConfigConstants.SerilogAppInsightsSinkName);

            configurationOverrides.Add(
                $"{TelemetryJsonPaths.SerilogWriteTo}:{sinkIdx}:{TelemetryJsonPaths.SerilogWriteToArgsConnectionString}",
                telemetryConfig.AppInsightsConnectionString);

            configurationOverrides.Add(
                $"{TelemetryJsonPaths.SerilogWriteTo}:{sinkIdx}:{TelemetryJsonPaths.SerilogWriteToArgsTelemetryConverter}",
                ConfigConstants.SerilogAppInsightsSinkTelemetryConverter);

            sinkIdx++;
        }

        // If no exporters are configured, throw an exception
        if (sinkIdx == 0)
        {
            throw new TelemetryException(TelemetryConstants.NoTelemetryExportersConfiguredError);
        }

        // Override the default "empty" configuration with provided values
        builder.AddInMemoryCollection(configurationOverrides);
        return builder.Build();
    }

    /// <summary>
    /// Configures telemetry for the application using the provided options.
    /// </summary>
    private static void ConfigureTraceAndMetrics(
        IServiceCollection services,
        TelemetryOptions telemetryOptions,
        Action<TracerProviderBuilder>? configureTracing,
        Action<MeterProviderBuilder>? configureMetrics)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(telemetryOptions);

        services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder =>
                resourceBuilder.AddService(
                serviceName: telemetryOptions.Resource.Component,
                serviceVersion: telemetryOptions.Resource.Version))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddHttpClientInstrumentation();
                if (string.Equals(TelemetryConstants.WebApp, telemetryOptions.Resource.HostType, StringComparison.Ordinal))
                {
                    tracerProviderBuilder.AddAspNetCoreInstrumentation();
                }
                tracerProviderBuilder.SetSampler(
                    telemetryOptions.Tracer.SampleRate > 0
                        ? new TraceIdRatioBasedSampler(telemetryOptions.Tracer.SampleRate)
                        : new AlwaysOnSampler());

                ConfigureOpenTelemetryTraceExporter(tracerProviderBuilder, telemetryOptions);

                // Apply additional user-provided tracing config
                configureTracing?.Invoke(tracerProviderBuilder);
            })
            .WithMetrics(meterProviderBuilder =>
            {
                // Runtime metrics and http client metrics can be added if later deemed necessary
                meterProviderBuilder
                    .AddMeter(telemetryOptions.Resource.Component)
                    .SetExemplarFilter(ExemplarFilterType.TraceBased);

                ConfigureOpenTelemetryMetricsExporter(meterProviderBuilder, telemetryOptions);

                // Apply additional user-provided metrics config
                configureMetrics?.Invoke(meterProviderBuilder);
            });
    }

    /// <summary>
    /// Configures OpenTelemetry tracing based on the provided telemetry options.
    /// </summary>
    /// <param name="builder">The tracer provider builder to configure exporter.</param>
    /// <param name="telemetryOptions">The telemetry options containing exporter configurations.</param>
    private static void ConfigureOpenTelemetryTraceExporter(TracerProviderBuilder builder, TelemetryOptions telemetryOptions)
    {
        if (telemetryOptions.Exporters.Console != null && telemetryOptions.Exporters.Console.Enabled)
        {
            builder.AddConsoleExporter();
        }

        if (telemetryOptions.Exporters.AppInsights != null && telemetryOptions.Exporters.AppInsights.Enabled)
        {
            builder.AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = telemetryOptions.Exporters.AppInsights.ConnectionString;
            });
        }
    }

    /// <summary>
    /// Configures OpenTelemetry metrics based on the provided telemetry options.
    /// </summary>
    /// <param name="builder">The meter provider builder for setting up exporter.</param>
    /// <param name="telemetryOptions">The telemetry options containing exporter configurations.</param>
    private static void ConfigureOpenTelemetryMetricsExporter(MeterProviderBuilder builder, TelemetryOptions telemetryOptions)
    {
        if (telemetryOptions.Exporters.Console != null && telemetryOptions.Exporters.Console.Enabled)
        {
            builder.AddConsoleExporter();
        }

        if (telemetryOptions.Exporters.AppInsights != null && telemetryOptions.Exporters.AppInsights.Enabled)
        {
            builder.AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = telemetryOptions.Exporters.AppInsights.ConnectionString;
            });
        }
    }
}
