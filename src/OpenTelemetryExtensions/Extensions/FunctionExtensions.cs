using System;
using System.Diagnostics;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;

namespace OpenTelemetryExtensions.Extensions
{
    public static class FunctionExtensions
    {
        private const string DefaultActivitySourceName = "FunctionApp.TimerTriggers";
        
        public static IFunctionsHostBuilder AddTelemetry(this IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();
            
            services.AddTelemetry(configuration);
            
            services.AddSingleton<ActivitySource>(new ActivitySource(DefaultActivitySourceName));
            
            return builder;
        }
        
        public static IFunctionsHostBuilder AddTelemetry(this IFunctionsHostBuilder builder, Action<OpenTelemetryBuilder> configureOptions)
        {
            var services = builder.Services;
            
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();
            
            services.AddTelemetry(configuration);
            
            services.AddOpenTelemetry(configureOptions);
            
            services.AddSingleton<ActivitySource>(new ActivitySource(DefaultActivitySourceName));
            
            return builder;
        }
        
        public static IFunctionsHostBuilder ConfigureTimerTriggerActivity(this IFunctionsHostBuilder builder, string activitySourceName = DefaultActivitySourceName)
        {
            var services = builder.Services;
            
            services.AddSingleton<ActivitySource>(new ActivitySource(activitySourceName));
            
            return builder;
        }
        
        public static ActivitySource GetTimerTriggerActivitySource(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<ActivitySource>() ?? 
                   new ActivitySource(DefaultActivitySourceName);
        }
    }
}
