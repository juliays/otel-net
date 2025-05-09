using Microsoft.Azure.Functions.Extensions.DependencyInjection;
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
    public class FunctionExtensionsTests
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
                    { "telemetry:tracer:sampleRate", "1.0" }
                })
                .Build();
            
            services.AddSingleton<IConfiguration>(configuration);
            
            var functionsHostBuilderMock = new Mock<IFunctionsHostBuilder>();
            functionsHostBuilderMock.SetupGet(x => x.Services).Returns(services);
            
            var result = functionsHostBuilderMock.Object.AddTelemetry();
            
            Assert.IsNotNull(result);
            
            Assert.IsTrue(services.Any(s => s.ServiceType.Namespace?.StartsWith("OpenTelemetry") == true));
        }
        
        [TestMethod]
        public void AddTelemetry_WithConfigureOptions_RegistersServices()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test" },
                    { "telemetry:resource:component", "test-component" },
                    { "telemetry:exporters:console:enabled", "true" },
                    { "telemetry:tracer:sampleRate", "1.0" }
                })
                .Build();
            
            services.AddSingleton<IConfiguration>(configuration);
            
            var functionsHostBuilderMock = new Mock<IFunctionsHostBuilder>();
            functionsHostBuilderMock.SetupGet(x => x.Services).Returns(services);
            
            var result = functionsHostBuilderMock.Object.AddTelemetry(options => {
                options.WithTracing(tracerProviderBuilder => {
                    tracerProviderBuilder.AddSource("TestSource");
                });
            });
            
            Assert.IsNotNull(result);
            
            Assert.IsTrue(services.Any(s => s.ServiceType.Namespace?.StartsWith("OpenTelemetry") == true));
        }
    }
}
