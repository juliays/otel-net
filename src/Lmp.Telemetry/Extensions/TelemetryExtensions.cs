using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Lmp.Telemetry.Configuration;
using Lmp.Telemetry.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
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
            services.AddTelemetry(configuration);
            
            if (configureTracing != null || configureMetrics != null)
            {
                services.AddOpenTelemetry(builder =>
                {
                    if (configureTracing != null)
                    {
                        builder.WithTracing(tracerProviderBuilder => 
                        {
                            configureTracing(tracerProviderBuilder);
                        });
                    }
                    
                    if (configureMetrics != null)
                    {
                        builder.WithMetrics(meterProviderBuilder => 
                        {
                            configureMetrics(meterProviderBuilder);
                        });
                    }
                });
            }
        });

        return hostBuilder;
    }
    
    public static WebApplicationBuilder AddOpenTelemetry(
        this WebApplicationBuilder builder,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration)
                        .Enrich.With<TraceContextEnricher>();
        });
        
        builder.Services.AddTelemetry(builder.Configuration);
        
        if (configureTracing != null || configureMetrics != null)
        {
            builder.Services.AddOpenTelemetry(otBuilder =>
            {
                if (configureTracing != null)
                {
                    otBuilder.WithTracing(tracerProviderBuilder => 
                    {
                        configureTracing(tracerProviderBuilder);
                    });
                }
                
                if (configureMetrics != null)
                {
                    otBuilder.WithMetrics(meterProviderBuilder => 
                    {
                        configureMetrics(meterProviderBuilder);
                    });
                }
            });
        }
        
        return builder;
    }
    
    public static IFunctionsHostBuilder AddOpenTelemetry(
        this IFunctionsHostBuilder builder,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>? configureMetrics = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        var services = builder.Services;
        
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>() ?? 
            throw new InvalidOperationException("Configuration not found in service provider");
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.With<TraceContextEnricher>()
            .CreateLogger();
            
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });
        
        services.AddTelemetry(configuration);
        
        if (configureTracing != null || configureMetrics != null)
        {
            services.AddOpenTelemetry(otBuilder =>
            {
                if (configureTracing != null)
                {
                    otBuilder.WithTracing(tracerProviderBuilder => 
                    {
                        configureTracing(tracerProviderBuilder);
                    });
                }
                
                if (configureMetrics != null)
                {
                    otBuilder.WithMetrics(meterProviderBuilder => 
                    {
                        configureMetrics(meterProviderBuilder);
                    });
                }
            });
        }
        
        return builder;
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
