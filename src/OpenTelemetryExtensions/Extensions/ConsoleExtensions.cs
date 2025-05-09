using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

namespace OpenTelemetryExtensions.Extensions
{
    public static class ConsoleExtensions
    {
        public static IHostBuilder AddTelemetry(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddTelemetry(context.Configuration);
            });
            
            return builder;
        }
        
        public static IHostBuilder AddTelemetry(this IHostBuilder builder, Action<OpenTelemetryBuilder> configureOptions)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddTelemetry(context.Configuration);
                
                services.AddOpenTelemetry(configureOptions);
            });
            
            return builder;
        }
    }
}
