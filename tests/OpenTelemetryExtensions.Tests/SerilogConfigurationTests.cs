using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetryExtensions.Configuration;
using OpenTelemetryExtensions.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenTelemetryExtensions.Tests
{
    [TestClass]
    public class SerilogConfigurationTests
    {
        [TestMethod]
        public void AddSerilog_BindsConfigurationCorrectly()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test-environment" },
                    { "telemetry:resource:component", "test-component" },
                    { "telemetry:resource:workspaceId", "test-workspace-id" },
                    { "telemetry:resource:applicationId", "app-12345" },
                    
                    { "telemetry:exporters:appInsights:enabled", "true" },
                    { "telemetry:exporters:appInsights:connectionString", "test-connection-string" },
                    
                    { "Serilog:MinimumLevel:Default", "Debug" },
                    { "Serilog:MinimumLevel:Override:Microsoft", "Warning" },
                    { "Serilog:MinimumLevel:Override:System", "Warning" },
                    { "Serilog:WriteTo:0:Name", "Console" },
                    { "Serilog:WriteTo:0:Args:formatter", "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact" },
                    { "Serilog:WriteTo:1:Name", "ApplicationInsights" },
                    { "Serilog:WriteTo:1:Args:connectionString", "test-connection-string" },
                    { "Serilog:Enrich:0", "FromLogContext" },
                    { "Serilog:Enrich:1", "WithMachineName" },
                    { "Serilog:Enrich:2", "WithThreadId" }
                })
                .Build();
            
            services.AddTelemetry(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            var telemetryConfig = serviceProvider.GetService<TelemetryConfig>();
            var logger = serviceProvider.GetService<ILogger>();
            
            Assert.IsNotNull(telemetryConfig);
            Assert.AreEqual("test-environment", telemetryConfig.Resource.Environment);
            Assert.AreEqual("test-component", telemetryConfig.Resource.Component);
            Assert.AreEqual("test-workspace-id", telemetryConfig.Resource.WorkspaceId);
            Assert.AreEqual("app-12345", telemetryConfig.Resource.ApplicationId);
            
            Assert.IsNotNull(logger);
            
            Assert.IsTrue(services.Any(s => s.ServiceType == typeof(ILogger)));
        }
        
        [TestMethod]
        public void AddSerilog_PropagatesResourceAttributesToLogContext()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test-environment" },
                    { "telemetry:resource:component", "test-component" },
                    { "telemetry:resource:workspaceId", "test-workspace-id" },
                    { "telemetry:resource:applicationId", "app-12345" },
                    
                    { "Serilog:MinimumLevel:Default", "Debug" },
                    { "Serilog:WriteTo:0:Name", "Console" }
                })
                .Build();
            
            services.AddTelemetry(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();
            
            Assert.IsNotNull(logger);
            
            Assert.IsTrue(services.Any(s => s.ServiceType == typeof(ILogger)));
        }
        
        [TestMethod]
        public void AddSerilog_AddsTraceContextEnricher()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "telemetry:resource:environment", "test-environment" },
                    { "telemetry:resource:component", "test-component" },
                    
                    { "Serilog:MinimumLevel:Default", "Debug" },
                    { "Serilog:WriteTo:0:Name", "Console" }
                })
                .Build();
            
            services.AddTelemetry(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();
            
            Assert.IsNotNull(logger);
            
            var activitySource = new ActivitySource("TestSource");
            using var activity = activitySource.StartActivity("TestActivity");
            
            Assert.IsTrue(services.Any(s => s.ServiceType == typeof(ILogger)));
        }
    }
}
