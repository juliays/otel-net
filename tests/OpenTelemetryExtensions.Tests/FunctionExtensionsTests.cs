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
            var functionsHostBuilderMock = new Mock<IFunctionsHostBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            
            functionsHostBuilderMock.SetupGet(x => x.Services).Returns(servicesMock.Object);
            servicesMock.Setup(x => x.BuildServiceProvider()).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);
            
            var result = functionsHostBuilderMock.Object.AddTelemetry();
            
            Assert.IsNotNull(result);
            servicesMock.Verify(x => x.BuildServiceProvider(), Times.Once);
        }
        
        [TestMethod]
        public void AddTelemetry_WithConfigureOptions_RegistersServices()
        {
            var functionsHostBuilderMock = new Mock<IFunctionsHostBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            
            functionsHostBuilderMock.SetupGet(x => x.Services).Returns(servicesMock.Object);
            servicesMock.Setup(x => x.BuildServiceProvider()).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);
            
            var result = functionsHostBuilderMock.Object.AddTelemetry(options => {
                options.WithTracing(tracerProviderBuilder => {
                    tracerProviderBuilder.AddSource("TestSource");
                });
            });
            
            Assert.IsNotNull(result);
            servicesMock.Verify(x => x.BuildServiceProvider(), Times.Once);
        }
    }
}
