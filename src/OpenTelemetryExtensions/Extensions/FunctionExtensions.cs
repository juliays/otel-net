using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;

namespace OpenTelemetryExtensions.Extensions
{
    public static class FunctionExtensions
    {
        public static IFunctionsHostBuilder AddTelemetry(this IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            
            services.AddTelemetry(configuration);
            return builder;
        }
        
        public static IFunctionsHostBuilder AddTelemetry(this IFunctionsHostBuilder builder, Action<OpenTelemetryBuilder> configureOptions)
        {
            var services = builder.Services;
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            
            services.AddTelemetry(configuration);
            
            services.AddOpenTelemetry(configureOptions);
            
            return builder;
        }
    }
}
