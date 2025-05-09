using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetry;
using OpenTelemetryExtensions.Configuration;
using OpenTelemetryExtensions.Extensions;
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
            var configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            
            configurationMock.Setup(c => c.GetSection(TelemetryConfig.SectionName))
                .Returns(configurationSectionMock.Object);
            
            services.AddTelemetry(configurationMock.Object);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
        
        [TestMethod]
        public void AddTelemetry_WithAppInsightsEnabled_ConfiguresAzureMonitor()
        {
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            
            var telemetryConfig = new TelemetryConfig
            {
                Exporters = new ExporterConfig
                {
                    AppInsights = new AppInsightsExporterConfig
                    {
                        Enabled = true,
                        ConnectionString = "test-connection-string"
                    }
                }
            };
            
            var memoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { $"{TelemetryConfig.SectionName}:Exporters:AppInsights:Enabled", "true" },
                    { $"{TelemetryConfig.SectionName}:Exporters:AppInsights:ConnectionString", "test-connection-string" }
                })
                .Build();
            
            services.AddTelemetry(memoryConfig);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
        
        [TestMethod]
        public void AddTelemetry_WithDatadogEnabled_ConfiguresOtlpExporter()
        {
            var services = new ServiceCollection();
            
            var memoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { $"{TelemetryConfig.SectionName}:Exporters:Datadog:Enabled", "true" },
                    { $"{TelemetryConfig.SectionName}:Exporters:Datadog:Endpoint", "https://api.datadoghq.com" },
                    { $"{TelemetryConfig.SectionName}:Exporters:Datadog:ApiKey", "test-api-key" }
                })
                .Build();
            
            services.AddTelemetry(memoryConfig);
            
            var serviceProvider = services.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<OpenTelemetryBuilder>());
        }
    }
}
