using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Lmp.Telemetry.Configuration;
using Lmp.Telemetry.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Lmp.Telemetry.Extensions;

public static class TelemetryExtensions
{
    public static IHostBuilder AddOpenTelemetry(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        ArgumentNullException.ThrowIfNull(configuration);

        var telemetryOptions = BindTelemetryOptions(configuration);

        hostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration)
                            .Enrich.With<TraceContextEnricher>();
        });

        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddOpenTelemetry()
                .ConfigureResource(builder =>
                {
                    builder.AddService(telemetryOptions.Resource.Component);
                })
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddSource(telemetryOptions.Resource.Component)
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation();

                    if (telemetryOptions.Tracer.SampleRate > 0)
                    {
                        tracerProviderBuilder.SetSampler(new TraceIdRatioBasedSampler(telemetryOptions.Tracer.SampleRate));
                    }
                    else
                    {
                        tracerProviderBuilder.SetSampler(new AlwaysOnSampler());
                    }

                    ConfigureOpenTelemetryTraceExporter(tracerProviderBuilder, telemetryOptions);

                    configureTracing?.Invoke(tracerProviderBuilder);
                })
                .WithMetrics(meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .AddMeter(telemetryOptions.Resource.Component)
                        .SetExemplarFilter(ExemplarFilterType.TraceBased)
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation();

                    ConfigureOpenTelemetryMetricsExporter(meterProviderBuilder, telemetryOptions);

                    configureMetrics?.Invoke(meterProviderBuilder);
                });
        });

        return hostBuilder;
    }

    public static void ConfigureOpenTelemetryTraceExporter(TracerProviderBuilder builder, TelemetryOptions telemetryOptions)
    {
        if (telemetryOptions.Exporters.Console?.Enabled == true)
        {
            builder.AddConsoleExporter();
        }

        if (telemetryOptions.Exporters.AppInsights?.Enabled == true)
        {
            builder.AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = telemetryOptions.Exporters.AppInsights.ConnectionString;
            });
        }
        else if (telemetryOptions.Exporters.Datadog?.Enabled == true)
        {
        }
    }

    public static void ConfigureOpenTelemetryMetricsExporter(MeterProviderBuilder builder, TelemetryOptions telemetryOptions)
    {
        if (telemetryOptions.Exporters.Console?.Enabled == true)
        {
            builder.AddConsoleExporter();
        }

        if (telemetryOptions.Exporters.AppInsights?.Enabled == true)
        {
            builder.AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = telemetryOptions.Exporters.AppInsights.ConnectionString;
            });
        }
        else if (telemetryOptions.Exporters.Datadog?.Enabled == true)
        {
        }
    }

    public static TelemetryOptions BindTelemetryOptions(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configuration.GetSection(TelemetryConstants.TelemetryResource));
        ArgumentNullException.ThrowIfNull(configuration.GetSection(TelemetryConstants.TelemetryExporter));
        ArgumentNullException.ThrowIfNull(configuration.GetSection(TelemetryConstants.TelemetryTracer));

        var telemetryOptions = new TelemetryOptions
        {
            Resource = configuration.GetSection(TelemetryConstants.TelemetryResource).BindWithDisplayName<ResourceOptions>(),
            Exporters = configuration.GetSection(TelemetryConstants.TelemetryExporter).Get<ExportersOptions>() ?? new ExportersOptions(),
            Tracer = configuration.GetSection(TelemetryConstants.TelemetryTracer).Get<TracerOptions>() ?? new TracerOptions(),
        };

        return telemetryOptions;
    }

    public static KeyValuePair<string, object>[] CreateAttributesFromResource(ResourceOptions resourceOptions)
    {
        ArgumentNullException.ThrowIfNull(resourceOptions);
        var keyValuePairs = new List<KeyValuePair<string, object>>();

        foreach (var property in resourceOptions.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(resourceOptions);
            if (value != null)
            {
                keyValuePairs.Add(new KeyValuePair<string, object>(property.Name, value?.ToString() ?? string.Empty));
            }
        }
        return keyValuePairs.ToArray();
    }
}
