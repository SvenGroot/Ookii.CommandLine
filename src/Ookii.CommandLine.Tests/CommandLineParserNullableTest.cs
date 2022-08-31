﻿#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class CommandLineParserNullableTest
    {
        #region Nested types

        class NullReturningStringConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            {
                if (value is string s)
                {
                    if (s == "(null)")
                        return null;
                    else
                        return s;
                }

                return base.ConvertFrom(context, culture, value);
            }
        }

        class NullReturningIntConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            {
                if (value is string s)
                {
                    if (s == "(null)")
                        return null;
                    else
                        return int.Parse(s);
                }

                return base.ConvertFrom(context, culture, value);
            }
        }

        class TestArguments
        {
            public TestArguments(
                [TypeConverter(typeof(NullReturningStringConverter))] string? constructorNullable,
                [TypeConverter(typeof(NullReturningStringConverter))] string constructorNonNullable,
                [TypeConverter(typeof(NullReturningIntConverter))] int constructorValueType,
                [TypeConverter(typeof(NullReturningIntConverter))] int? constructorNullableValueType)
            {
                ConstructorNullable = constructorNullable;
                ConstructorNonNullable = constructorNonNullable;
                ConstructorValueType = constructorValueType;
                ConstructorNullableValueType = constructorNullableValueType;
            }

            public string? ConstructorNullable { get; set; }
            public string ConstructorNonNullable { get; set; }
            public int ConstructorValueType { get; set; }
            public int? ConstructorNullableValueType { get; set; }

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public string? Nullable { get; set; } = "NotNullDefaultValue";

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public string NonNullable { get; set; } = string.Empty;

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningIntConverter))]
            public int ValueType { get; set; }

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningIntConverter))]
            public int? NullableValueType { get; set; } = 42;

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public string[]? NonNullableArray { get; set; }

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningIntConverter))]
            public int[]? ValueArray { get; set; }

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public ICollection<string> NonNullableCollection { get; } = new List<string>();

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningIntConverter))]
            [MultiValueSeparator(";")]
            public ICollection<int> ValueCollection { get; } = new List<int>();

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public string?[]? NullableArray { get; set; }

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningIntConverter))]
            public string?[]? NullableValueArray { get; set; }

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public ICollection<string?> NullableCollection { get; } = new List<string?>();

            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public ICollection<int?> NullableValueCollection { get; } = new List<int?>();

            [CommandLineArgument]
            [KeyTypeConverter(typeof(NullReturningStringConverter))]
            [ValueTypeConverter(typeof(NullReturningStringConverter))]
            public Dictionary<string, string>? NonNullableDictionary { get; set; }

            [CommandLineArgument]
            [ValueTypeConverter(typeof(NullReturningIntConverter))]
            public Dictionary<string, int>? ValueDictionary { get; set; }

            [CommandLineArgument]
            [ValueTypeConverter(typeof(NullReturningStringConverter))]
            public IDictionary<string, string> NonNullableIDictionary { get; } = new Dictionary<string, string>();

            [CommandLineArgument]
            [KeyTypeConverter(typeof(NullReturningStringConverter))]
            [ValueTypeConverter(typeof(NullReturningIntConverter))]
            [MultiValueSeparator(";")]
            public IDictionary<string, int> ValueIDictionary { get; } = new Dictionary<string, int>();

            [CommandLineArgument]
            [KeyTypeConverter(typeof(NullReturningStringConverter))]
            [ValueTypeConverter(typeof(NullReturningStringConverter))]
            public Dictionary<string, string?>? NullableDictionary { get; set; }

            [CommandLineArgument]
            [KeyTypeConverter(typeof(NullReturningStringConverter))]
            [ValueTypeConverter(typeof(NullReturningIntConverter))]
            public Dictionary<string, int?>? NullableValueDictionary { get; set; }

            [CommandLineArgument]
            [KeyTypeConverter(typeof(NullReturningStringConverter))]
            [ValueTypeConverter(typeof(NullReturningStringConverter))]
            public IDictionary<string, string?> NullableIDictionary { get; } = new Dictionary<string, string?>();

            [CommandLineArgument]
            [KeyTypeConverter(typeof(NullReturningStringConverter))]
            [ValueTypeConverter(typeof(NullReturningIntConverter))]
            [MultiValueSeparator(";")]
            public IDictionary<string, int?> NullableValueIDictionary { get; } = new Dictionary<string, int?>();

            // This is an incorrect type converter (doesn't return KeyValuePair), but it doesn't
            // matter since it'll only be used to test null values.
            [CommandLineArgument]
            [TypeConverter(typeof(NullReturningStringConverter))]
            public Dictionary<string, string?>? InvalidDictionary { get; set; }
        }

        #endregion

        [TestMethod]
        public void TestAllowNull()
        {
            var parser = new CommandLineParser(typeof(TestArguments));
            Assert.IsTrue(parser.GetArgument("constructorNullable")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("constructorNonNullable")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("constructorValueType")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("constructorNullableValueType")!.AllowNull);

            Assert.IsTrue(parser.GetArgument("Nullable")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("NonNullable")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("ValueType")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableValueType")!.AllowNull);

            Assert.IsFalse(parser.GetArgument("NonNullableArray")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("ValueArray")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("NonNullableCollection")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("ValueCollection")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableArray")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableValueArray")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableCollection")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableValueCollection")!.AllowNull);

            Assert.IsFalse(parser.GetArgument("NonNullableDictionary")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("ValueDictionary")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("NonNullableIDictionary")!.AllowNull);
            Assert.IsFalse(parser.GetArgument("ValueIDictionary")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableDictionary")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableValueDictionary")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableIDictionary")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("NullableValueIDictionary")!.AllowNull);
        }

        [TestMethod]
        public void TestNonNullableConstructor()
        {
            var parser = new CommandLineParser(typeof(TestArguments));
            ExpectNullException(parser, "constructorNonNullable", "foo", "(null)", "4", "5");
            ExpectNullException(parser, "constructorValueType", "foo", "bar", "(null)", "5");
            var result = ExpectSuccess(parser, "(null)", "bar", "4", "(null)");
            Assert.IsNull(result.ConstructorNullable);
            Assert.AreEqual("bar", result.ConstructorNonNullable);
            Assert.AreEqual(4, result.ConstructorValueType);
            Assert.IsNull(result.ConstructorNullableValueType);
        }

        [TestMethod]
        public void TestNonNullableProperties()
        {
            var parser = new CommandLineParser(typeof(TestArguments));
            ExpectNullException(parser, "NonNullable", "foo", "bar", "4", "5", "-NonNullable", "(null)");
            ExpectNullException(parser, "ValueType", "foo", "bar", "4", "5", "-ValueType", "(null)");
            var result = ExpectSuccess(parser, "foo", "bar", "4", "5", "-NonNullable", "baz", "-ValueType", "47", "-Nullable", "(null)", "-NullableValueType", "(null)");
            Assert.IsNull(result.Nullable);
            Assert.AreEqual("baz", result.NonNullable);
            Assert.AreEqual(47, result.ValueType);
            Assert.IsNull(result.NullableValueType);
        }

        [TestMethod]
        public void TestNonNullableMultiValue()
        {
            var parser = new CommandLineParser(typeof(TestArguments));
            ExpectNullException(parser, "NonNullableArray", "-NonNullableArray", "foo", "-NonNullableArray", "(null)");
            ExpectNullException(parser, "NonNullableCollection", "-NonNullableCollection", "foo", "-NonNullableCollection", "(null)");
            ExpectNullException(parser, "ValueArray", "-ValueArray", "5", "-ValueArray", "(null)");
            ExpectNullException(parser, "ValueCollection", "-ValueCollection", "5;(null)");
            ExpectNullException(parser, "ValueCollection", "-ValueCollection", "5", "-ValueCollection", "(null)");
            var result = ExpectSuccess(parser, "a", "b", "4", "5", "-NonNullableArray", "foo", "-NonNullableArray", "bar",
                "-NonNullableCollection", "baz", "-NonNullableCollection", "bif",
                "-ValueArray", "5", "-ValueArray", "6",
                "-ValueCollection", "6;7",
                "-NullableValueArray", "(null)",
                "-NullableValueCollection", "(null)",
                "-NullableArray", "(null)",
                "-NullableCollection", "(null)"
                );
            CollectionAssert.AreEqual(new[] { "foo", "bar", }, result.NonNullableArray);
            CollectionAssert.AreEqual(new[] { "baz", "bif", }, (List<string>)result.NonNullableCollection);
            CollectionAssert.AreEqual(new[] { 5, 6 }, result.ValueArray);
            CollectionAssert.AreEqual(new[] { 6, 7 }, (List<int>)result.ValueCollection);
            CollectionAssert.AreEqual(new int?[] { null }, result.NullableValueArray);
            CollectionAssert.AreEqual(new int?[] { null }, (List<int?>)result.NullableValueCollection);
            CollectionAssert.AreEqual(new string?[] { null }, (List<string?>)result.NullableCollection);
            CollectionAssert.AreEqual(new string?[] { null }, result.NullableArray);
        }

        [TestMethod]
        public void TestNonNullableDictionary()
        {
            var parser = new CommandLineParser(typeof(TestArguments));
            ExpectNullException(parser, "NonNullableDictionary", "-NonNullableDictionary", "foo=bar", "-NonNullableDictionary", "baz=(null)");
            ExpectNullException(parser, "NonNullableIDictionary", "-NonNullableIDictionary", "foo=bar", "-NonNullableIDictionary", "baz=(null)");
            ExpectNullException(parser, "ValueDictionary", "-ValueDictionary", "foo=5", "-ValueDictionary", "foo=(null)");
            ExpectNullException(parser, "ValueIDictionary", "-ValueIDictionary", "foo=5;bar=(null)");
            ExpectNullException(parser, "ValueIDictionary", "-ValueIDictionary", "foo=5", "-ValueIDictionary", "bar=(null)");
            // A null key is never allowed.
            ExpectNullException(parser, "NullableDictionary", "-NullableDictionary", "(null)=foo");
            // The whole KeyValuePair being null is never allowed.
            ExpectNullException(parser, "InvalidDictionary", "-InvalidDictionary", "(null)");
            var result = ExpectSuccess(parser, "a", "b", "4", "5", "-NonNullableDictionary", "foo=bar", "-NonNullableDictionary", "bar=baz",
                "-NonNullableIDictionary", "baz=bam", "-NonNullableIDictionary", "bif=zap",
                "-ValueDictionary", "foo=5", "-ValueDictionary", "bar=6",
                "-ValueIDictionary", "foo=6;bar=7",
                "-NullableValueDictionary", "foo=(null)",
                "-NullableValueIDictionary", "bar=(null)",
                "-NullableDictionary", "baz=(null)",
                "-NullableIDictionary", "bif=(null)"
                );
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("foo", "bar"), KeyValuePair.Create("bar", "baz") }, result.NonNullableDictionary);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("baz", "bam"), KeyValuePair.Create("bif", "zap") }, (Dictionary<string, string>)result.NonNullableIDictionary);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("foo", 5), KeyValuePair.Create("bar", 6) }, result.ValueDictionary);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("foo", 6), KeyValuePair.Create("bar", 7) }, (Dictionary<string, int>)result.ValueIDictionary);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("foo", (int?)null) }, result.NullableValueDictionary);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("bar", (int?)null) }, (Dictionary<string, int?>)result.NullableValueIDictionary);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("baz", (string?)null) }, result.NullableDictionary);
            CollectionAssert.AreEquivalent(new[] { KeyValuePair.Create("bif", (string?)null) }, (Dictionary<string, string?>)result.NullableIDictionary);

        }

        private static void ExpectNullException(CommandLineParser parser, string argumentName, params string[] args)
        {
            try
            {
                parser.Parse(args);
                Assert.Fail("Expected exception not thrown.");
            }
            catch (CommandLineArgumentException ex)
            {
                Assert.AreEqual(CommandLineArgumentErrorCategory.NullArgumentValue, ex.Category);
                Assert.AreEqual(argumentName, ex.ArgumentName);
            }
        }

        private static TestArguments ExpectSuccess(CommandLineParser parser, params string[] args)
        {
            var result = (TestArguments?)parser.Parse(args);
            Assert.IsNotNull(result);
            return result;
        }
    }
}