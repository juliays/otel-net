using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetry;
using OpenTelemetryExtensions.Extensions;
using System;

namespace OpenTelemetryExtensions.Tests
{
    [TestClass]
    public class ConsoleExtensionsTests
    {
        [TestMethod]
        public void AddTelemetry_RegistersServices()
        {
            var hostBuilder = Host.CreateDefaultBuilder(new string[] { });
            
            var result = hostBuilder.AddTelemetry();
            
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void AddTelemetry_WithConfigureOptions_RegistersServices()
        {
            var hostBuilder = Host.CreateDefaultBuilder(new string[] { });
            
            var result = hostBuilder.AddTelemetry(options => {
                options.WithTracing(tracerProviderBuilder => {
                    tracerProviderBuilder.AddSource("TestSource");
                });
            });
            
            Assert.IsNotNull(result);
        }
    }
}
