using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Conversion;
using System.Collections.Generic;
using System.Globalization;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class KeyValuePairConverterTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Avoid exception when testing reflection on argument types that also have the
            // GeneratedParseAttribute set.
            ParseOptions.AllowReflectionWithGeneratedParserDefault = true;
        }

        // Needed because SpanParsableConverter only exists on .Net 7.
        private class IntConverter : ArgumentConverter
        {
            public override object Convert(string value, CultureInfo culture, CommandLineArgument argument)
                => int.Parse(value, culture);
        }

        [TestMethod]
        public void TestConvertFrom()
        {
            var parser = new CommandLineParser<SimpleArguments>();
            var converter = new KeyValuePairConverter<string, int>();
            var converted = converter.Convert("foo=5", CultureInfo.InvariantCulture, parser.GetArgument("Argument1"));
            Assert.AreEqual(KeyValuePair.Create("foo", 5), converted);
        }

        [TestMethod]
        public void TestCustomSeparator()
        {
            var parser = new CommandLineParser<SimpleArguments>();
            var converter = new KeyValuePairConverter<string, int>(new StringConverter(), new IntConverter(), ":", false);
            var pair = converter.Convert("foo:5", CultureInfo.InvariantCulture, parser.GetArgument("Argument1"));
            Assert.AreEqual(KeyValuePair.Create("foo", 5), pair);
        }
    }
}
