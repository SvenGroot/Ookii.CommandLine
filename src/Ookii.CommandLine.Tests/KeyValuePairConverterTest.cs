using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class KeyValuePairConverterTest
    {
        [TestMethod]
        public void TestConvertFrom()
        {
            var converter = new KeyValuePairConverter<string, int>();
            Assert.IsTrue(converter.CanConvertFrom(null, typeof(string)));
            var converted = converter.ConvertFromInvariantString(null, "foo=5");
            Assert.AreEqual(KeyValuePair.Create("foo", 5), converted);
        }

        [TestMethod]
        public void TestConvertTo()
        {
            var converter = new KeyValuePairConverter<string, int>();
            Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
            var converted = converter.ConvertToInvariantString(null, KeyValuePair.Create("bar", 6));
            Assert.AreEqual("bar=6", converted);
        }

        [TestMethod]
        public void TestCustomSeparator()
        {
            var converter = new KeyValuePairConverter<string, int>(new LocalizedStringProvider(), "Test", false, null, null, ":");
            var pair = converter.ConvertFromInvariantString(null, "foo:5");
            Assert.AreEqual(KeyValuePair.Create("foo", 5), pair);

            Assert.IsTrue(converter.CanConvertTo(null, typeof(string)));
            var converted = converter.ConvertToInvariantString(null, KeyValuePair.Create("bar", 6));
            Assert.AreEqual("bar:6", converted);
        }
    }
}
