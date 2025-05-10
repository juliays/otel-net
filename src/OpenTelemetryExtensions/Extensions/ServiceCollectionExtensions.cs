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
using Serilog.Enrichers.Span;

namespace OpenTelemetryExtensions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var telemetryConfig = new TelemetryConfig();
            configuration.GetSection(TelemetryConfig.SectionName).Bind(telemetryConfig);
            
            services.Configure<TelemetryConfig>(configuration.GetSection(TelemetryConfig.SectionName));
            
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
                .ReadFrom.Configuration(configuration.GetSection($"{TelemetryConfig.SectionName}:Serilog"));
            
            // This will override any console sink from configuration
            loggerConfiguration.WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter());
            
            loggerConfiguration
                .Enrich.WithSpan()
                .Enrich.With<TraceContextEnricher>();
            
            foreach (var attribute in resourceAttributes)
            {
                loggerConfiguration.Enrich.WithProperty(attribute.Key, attribute.Value);
            }
            
            Log.Logger = loggerConfiguration.CreateLogger();
            
            services.AddLogging(builder => 
            {
                // Clear existing providers to ensure Serilog is the only one
                builder.ClearProviders();
                
                builder.AddSerilog(Log.Logger, dispose: true);
                
                foreach (var attribute in resourceAttributes)
                {
                    Serilog.Context.LogContext.PushProperty(attribute.Key, attribute.Value);
                }
            });
            
            services.AddSingleton(Log.Logger);
            
            return services;
        }
        
        private static IServiceCollection AddOpenTelemetry(this IServiceCollection services, TelemetryConfig config)
        {
            var builder = services.AddOpenTelemetry();
            
            builder.ConfigureResource(resourceBuilder => ConfigureResource(resourceBuilder, config.Resource));
            
            builder.WithTracing(tracerProviderBuilder => ConfigureTracing(tracerProviderBuilder, config));
            
            builder.WithMetrics(meterProviderBuilder => ConfigureMetrics(meterProviderBuilder, config));
            
            if (config.Exporters.AppInsights.Enabled)
            {
                builder.UseAzureMonitor(options =>
                {
                    options.ConnectionString = config.Exporters.AppInsights.ConnectionString;
                });
            }
            
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
            if (config.Console.Enabled)
            {
                builder.AddConsoleExporter(options => 
                {
                    options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Debug;
                });
            }
            
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
            if (config.Console.Enabled)
            {
                builder.AddConsoleExporter(options => 
                {
                    options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Debug;
                });
            }
            
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
