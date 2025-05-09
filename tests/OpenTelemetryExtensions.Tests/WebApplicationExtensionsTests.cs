using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetry;
using OpenTelemetryExtensions.Extensions;
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
                    { "telemetry:resource:environment", "test" }
                })
                .Build();
            
            var builder = WebApplication.CreateBuilder(new string[] { });
            
            builder.Configuration.AddConfiguration(memoryConfig);
            
            var result = builder.AddTelemetry();
            
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void AddTelemetry_WithConfigureOptions_RegistersServices()
        {
            var memoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test" }
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
        }
    }
}
