// Copyright (c) Sven Groot (Ookii.org)

// These tests don't apply to .Net Framework.
#if NET6_0_OR_GREATER

#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class CommandLineParserNullableTest
    {
        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestAllowNull(ArgumentProviderKind kind)
        {
            var parser = CommandLineParserTest.CreateParser<NullableArguments>(kind);
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
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestNonNullableConstructor(ArgumentProviderKind kind)
        {
            // TODO: Update for new ctor arguments style.
            var parser = CommandLineParserTest.CreateParser<NullableArguments>(kind);
            ExpectNullException(parser, "constructorNonNullable", "foo", "(null)", "4", "5");
            ExpectNullException(parser, "constructorValueType", "foo", "bar", "(null)", "5");
            var result = ExpectSuccess(parser, "(null)", "bar", "4", "(null)");
            Assert.IsNull(result.ConstructorNullable);
            Assert.AreEqual("bar", result.ConstructorNonNullable);
            Assert.AreEqual(4, result.ConstructorValueType);
            Assert.IsNull(result.ConstructorNullableValueType);
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestNonNullableProperties(ArgumentProviderKind kind)
        {
            var parser = CommandLineParserTest.CreateParser<NullableArguments>(kind);
            ExpectNullException(parser, "NonNullable", "foo", "bar", "4", "5", "-NonNullable", "(null)");
            ExpectNullException(parser, "ValueType", "foo", "bar", "4", "5", "-ValueType", "(null)");
            var result = ExpectSuccess(parser, "foo", "bar", "4", "5", "-NonNullable", "baz", "-ValueType", "47", "-Nullable", "(null)", "-NullableValueType", "(null)");
            Assert.IsNull(result.Nullable);
            Assert.AreEqual("baz", result.NonNullable);
            Assert.AreEqual(47, result.ValueType);
            Assert.IsNull(result.NullableValueType);
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestNonNullableMultiValue(ArgumentProviderKind kind)
        {
            var parser = CommandLineParserTest.CreateParser<NullableArguments>(kind);
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
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestNonNullableDictionary(ArgumentProviderKind kind)
        {
            var parser = CommandLineParserTest.CreateParser<NullableArguments>(kind);
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

        private static NullableArguments ExpectSuccess(CommandLineParser parser, params string[] args)
        {
            var result = (NullableArguments?)parser.Parse(args);
            Assert.IsNotNull(result);
            return result;
        }

        public static string GetCustomDynamicDataDisplayName(MethodInfo methodInfo, object[] data)
            => $"{methodInfo.Name} ({data[0]})";


        public static IEnumerable<object[]> ProviderKinds
            => new[]
            {
                new object[] { ArgumentProviderKind.Reflection },
                new object[] { ArgumentProviderKind.Generated }
            };
    }

    class NullReturningStringConverter : ArgumentConverter
    {
        public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
        {
            if (value == "(null)")
            {
                return null;
            }
            else
            {
                return value;
            }
        }
    }

    class NullReturningIntConverter : ArgumentConverter
    {
        public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
        {
            if (value == "(null)")
            {
                return null;
            }
            else
            {
                return int.Parse(value);
            }
        }
    }

    [GeneratedParser]
    partial class NullableArguments
    {
        // TODO: Put back with new ctor approach.
        //public TestArguments(
        //    [ArgumentConverter(typeof(NullReturningStringConverter))] string? constructorNullable,
        //    [ArgumentConverter(typeof(NullReturningStringConverter))] string constructorNonNullable,
        //    [ArgumentConverter(typeof(NullReturningIntConverter))] int constructorValueType,
        //    [ArgumentConverter(typeof(NullReturningIntConverter))] int? constructorNullableValueType)
        //{
        //    ConstructorNullable = constructorNullable;
        //    ConstructorNonNullable = constructorNonNullable;
        //    ConstructorValueType = constructorValueType;
        //    ConstructorNullableValueType = constructorNullableValueType;
        //}

        [CommandLineArgument("constructorNullable", Position = 0)]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public string? ConstructorNullable { get; set; }

        [CommandLineArgument("constructorNonNullable", Position = 1)]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public string ConstructorNonNullable { get; set; } = default!;

        [CommandLineArgument("constructorValueType", Position = 2)]
        [ArgumentConverter(typeof(NullReturningIntConverter))]
        public int ConstructorValueType { get; set; }

        [CommandLineArgument("constructorNullableValueType", Position = 3)]
        [ArgumentConverter(typeof(NullReturningIntConverter))]
        public int? ConstructorNullableValueType { get; set; }

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public string? Nullable { get; set; } = "NotNullDefaultValue";

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public string NonNullable { get; set; } = string.Empty;

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningIntConverter))]
        public int ValueType { get; set; }

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningIntConverter))]
        public int? NullableValueType { get; set; } = 42;

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public string[]? NonNullableArray { get; set; }

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningIntConverter))]
        public int[]? ValueArray { get; set; }

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public ICollection<string> NonNullableCollection { get; } = new List<string>();

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningIntConverter))]
        [MultiValueSeparator(";")]
        public ICollection<int> ValueCollection { get; } = new List<int>();

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public string?[]? NullableArray { get; set; }

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningIntConverter))]
        public string?[]? NullableValueArray { get; set; }

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public ICollection<string?> NullableCollection { get; } = new List<string?>();

        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public ICollection<int?> NullableValueCollection { get; } = new List<int?>();

        [CommandLineArgument]
        [KeyConverter(typeof(NullReturningStringConverter))]
        [ValueConverter(typeof(NullReturningStringConverter))]
        public Dictionary<string, string>? NonNullableDictionary { get; set; }

        [CommandLineArgument]
        [ValueConverter(typeof(NullReturningIntConverter))]
        public Dictionary<string, int>? ValueDictionary { get; set; }

        [CommandLineArgument]
        [ValueConverter(typeof(NullReturningStringConverter))]
        public IDictionary<string, string> NonNullableIDictionary { get; } = new Dictionary<string, string>();

        [CommandLineArgument]
        [KeyConverter(typeof(NullReturningStringConverter))]
        [ValueConverter(typeof(NullReturningIntConverter))]
        [MultiValueSeparator(";")]
        public IDictionary<string, int> ValueIDictionary { get; } = new Dictionary<string, int>();

        [CommandLineArgument]
        [KeyConverter(typeof(NullReturningStringConverter))]
        [ValueConverter(typeof(NullReturningStringConverter))]
        public Dictionary<string, string?>? NullableDictionary { get; set; }

        [CommandLineArgument]
        [KeyConverter(typeof(NullReturningStringConverter))]
        [ValueConverter(typeof(NullReturningIntConverter))]
        public Dictionary<string, int?>? NullableValueDictionary { get; set; }

        [CommandLineArgument]
        [KeyConverter(typeof(NullReturningStringConverter))]
        [ValueConverter(typeof(NullReturningStringConverter))]
        public IDictionary<string, string?> NullableIDictionary { get; } = new Dictionary<string, string?>();

        [CommandLineArgument]
        [KeyConverter(typeof(NullReturningStringConverter))]
        [ValueConverter(typeof(NullReturningIntConverter))]
        [MultiValueSeparator(";")]
        public IDictionary<string, int?> NullableValueIDictionary { get; } = new Dictionary<string, int?>();

        // This is an incorrect type converter (doesn't return KeyValuePair), but it doesn't
        // matter since it'll only be used to test null values.
        [CommandLineArgument]
        [ArgumentConverter(typeof(NullReturningStringConverter))]
        public Dictionary<string, string?>? InvalidDictionary { get; set; }
    }

}

#endif
