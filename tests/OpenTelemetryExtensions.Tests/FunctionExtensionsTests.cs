using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenTelemetry;
using OpenTelemetryExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            
            Assert.IsTrue(services.Any(s => s.ServiceType == typeof(ActivitySource)));
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
            
            Assert.IsTrue(services.Any(s => s.ServiceType == typeof(ActivitySource)));
        }
        
        [TestMethod]
        public void ConfigureTimerTriggerActivity_RegistersActivitySource()
        {
            var services = new ServiceCollection();
            var functionsHostBuilderMock = new Mock<IFunctionsHostBuilder>();
            functionsHostBuilderMock.SetupGet(x => x.Services).Returns(services);
            
            var result = functionsHostBuilderMock.Object.ConfigureTimerTriggerActivity("TestActivitySource");
            
            Assert.IsNotNull(result);
            
            var activitySourceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ActivitySource));
            Assert.IsNotNull(activitySourceDescriptor);
            
            var serviceProvider = services.BuildServiceProvider();
            var activitySource = serviceProvider.GetService<ActivitySource>();
            Assert.IsNotNull(activitySource);
            Assert.AreEqual("TestActivitySource", activitySource.Name);
        }
        
        [TestMethod]
        public void GetTimerTriggerActivitySource_ReturnsRegisteredActivitySource()
        {
            var services = new ServiceCollection();
            var activitySource = new ActivitySource("TestActivitySource");
            services.AddSingleton<ActivitySource>(activitySource);
            var serviceProvider = services.BuildServiceProvider();
            
            var result = serviceProvider.GetTimerTriggerActivitySource();
            
            Assert.IsNotNull(result);
            Assert.AreEqual("TestActivitySource", result.Name);
            Assert.AreSame(activitySource, result);
        }
        
        [TestMethod]
        public void GetTimerTriggerActivitySource_WhenNotRegistered_ReturnsDefaultActivitySource()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            
            var result = serviceProvider.GetTimerTriggerActivitySource();
            
            Assert.IsNotNull(result);
            Assert.AreEqual("FunctionApp.TimerTriggers", result.Name);
        }
        
        [TestMethod]
        public void TimerTriggerActivity_CreatesAndPropagatesActivity()
        {
            var services = new ServiceCollection();
            var activitySource = new ActivitySource("TestTimerTriggers");
            services.AddSingleton<ActivitySource>(activitySource);
            var serviceProvider = services.BuildServiceProvider();
            
            using var activityListener = new ActivityListener
            {
                ShouldListenTo = s => s.Name == "TestTimerTriggers",
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(activityListener);
            
            var activitySourceFromProvider = serviceProvider.GetTimerTriggerActivitySource();
            
            using var parentActivity = activitySourceFromProvider.StartActivity("TimerParentOperation");
            Assert.IsNotNull(parentActivity);
            
            parentActivity.SetTag("timer.function", "TestTimerFunction");
            
            using var childActivity = activitySourceFromProvider.StartActivity("TimerChildOperation");
            Assert.IsNotNull(childActivity);
            
            Assert.AreEqual("TestTimerTriggers", activitySourceFromProvider.Name);
            Assert.AreEqual("TimerParentOperation", parentActivity.OperationName);
            Assert.AreEqual("TimerChildOperation", childActivity.OperationName);
            
            Assert.AreEqual(parentActivity.TraceId, childActivity.TraceId);
            Assert.AreEqual(parentActivity.SpanId, childActivity.ParentSpanId);
            
            Assert.AreEqual("TestTimerFunction", parentActivity.GetTagItem("timer.function"));
        }
    }
}
