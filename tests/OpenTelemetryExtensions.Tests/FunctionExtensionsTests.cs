using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetry;
using OpenTelemetryExtensions.Extensions;
using System;

namespace OpenTelemetryExtensions.Tests
{
    [TestClass]
    public class FunctionExtensionsTests
    {
        [TestMethod]
        public void AddTelemetry_RegistersServices()
        {
            var services = new ServiceCollection();
            
            var configurationMock = new Mock<IConfiguration>();
            services.AddSingleton<IConfiguration>(configurationMock.Object);
            
            var functionsHostBuilderMock = new Mock<IFunctionsHostBuilder>();
            functionsHostBuilderMock.SetupGet(x => x.Services).Returns(services);
            
            var result = functionsHostBuilderMock.Object.AddTelemetry();
            
            Assert.IsNotNull(result);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
        
        [TestMethod]
        public void AddTelemetry_WithConfigureOptions_RegistersServices()
        {
            var services = new ServiceCollection();
            
            var configurationMock = new Mock<IConfiguration>();
            services.AddSingleton<IConfiguration>(configurationMock.Object);
            
            var functionsHostBuilderMock = new Mock<IFunctionsHostBuilder>();
            functionsHostBuilderMock.SetupGet(x => x.Services).Returns(services);
            
            var result = functionsHostBuilderMock.Object.AddTelemetry(options => {
                options.WithTracing(tracerProviderBuilder => {
                    tracerProviderBuilder.AddSource("TestSource");
                });
            });
            
            Assert.IsNotNull(result);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
    }
}
