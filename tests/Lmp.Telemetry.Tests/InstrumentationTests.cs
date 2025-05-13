using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Lmp.Telemetry.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lmp.Telemetry.Tests
{
    [TestClass]
    public class InstrumentationTests
    {
        private Mock<ILogger<Instrumentation>> _loggerMock;
        private Mock<IActivitySource> _activitySourceMock;
        private Mock<IMeter> _meterMock;
        private Activity _testActivity;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<Instrumentation>>();
            _activitySourceMock = new Mock<IActivitySource>();
            _meterMock = new Mock<IMeter>();
            _testActivity = new Activity("TestActivity");
            
            _activitySourceMock.Setup(a => a.StartActivity(
                It.IsAny<string>(), 
                It.IsAny<ActivityKind>()))
                .Returns(_testActivity);
                
            _activitySourceMock.Setup(a => a.StartActivity(
                It.IsAny<string>(), 
                It.IsAny<ActivityKind>(),
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.IsAny<IEnumerable<ActivityLink>>(),
                It.IsAny<DateTimeOffset>()))
                .Returns(_testActivity);
                
            _meterMock.Setup(m => m.Name).Returns("TestMeter");
            _meterMock.Setup(m => m.Version).Returns("1.0.0");
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithComponentAndVersion_InitializesCorrectly()
        {
            var instrumentation = new Instrumentation("TestComponent", "1.0.0", _loggerMock.Object);

            Assert.IsNotNull(instrumentation);
        }

        [TestMethod]
        public void Constructor_WithComponentOnly_InitializesWithDefaultVersion()
        {
            var instrumentation = new Instrumentation("TestComponent", _loggerMock.Object);

            Assert.IsNotNull(instrumentation);
        }

        [TestMethod]
        public void Constructor_WithCustomActivitySourceAndMeter_InitializesCorrectly()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            Assert.IsNotNull(instrumentation);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullComponent_ThrowsArgumentNullException()
        {
            var instrumentation = new Instrumentation(null, _loggerMock.Object);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyComponent_ThrowsArgumentException()
        {
            var instrumentation = new Instrumentation("", _loggerMock.Object);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            var instrumentation = new Instrumentation("TestComponent", null);

        }

        #endregion

        #region StartSpan Tests

        [TestMethod]
        public void StartServerSpan_WithName_CallsStartSpanWithServerKind()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            var activity = instrumentation.StartServerSpan("TestSpan");

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Server), 
                Times.Once);
        }

        [TestMethod]
        public void StartClientSpan_WithName_CallsStartSpanWithClientKind()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            var activity = instrumentation.StartClientSpan("TestSpan");

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Client), 
                Times.Once);
        }

        [TestMethod]
        public void StartInternalSpan_WithName_CallsStartSpanWithInternalKind()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            var activity = instrumentation.StartInternalSpan("TestSpan");

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Internal), 
                Times.Once);
        }

        [TestMethod]
        public void StartSpan_WithNameAndKind_CallsStartActivity()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            var activity = instrumentation.StartSpan("TestSpan", ActivityKind.Server);

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Server), 
                Times.Once);
        }

        [TestMethod]
        public void StartSpan_WithNameKindAndTags_CallsStartLinkedSpan()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);
            var tags = new Dictionary<string, object> { { "key", "value" } };

            var activity = instrumentation.StartSpan("TestSpan", ActivityKind.Server, tags);

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Server,
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.IsAny<IEnumerable<ActivityLink>>(),
                It.IsAny<DateTimeOffset>()), 
                Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartSpan_WithNullName_ThrowsArgumentNullException()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            instrumentation.StartSpan(null, ActivityKind.Server);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StartSpan_WithEmptyName_ThrowsArgumentException()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            instrumentation.StartSpan("", ActivityKind.Server);

        }

        #endregion

        #region StartLinkedSpan Tests

        [TestMethod]
        public void StartLinkedServerSpan_WithNameAndLinkedActivity_CallsStartLinkedSpanWithServerKind()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);
            var linkedActivity = new List<string> { "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01" };

            var activity = instrumentation.StartLinkedServerSpan("TestSpan", linkedActivity);

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Server,
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.IsAny<IEnumerable<ActivityLink>>(),
                It.IsAny<DateTimeOffset>()), 
                Times.Once);
        }

        [TestMethod]
        public void StartLinkedClientSpan_WithNameAndLinkedActivity_CallsStartLinkedSpanWithClientKind()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);
            var linkedActivity = new List<string> { "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01" };

            var activity = instrumentation.StartLinkedClientSpan("TestSpan", linkedActivity);

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Client,
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.IsAny<IEnumerable<ActivityLink>>(),
                It.IsAny<DateTimeOffset>()), 
                Times.Once);
        }

        [TestMethod]
        public void StartLinkedInternalSpan_WithNameAndLinkedActivity_CallsStartLinkedSpanWithInternalKind()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);
            var linkedActivity = new List<string> { "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01" };

            var activity = instrumentation.StartLinkedInternalSpan("TestSpan", linkedActivity);

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Internal,
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.IsAny<IEnumerable<ActivityLink>>(),
                It.IsAny<DateTimeOffset>()), 
                Times.Once);
        }

        [TestMethod]
        public void StartLinkedSpan_WithValidParameters_CallsStartActivity()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);
            var linkedActivity = new List<string> { "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01" };
            var tags = new Dictionary<string, object> { { "key", "value" } };

            var activity = instrumentation.StartLinkedSpan("TestSpan", linkedActivity, ActivityKind.Server, tags);

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Server,
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.IsAny<IEnumerable<ActivityLink>>(),
                It.IsAny<DateTimeOffset>()), 
                Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartLinkedSpan_WhenActivityCannotBeStarted_ThrowsInvalidOperationException()
        {
            _activitySourceMock.Setup(a => a.StartActivity(
                It.IsAny<string>(), 
                It.IsAny<ActivityKind>(),
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.IsAny<IEnumerable<ActivityLink>>(),
                It.IsAny<DateTimeOffset>()))
                .Returns((Activity)null);
                
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            instrumentation.StartLinkedSpan("TestSpan", null, ActivityKind.Server);

        }

        [TestMethod]
        public void StartLinkedSpan_WithInvalidActivityContext_SkipsInvalidContexts()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);
            var linkedActivity = new List<string> { "invalid-context-string" };

            var activity = instrumentation.StartLinkedSpan("TestSpan", linkedActivity, ActivityKind.Server);

            Assert.AreEqual(_testActivity, activity);
            _activitySourceMock.Verify(a => a.StartActivity(
                "TestSpan", 
                ActivityKind.Server,
                It.IsAny<ActivityContext>(),
                It.IsAny<IEnumerable<KeyValuePair<string, object?>>>(),
                It.Is<IEnumerable<ActivityLink>>(links => !links.Any()),
                It.IsAny<DateTimeOffset>()), 
                Times.Once);
        }

        #endregion

        #region Dispose Tests

        [TestMethod]
        public void Dispose_CallsDisposeOnActivitySourceAndMeter()
        {
            var instrumentation = new Instrumentation(
                "TestComponent", 
                _loggerMock.Object, 
                _activitySourceMock.Object, 
                _meterMock.Object);

            instrumentation.Dispose();

            _activitySourceMock.Verify(a => a.Dispose(), Times.Once);
            _meterMock.Verify(m => m.Dispose(), Times.Once);
        }

        #endregion
    }
}
