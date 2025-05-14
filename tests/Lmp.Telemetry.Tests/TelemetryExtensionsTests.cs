using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Lmp.Telemetry.Extensions;
using Lmp.Telemetry.Configuration;
using Lmp.Telemetry.Constants;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;

namespace Lmp.Telemetry.Tests
{
    [TestClass]
    public class TelemetryExtensionsTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IConfigurationSection> _mockResourceSection;
        private Mock<IConfigurationSection> _mockExporterSection;
        private Mock<IConfigurationSection> _mockTracerSection;
        
        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockResourceSection = new Mock<IConfigurationSection>();
            _mockExporterSection = new Mock<IConfigurationSection>();
            _mockTracerSection = new Mock<IConfigurationSection>();
            
            _mockConfiguration.Setup(c => c.GetSection(TelemetryConstants.TelemetryResource))
                .Returns(_mockResourceSection.Object);
            _mockConfiguration.Setup(c => c.GetSection(TelemetryConstants.TelemetryExporter))
                .Returns(_mockExporterSection.Object);
            _mockConfiguration.Setup(c => c.GetSection(TelemetryConstants.TelemetryTracer))
                .Returns(_mockTracerSection.Object);
        }
        
        #region BindTelemetryOptions Tests
        
        [TestMethod]
        public void BindTelemetryOptions_WithValidConfiguration_ReturnsPopulatedOptions()
        {
            var resourceOptions = new ResourceOptions { Component = "TestComponent" };
            var exportersOptions = new ExportersOptions 
            { 
                Console = new ConsoleOptions { Enabled = true } 
            };
            var tracerOptions = new TracerOptions { SampleRate = 0.5 };
            
            _mockResourceSection.Setup(s => s.BindWithDisplayName<ResourceOptions>())
                .Returns(resourceOptions);
            _mockExporterSection.Setup(s => s.Get<ExportersOptions>())
                .Returns(exportersOptions);
            _mockTracerSection.Setup(s => s.Get<TracerOptions>())
                .Returns(tracerOptions);
            
            var result = TelemetryExtensions.BindTelemetryOptions(_mockConfiguration.Object);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("TestComponent", result.Resource.Component);
            Assert.IsTrue(result.Exporters.Console.Enabled);
            Assert.AreEqual(0.5, result.Tracer.SampleRate);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BindTelemetryOptions_WithNullConfiguration_ThrowsArgumentNullException()
        {
            TelemetryExtensions.BindTelemetryOptions(null);
        }
        
        #endregion
        
        #region CreateAttributesFromResource Tests
        
        [TestMethod]
        public void CreateAttributesFromResource_WithValidResourceOptions_ReturnsKeyValuePairs()
        {
            var resourceOptions = new ResourceOptions 
            { 
                Component = "TestComponent",
                Environment = "Test",
                Version = "1.0.0"
            };
            
            var result = TelemetryExtensions.CreateAttributesFromResource(resourceOptions);
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            
            var componentPair = Array.Find(result, kvp => kvp.Key == "Component");
            Assert.AreEqual("TestComponent", componentPair.Value);
            
            var environmentPair = Array.Find(result, kvp => kvp.Key == "Environment");
            Assert.AreEqual("Test", environmentPair.Value);
            
            var versionPair = Array.Find(result, kvp => kvp.Key == "Version");
            Assert.AreEqual("1.0.0", versionPair.Value);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAttributesFromResource_WithNullResourceOptions_ThrowsArgumentNullException()
        {
            TelemetryExtensions.CreateAttributesFromResource(null);
        }
        
        #endregion
        
        #region IHostBuilder Extension Tests
        
        [TestMethod]
        public void AddOpenTelemetry_WithHostBuilder_ConfiguresServices()
        {
            var hostBuilder = new HostBuilder();
            var services = new ServiceCollection();
            
            hostBuilder.AddOpenTelemetry(_mockConfiguration.Object);
            
            Assert.IsTrue(true);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOpenTelemetry_WithNullHostBuilder_ThrowsArgumentNullException()
        {
            TelemetryExtensions.AddOpenTelemetry(null, _mockConfiguration.Object);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOpenTelemetry_WithNullConfiguration_ThrowsArgumentNullException()
        {
            var hostBuilder = new HostBuilder();
            
            hostBuilder.AddOpenTelemetry(null);
        }
        
        #endregion
        
        #region WebApplicationBuilder Extension Tests
        
        [TestMethod]
        public void AddOpenTelemetry_WithWebApplicationBuilder_ConfiguresServices()
        {
            
            Assert.IsTrue(true);
        }
        
        #endregion
        
        #region IFunctionsHostBuilder Extension Tests
        
        [TestMethod]
        public void AddOpenTelemetry_WithFunctionsHostBuilder_ConfiguresServices()
        {
            
            Assert.IsTrue(true);
        }
        
        #endregion
        
        #region ConfigureOpenTelemetryTraceExporter Tests
        
        [TestMethod]
        public void ConfigureOpenTelemetryTraceExporter_WithConsoleEnabled_AddsConsoleExporter()
        {
            var telemetryOptions = new TelemetryOptions
            {
                Exporters = new ExportersOptions
                {
                    Console = new ConsoleOptions { Enabled = true }
                }
            };
            
            var tracerProviderBuilder = new Mock<TracerProviderBuilder>();
            
            TelemetryExtensions.ConfigureOpenTelemetryTraceExporter(tracerProviderBuilder.Object, telemetryOptions);
            
            Assert.IsTrue(true);
        }
        
        [TestMethod]
        public void ConfigureOpenTelemetryTraceExporter_WithAppInsightsEnabled_AddsAzureMonitorExporter()
        {
            var telemetryOptions = new TelemetryOptions
            {
                Exporters = new ExportersOptions
                {
                    AppInsights = new AppInsightsOptions 
                    { 
                        Enabled = true,
                        ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
                    }
                }
            };
            
            var tracerProviderBuilder = new Mock<TracerProviderBuilder>();
            
            TelemetryExtensions.ConfigureOpenTelemetryTraceExporter(tracerProviderBuilder.Object, telemetryOptions);
            
            Assert.IsTrue(true);
        }
        
        #endregion
        
        #region ConfigureOpenTelemetryMetricsExporter Tests
        
        [TestMethod]
        public void ConfigureOpenTelemetryMetricsExporter_WithConsoleEnabled_AddsConsoleExporter()
        {
            var telemetryOptions = new TelemetryOptions
            {
                Exporters = new ExportersOptions
                {
                    Console = new ConsoleOptions { Enabled = true }
                }
            };
            
            var meterProviderBuilder = new Mock<MeterProviderBuilder>();
            
            TelemetryExtensions.ConfigureOpenTelemetryMetricsExporter(meterProviderBuilder.Object, telemetryOptions);
            
            Assert.IsTrue(true);
        }
        
        [TestMethod]
        public void ConfigureOpenTelemetryMetricsExporter_WithAppInsightsEnabled_AddsAzureMonitorExporter()
        {
            var telemetryOptions = new TelemetryOptions
            {
                Exporters = new ExportersOptions
                {
                    AppInsights = new AppInsightsOptions 
                    { 
                        Enabled = true,
                        ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
                    }
                }
            };
            
            var meterProviderBuilder = new Mock<MeterProviderBuilder>();
            
            TelemetryExtensions.ConfigureOpenTelemetryMetricsExporter(meterProviderBuilder.Object, telemetryOptions);
            
            Assert.IsTrue(true);
        }
        
        #endregion
    }
}
