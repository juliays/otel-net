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
using Serilog;

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
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration.GetSection($"{TelemetryConfig.SectionName}:Serilog"))
                .Enrich.WithProperty("environment", config.Resource.Environment)
                .Enrich.WithProperty("workspace_id", config.Resource.WorkspaceId)
                .Enrich.WithProperty("notebook_id", config.Resource.NotebookId)
                .Enrich.WithProperty("livy_id", config.Resource.LivyId)
                .Enrich.WithProperty("region", config.Resource.Region)
                .Enrich.WithProperty("website_name", config.Resource.WebsiteName)
                .Enrich.WithProperty("website_instance", config.Resource.WebsiteInstance)
                .Enrich.WithProperty("mnd-applicationid", config.Resource.ApplicationId)
                .Enrich.WithProperty("cloud_provider", config.Resource.CloudProvider)
                .Enrich.WithProperty("opt-dora", config.Resource.OptDora)
                .Enrich.WithProperty("opt-service-id", config.Resource.OptServiceId)
                .Enrich.WithProperty("service.name", config.Resource.Component);
            
            Log.Logger = loggerConfiguration.CreateLogger();
            
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
            
            return services;
        }
        
        private static IServiceCollection AddOpenTelemetry(this IServiceCollection services, TelemetryConfig config)
        {
            if (config.Exporters.AppInsights.Enabled)
            {
                services.AddOpenTelemetry().UseAzureMonitor(options =>
                {
                    options.ConnectionString = config.Exporters.AppInsights.ConnectionString;
                });
                
                return services;
            }
            
            var builder = services.AddOpenTelemetry();
            
            builder.ConfigureResource(resourceBuilder => ConfigureResource(resourceBuilder, config.Resource));
            
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
            if (config.Console.Enabled)
            {
                builder.AddConsoleExporter();
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
                builder.AddConsoleExporter();
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
