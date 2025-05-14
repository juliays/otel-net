using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Lmp.Telemetry.Configuration;
using Lmp.Telemetry.Constants;
using Serilog;

namespace Lmp.Telemetry.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var telemetryOptions = TelemetryExtensions.BindTelemetryOptions(configuration);
            
            ConfigureSerilog(services, configuration);
            
            services.AddOpenTelemetry()
                .ConfigureResource(builder =>
                {
                    builder.AddService(telemetryOptions.Resource.Component);
                })
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddSource(telemetryOptions.Resource.Component)
                        .AddHttpClientInstrumentation();

                    if (IsWebContext())
                    {
                        tracerProviderBuilder.AddAspNetCoreInstrumentation();
                    }

                    if (telemetryOptions.Tracer.SampleRate > 0)
                    {
                        tracerProviderBuilder.SetSampler(new TraceIdRatioBasedSampler(telemetryOptions.Tracer.SampleRate));
                    }
                    else
                    {
                        tracerProviderBuilder.SetSampler(new AlwaysOnSampler());
                    }

                    TelemetryExtensions.ConfigureOpenTelemetryTraceExporter(tracerProviderBuilder, telemetryOptions);
                })
                .WithMetrics(meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .AddMeter(telemetryOptions.Resource.Component)
                        .SetExemplarFilter(ExemplarFilterType.TraceBased)
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation();

                    if (IsWebContext())
                    {
                        meterProviderBuilder.AddAspNetCoreInstrumentation();
                    }

                    TelemetryExtensions.ConfigureOpenTelemetryMetricsExporter(meterProviderBuilder, telemetryOptions);
                });
            
            return services;
        }
        
        private static bool IsWebContext()
        {
            try
            {
                var type = Type.GetType("Microsoft.AspNetCore.Http.HttpContext, Microsoft.AspNetCore.Http.Abstractions");
                return type != null;
            }
            catch
            {
                return false;
            }
        }
        
        private static void ConfigureSerilog(IServiceCollection services, IConfiguration configuration)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration);
                
            loggerConfiguration.Enrich.With<TraceContextEnricher>();
            
            Log.Logger = loggerConfiguration.CreateLogger();
            
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
        }
    }
}
