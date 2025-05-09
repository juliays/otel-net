using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTelemetryExtensions.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder)
        {
            builder.Services.AddTelemetry(builder.Configuration);
            return builder;
        }
        
        public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder, Action<OpenTelemetryBuilder> configureOptions)
        {
            builder.Services.AddTelemetry(builder.Configuration);
            
            builder.Services.AddOpenTelemetry(configureOptions);
            
            return builder;
        }
    }
}
