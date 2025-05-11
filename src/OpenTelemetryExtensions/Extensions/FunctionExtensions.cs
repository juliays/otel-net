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
            
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();
            
            services.AddTelemetry(configuration);
            return builder;
        }
        
        public static IFunctionsHostBuilder AddTelemetry(this IFunctionsHostBuilder builder, IConfiguration configuration)
        {
            var services = builder.Services;
            
            if (configuration == null)
            {
                var serviceProvider = services.BuildServiceProvider();
                configuration = serviceProvider.GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();
            }
            
            services.AddTelemetry(configuration);
            return builder;
        }
        
        public static IFunctionsHostBuilder AddTelemetry(this IFunctionsHostBuilder builder, Action<OpenTelemetryBuilder> configureOptions)
        {
            var services = builder.Services;
            
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();
            
            services.AddTelemetry(configuration);
            
            services.AddOpenTelemetry(configureOptions);
            
            return builder;
        }
        
        public static IFunctionsHostBuilder AddTelemetry(this IFunctionsHostBuilder builder, IConfiguration configuration, Action<OpenTelemetryBuilder> configureOptions)
        {
            var services = builder.Services;
            
            if (configuration == null)
            {
                var serviceProvider = services.BuildServiceProvider();
                configuration = serviceProvider.GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();
            }
            
            services.AddTelemetry(configuration);
            
            services.AddOpenTelemetry(configureOptions);
            
            return builder;
        }
    }
}
