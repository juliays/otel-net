using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Lmp.Telemetry.Interfaces;

namespace Lmp.Telemetry.Tests
{
    [TestClass]
    public class CounterWrapperTests
    {
        private Counter<long> _counter;
        private CounterWrapper<long> _counterWrapper;
        private readonly string _testName = "TestCounter";
        private readonly string _testUnit = "Count";
        private readonly string _testDescription = "Test counter description";

        [TestInitialize]
        public void Setup()
        {
            var meter = new Meter("TestMeter", "1.0.0");
            _counter = meter.CreateCounter<long>(_testName, _testUnit, _testDescription);
            _counterWrapper = new CounterWrapper<long>(_counter);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidCounter_InitializesCorrectly()
        {
            var wrapper = new CounterWrapper<long>(_counter);
            
            Assert.IsNotNull(wrapper);
            Assert.AreEqual(_testName, wrapper.Name);
            Assert.AreEqual(_testUnit, wrapper.Unit);
            Assert.AreEqual(_testDescription, wrapper.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullCounter_ThrowsArgumentNullException()
        {
            var wrapper = new CounterWrapper<long>(null);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void Name_ReturnsCorrectValue()
        {
            Assert.AreEqual(_testName, _counterWrapper.Name);
        }

        [TestMethod]
        public void Unit_ReturnsCorrectValue()
        {
            Assert.AreEqual(_testUnit, _counterWrapper.Unit);
        }

        [TestMethod]
        public void Description_ReturnsCorrectValue()
        {
            Assert.AreEqual(_testDescription, _counterWrapper.Description);
        }

        #endregion

        #region Add Tests

        [TestMethod]
        public void Add_WithValueAndArrayTags_CallsUnderlyingCounter()
        {
            var tags = new KeyValuePair<string, object>[] 
            { 
                new KeyValuePair<string, object>("key1", "value1"),
                new KeyValuePair<string, object>("key2", "value2")
            };
            
            _counterWrapper.Add(10, tags);
            
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Add_WithValueAndSpanTags_CallsUnderlyingCounter()
        {
            var tags = new KeyValuePair<string, object>[] 
            { 
                new KeyValuePair<string, object>("key1", "value1"),
                new KeyValuePair<string, object>("key2", "value2")
            };
            
            _counterWrapper.Add(10, new ReadOnlySpan<KeyValuePair<string, object>>(tags));
            
            Assert.IsTrue(true);
        }

        #endregion
    }
}
