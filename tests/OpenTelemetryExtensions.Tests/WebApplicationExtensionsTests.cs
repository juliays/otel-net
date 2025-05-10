using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetry;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Collections.Generic;

namespace OpenTelemetryExtensions.Tests
{
    [TestClass]
    public class WebApplicationExtensionsTests
    {
        [TestMethod]
        public void AddTelemetry_RegistersServices()
        {
            var configurationMock = new Mock<IConfiguration>();
            var servicesMock = new Mock<IServiceCollection>();
            
            var memoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test" },
                    { "telemetry:resource:component", "test-component" },
                    
                    { "telemetry:Serilog:MinimumLevel:Default", "Information" },
                    { "telemetry:Serilog:WriteTo:0:Name", "Console" },
                    
                    { "telemetry:exporters:console:enabled", "true" },
                    
                    { "telemetry:tracer:sampleRate", "1.0" }
                })
                .Build();
            
            var builder = WebApplication.CreateBuilder(new string[] { });
            
            builder.Configuration.AddConfiguration(memoryConfig);
            
            var result = builder.AddTelemetry();
            
            Assert.IsNotNull(result);
            
            Assert.IsTrue(builder.Services.Any(s => s.ServiceType.Namespace?.StartsWith("OpenTelemetry") == true));
        }
        
        [TestMethod]
        public void AddTelemetry_WithConfigureOptions_RegistersServices()
        {
            var memoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test" },
                    { "telemetry:resource:component", "test-component" },
                    
                    { "telemetry:Serilog:MinimumLevel:Default", "Information" },
                    { "telemetry:Serilog:WriteTo:0:Name", "Console" },
                    
                    { "telemetry:exporters:console:enabled", "true" },
                    
                    { "telemetry:tracer:sampleRate", "1.0" }
                })
                .Build();
            
            var builder = WebApplication.CreateBuilder(new string[] { });
            
            builder.Configuration.AddConfiguration(memoryConfig);
            
            var result = builder.AddTelemetry(options => {
                options.WithTracing(tracerProviderBuilder => {
                    tracerProviderBuilder.AddSource("TestSource");
                });
            });
            
            Assert.IsNotNull(result);
            
            Assert.IsTrue(builder.Services.Any(s => s.ServiceType.Namespace?.StartsWith("OpenTelemetry") == true));
        }
    }
}
