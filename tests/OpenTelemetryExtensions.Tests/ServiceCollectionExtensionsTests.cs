using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetry;
using OpenTelemetryExtensions.Configuration;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Collections.Generic;

namespace OpenTelemetryExtensions.Tests
{
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddTelemetry_RegistersServices()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test" },
                    { "telemetry:resource:component", "test-component" },
                    { "telemetry:exporters:console:enabled", "true" },
                    { "telemetry:tracer:sampleRate", "1.0" },
                    { "telemetry:Serilog:MinimumLevel:Default", "Information" },
                    { "telemetry:Serilog:WriteTo:0:Name", "Console" }
                })
                .Build();
            
            services.AddLogging();
            
            services.AddTelemetry(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
        
        [TestMethod]
        public void AddTelemetry_WithAppInsightsEnabled_ConfiguresAzureMonitor()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test" },
                    { "telemetry:resource:component", "test-component" },
                    
                    { "telemetry:Serilog:MinimumLevel:Default", "Information" },
                    { "telemetry:Serilog:WriteTo:0:Name", "Console" },
                    
                    // AppInsights exporter configuration
                    { "telemetry:exporters:appInsights:enabled", "true" },
                    { "telemetry:exporters:appInsights:connectionString", "test-connection-string" },
                    
                    { "telemetry:tracer:sampleRate", "1.0" }
                })
                .Build();
            
            services.AddLogging();
            
            services.AddTelemetry(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
        
        [TestMethod]
        public void AddTelemetry_WithDatadogEnabled_ConfiguresOtlpExporter()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    // Resource configuration
                    { "telemetry:resource:environment", "test" },
                    { "telemetry:resource:component", "test-component" },
                    
                    // Serilog configuration
                    { "telemetry:Serilog:MinimumLevel:Default", "Information" },
                    { "telemetry:Serilog:WriteTo:0:Name", "Console" },
                    
                    // Datadog exporter configuration
                    { "telemetry:exporters:datadog:enabled", "true" },
                    { "telemetry:exporters:datadog:endpoint", "https://api.datadoghq.com" },
                    { "telemetry:exporters:datadog:apiKey", "test-api-key" },
                    
                    // Tracer configuration
                    { "telemetry:tracer:sampleRate", "1.0" }
                })
                .Build();
            
            services.AddLogging();
            
            services.AddTelemetry(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
    }
}
