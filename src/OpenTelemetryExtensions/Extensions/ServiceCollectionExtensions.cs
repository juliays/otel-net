using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetryExtensions.Configuration;
using OpenTelemetryExtensions.Enrichers;
using Serilog;
using Serilog.Context;

namespace OpenTelemetryExtensions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var telemetryConfig = new TelemetryConfig();
            configuration.GetSection(TelemetryConfig.SectionName).Bind(telemetryConfig);
            
            services.Configure<TelemetryConfig>(configuration.GetSection(TelemetryConfig.SectionName));
            services.AddSingleton(telemetryConfig);
            
            services.AddSerilog(telemetryConfig, configuration);
            
            services.AddOpenTelemetry(telemetryConfig);
            
            return services;
        }
        
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, Action<OpenTelemetryBuilder> configureOptions)
        {
            var builder = services.AddOpenTelemetry();
            configureOptions?.Invoke(builder);
            return services;
        }
        
        private static IServiceCollection AddSerilog(this IServiceCollection services, TelemetryConfig config, IConfiguration configuration)
        {
            var resourceAttributes = new Dictionary<string, object>
            {
                ["environment"] = config.Resource.Environment,
                ["component"] = config.Resource.Component,
                ["workspace_id"] = config.Resource.WorkspaceId,
                ["notebook_id"] = config.Resource.NotebookId,
                ["livy_id"] = config.Resource.LivyId,
                ["region"] = config.Resource.Region,
                ["website_name"] = config.Resource.WebsiteName,
                ["website_instance"] = config.Resource.WebsiteInstance,
                ["mnd-applicationid"] = config.Resource.ApplicationId,
                ["cloud_provider"] = config.Resource.CloudProvider,
                ["opt-dora"] = config.Resource.OptDora,
                ["opt-service-id"] = config.Resource.OptServiceId,
                ["service.name"] = config.Resource.Component
            };
            
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration);
            
            loggerConfiguration.WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter());
            
            loggerConfiguration.Enrich.With<TraceContextEnricher>();
            
            foreach (var attribute in resourceAttributes)
            {
                if (!string.IsNullOrEmpty(attribute.Value?.ToString()))
                {
                    loggerConfiguration.Enrich.WithProperty(attribute.Key, attribute.Value);
                }
            }
            
            Log.Logger = loggerConfiguration.CreateLogger();
            
            services.AddLogging(builder => 
            {
                builder.ClearProviders();
                
                builder.AddSerilog(Log.Logger, dispose: true);
                
                foreach (var attribute in resourceAttributes)
                {
                    if (!string.IsNullOrEmpty(attribute.Value?.ToString()))
                    {
                        LogContext.PushProperty(attribute.Key, attribute.Value);
                    }
                }
            });
            
            services.AddSingleton(Log.Logger);
            
            return services;
        }
        
        private static IServiceCollection AddOpenTelemetry(this IServiceCollection services, TelemetryConfig config)
        {
            var builder = services.AddOpenTelemetry();
            
            builder.ConfigureResource(resourceBuilder => ConfigureResource(resourceBuilder, config.Resource));
            
            if (config.Exporters.AppInsights.Enabled)
            {
                var connectionString = config.Exporters.AppInsights.ConnectionString;
                if (!string.IsNullOrEmpty(connectionString) && 
                    !connectionString.Contains("YOUR_") && 
                    !connectionString.Equals("your-connection-string", StringComparison.OrdinalIgnoreCase))
                {
                    builder.UseAzureMonitor(options =>
                    {
                        options.ConnectionString = connectionString;
                    });
                }
                else
                {
                    Console.WriteLine("WARNING: App Insights is enabled but connection string appears to be a placeholder.");
                    Console.WriteLine("Skipping App Insights configuration to avoid runtime errors.");
                    Console.WriteLine("Set a valid connection string in the configuration to enable App Insights telemetry.");
                }
            }
            
            builder.WithTracing(tracerProviderBuilder => ConfigureTracing(tracerProviderBuilder, config));
            builder.WithMetrics(meterProviderBuilder => ConfigureMetrics(meterProviderBuilder, config));
            
            return services;
        }
        
        private static void ConfigureResource(ResourceBuilder resourceBuilder, ResourceConfig config)
        {
            resourceBuilder.AddService(config.Component)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment"] = config.Environment,
                    ["workspace_id"] = config.WorkspaceId,
                    ["notebook_id"] = config.NotebookId,
                    ["livy_id"] = config.LivyId,
                    ["region"] = config.Region,
                    ["website_name"] = config.WebsiteName,
                    ["website_instance"] = config.WebsiteInstance,
                    ["mnd-applicationid"] = config.ApplicationId,
                    ["cloud_provider"] = config.CloudProvider,
                    ["opt-dora"] = config.OptDora,
                    ["opt-service-id"] = config.OptServiceId
                });
        }
        
        private static void ConfigureTracing(TracerProviderBuilder builder, TelemetryConfig config)
        {
            builder
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .SetSampler(new TraceIdRatioBasedSampler(config.Tracer.SampleRate));
            
            ConfigureTracingExporters(builder, config.Exporters);
        }
        
        private static void ConfigureTracingExporters(TracerProviderBuilder builder, ExporterConfig config)
        {
            builder.AddConsoleExporter(options => 
            {
                options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Console;
            });
            
            if (config.Datadog.Enabled)
            {
                builder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(config.Datadog.Endpoint);
                    options.Headers = $"DD-API-KEY={config.Datadog.ApiKey}";
                });
            }
        }
        
        private static void ConfigureMetrics(MeterProviderBuilder builder, TelemetryConfig config)
        {
            builder
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation();
            
            ConfigureMetricsExporters(builder, config.Exporters);
        }
        
        private static void ConfigureMetricsExporters(MeterProviderBuilder builder, ExporterConfig config)
        {
            builder.AddConsoleExporter(options => 
            {
                options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Console;
            });
            
            if (config.Datadog.Enabled)
            {
                builder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(config.Datadog.Endpoint);
                    options.Headers = $"DD-API-KEY={config.Datadog.ApiKey}";
                });
            }
        }
    }
}
