using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using System;

namespace OpenTelemetryExtensions.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder)
        {
            builder.Services.AddTelemetry(builder.Configuration);
            return builder;
        }
        
        public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddTelemetry(configuration ?? builder.Configuration);
            return builder;
        }
        
        public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder, Action<OpenTelemetryBuilder> configureOptions)
        {
            builder.Services.AddTelemetry(builder.Configuration);
            
            builder.Services.AddOpenTelemetry(configureOptions);
            
            return builder;
        }
        
        public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder, IConfiguration configuration, Action<OpenTelemetryBuilder> configureOptions)
        {
            builder.Services.AddTelemetry(configuration ?? builder.Configuration);
            
            builder.Services.AddOpenTelemetry(configureOptions);
            
            return builder;
        }
    }
}
