using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class NameTransformTest
    {
        [TestMethod]
        public void TestNone()
        {
            var transform = NameTransform.None;
            Assert.AreEqual("TestName", transform.Apply("TestName"));
            Assert.AreEqual("testName", transform.Apply("testName"));
            Assert.AreEqual("__test__name__", transform.Apply("__test__name__"));
            Assert.AreEqual("TestName<TestName, testName>", transform.Apply("TestName<TestName, testName>"));
        }

        [TestMethod]
        public void TestPascalCase()
        {
            var transform = NameTransform.PascalCase;
            Assert.AreEqual("TestName", transform.Apply("TestName"));
            Assert.AreEqual("TestName", transform.Apply("testName"));
            Assert.AreEqual("TestName", transform.Apply("__test__name__"));
            Assert.AreEqual("TestName<TestName, TestName>", transform.Apply("TestName<TestName, testName>"));
        }

        [TestMethod]
        public void TestCamelCase()
        {
            var transform = NameTransform.CamelCase;
            Assert.AreEqual("testName", transform.Apply("TestName"));
            Assert.AreEqual("testName", transform.Apply("testName"));
            Assert.AreEqual("testName", transform.Apply("__test__name__"));
            Assert.AreEqual("testName<testName, testName>", transform.Apply("TestName<TestName, testName>"));
        }

        [TestMethod]
        public void TestSnakeCase()
        {
            var transform = NameTransform.SnakeCase;
            Assert.AreEqual("test_name", transform.Apply("TestName"));
            Assert.AreEqual("test_name", transform.Apply("testName"));
            Assert.AreEqual("test_name", transform.Apply("__test__name__"));
            Assert.AreEqual("test_name<test_name, test_name>", transform.Apply("TestName<TestName, testName>"));
        }

        [TestMethod]
        public void TestDashCase()
        {
            var transform = NameTransform.DashCase;
            Assert.AreEqual("test-name", transform.Apply("TestName"));
            Assert.AreEqual("test-name", transform.Apply("testName"));
            Assert.AreEqual("test-name", transform.Apply("__test__name__"));
            Assert.AreEqual("test-name<test-name, test-name>", transform.Apply("TestName<TestName, testName>"));
        }
    }
}
