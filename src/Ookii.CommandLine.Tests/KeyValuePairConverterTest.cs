using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Conversion;
using System.Collections.Generic;
using System.Globalization;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class KeyValuePairConverterTest
    {
        [TestMethod]
        public void TestConvertFrom()
        {
            var converter = new KeyValuePairConverter<string, int>();
            var converted = converter.Convert("foo=5", CultureInfo.InvariantCulture);
            Assert.AreEqual(KeyValuePair.Create("foo", 5), converted);
        }

        [TestMethod]
        public void TestCustomSeparator()
        {
            var converter = new KeyValuePairConverter<string, int>(new LocalizedStringProvider(), "Test", false, null, null, ":");
            var pair = converter.Convert("foo:5", CultureInfo.InvariantCulture);
            Assert.AreEqual(KeyValuePair.Create("foo", 5), pair);
        }
    }
}
