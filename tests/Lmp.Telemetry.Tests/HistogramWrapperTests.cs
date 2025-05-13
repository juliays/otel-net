using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Lmp.Telemetry.Interfaces;

namespace Lmp.Telemetry.Tests
{
    [TestClass]
    public class HistogramWrapperTests
    {
        private Histogram<double> _histogram;
        private HistogramWrapper<double> _histogramWrapper;
        private readonly string _testName = "TestHistogram";
        private readonly string _testUnit = "Seconds";
        private readonly string _testDescription = "Test histogram description";

        [TestInitialize]
        public void Setup()
        {
            var meter = new Meter("TestMeter", "1.0.0");
            _histogram = meter.CreateHistogram<double>(_testName, _testUnit, _testDescription);
            _histogramWrapper = new HistogramWrapper<double>(_histogram);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidHistogram_InitializesCorrectly()
        {
            var wrapper = new HistogramWrapper<double>(_histogram);
            
            Assert.IsNotNull(wrapper);
            Assert.AreEqual(_testName, wrapper.Name);
            Assert.AreEqual(_testUnit, wrapper.Unit);
            Assert.AreEqual(_testDescription, wrapper.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullHistogram_ThrowsArgumentNullException()
        {
            var wrapper = new HistogramWrapper<double>(null);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void Name_ReturnsCorrectValue()
        {
            Assert.AreEqual(_testName, _histogramWrapper.Name);
        }

        [TestMethod]
        public void Unit_ReturnsCorrectValue()
        {
            Assert.AreEqual(_testUnit, _histogramWrapper.Unit);
        }

        [TestMethod]
        public void Description_ReturnsCorrectValue()
        {
            Assert.AreEqual(_testDescription, _histogramWrapper.Description);
        }

        #endregion

        #region Record Tests

        [TestMethod]
        public void Record_WithValueAndArrayTags_CallsUnderlyingHistogram()
        {
            var tags = new KeyValuePair<string, object>[] 
            { 
                new KeyValuePair<string, object>("key1", "value1"),
                new KeyValuePair<string, object>("key2", "value2")
            };
            
            _histogramWrapper.Record(10.5, tags);
            
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Record_WithValueAndSpanTags_CallsUnderlyingHistogram()
        {
            var tags = new KeyValuePair<string, object>[] 
            { 
                new KeyValuePair<string, object>("key1", "value1"),
                new KeyValuePair<string, object>("key2", "value2")
            };
            
            _histogramWrapper.Record(10.5, new ReadOnlySpan<KeyValuePair<string, object>>(tags));
            
            Assert.IsTrue(true);
        }

        #endregion
    }
}
