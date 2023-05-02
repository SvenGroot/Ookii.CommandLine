// These tests don't apply to .Net Standard.
#if NET6_0_OR_GREATER

#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Ookii.CommandLine.Tests
{
    [TestClass]
    public class CommandLineParserNullableTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Avoid exception when testing reflection on argument types that also have the
            // GeneratedParseAttribute set.
            ParseOptions.AllowReflectionWithGeneratedParserDefault = true;
        }

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestAllowNull(ProviderKind kind)
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
        public void TestNonNullableConstructor(ProviderKind kind)
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
        public void TestNonNullableProperties(ProviderKind kind)
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
        public void TestNonNullableMultiValue(ProviderKind kind)
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
        public void TestNonNullableDictionary(ProviderKind kind)
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

#if NET7_0_OR_GREATER

        [TestMethod]
        [DynamicData(nameof(ProviderKinds), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void TestRequiredProperty(ProviderKind kind)
        {
            var parser = CommandLineParserTest.CreateParser<RequiredPropertyArguments>(kind);
            Assert.IsTrue(parser.GetArgument("Arg1")!.IsRequired);
            Assert.IsTrue(parser.GetArgument("Arg1")!.IsRequiredProperty);
            Assert.IsFalse(parser.GetArgument("Arg1")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("Foo")!.IsRequired);
            Assert.IsTrue(parser.GetArgument("Foo")!.IsRequiredProperty);
            Assert.IsTrue(parser.GetArgument("Foo")!.AllowNull);
            Assert.IsTrue(parser.GetArgument("Bar")!.IsRequired);
            Assert.IsTrue(parser.GetArgument("Bar")!.IsRequiredProperty);
            Assert.IsFalse(parser.GetArgument("Bar")!.AllowNull);
            var result = ExpectSuccess(parser, "-Arg1", "test", "-Foo", "foo", "-Bar", "42");
            Assert.AreEqual("test", result.Arg1);
            Assert.AreEqual("foo", result.Foo);
            CollectionAssert.AreEqual(new[] { 42 }, result.Bar);
            Assert.IsNull(result.Arg2);
        }

#endif

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

        private static T ExpectSuccess<T>(CommandLineParser<T> parser, params string[] args)
            where T : class
        {
            var result = parser.Parse(args);
            Assert.IsNotNull(result);
            return result;
        }

        public static string GetCustomDynamicDataDisplayName(MethodInfo methodInfo, object[] data)
            => $"{methodInfo.Name} ({data[0]})";


        public static IEnumerable<object[]> ProviderKinds
            => new[]
            {
                new object[] { ProviderKind.Reflection },
                new object[] { ProviderKind.Generated }
            };
    }
}

#endif
