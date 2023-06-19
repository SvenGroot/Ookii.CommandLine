using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Terminal;
using System;
using System.Linq;

namespace Ookii.CommandLine.Tests;

[TestClass]
public class ParseOptionsTest
{
    [ClassInitialize]
    public static void TestFixtureSetup(TestContext context)
    {
        // In case other tests had changed this.
        ParseOptions.ForceReflectionDefault = false;
    }

    [TestMethod]
    public void TestConstructor()
    {
        var options = new ParseOptions();
        Assert.IsNull(options.AllowWhiteSpaceValueSeparator);
        Assert.IsNull(options.ArgumentNameComparison);
        Assert.IsNull(options.ArgumentNamePrefixes);
        Assert.IsNull(options.ArgumentNameTransform);
        Assert.IsNull(options.AutoHelpArgument);
        Assert.IsNull(options.AutoPrefixAliases);
        Assert.IsNull(options.AutoVersionArgument);
        Assert.IsNull(options.Culture);
        Assert.IsNull(options.DefaultValueDescriptions);
        Assert.IsNull(options.DuplicateArguments);
        Assert.IsNull(options.Error);
        Assert.AreEqual(TextFormat.ForegroundRed, options.ErrorColor);
        Assert.IsFalse(options.ForceReflection);
        Assert.IsFalse(options.IsPosix);
        Assert.IsNull(options.LongArgumentNamePrefix);
        Assert.IsNull(options.Mode);
        Assert.IsNull(options.NameValueSeparators);
        Assert.AreEqual(UsageHelpRequest.SyntaxOnly, options.ShowUsageOnError);
        Assert.IsNotNull(options.StringProvider);
        Assert.IsNotNull(options.UsageWriter);
        Assert.IsNull(options.UseErrorColor);
        Assert.IsNull(options.ValueDescriptionTransform);
        Assert.AreEqual(TextFormat.ForegroundYellow, options.WarningColor);

        // Defaults
        Assert.IsTrue(options.AllowWhiteSpaceValueSeparatorOrDefault);
        CollectionAssert.AreEqual(CommandLineParser.GetDefaultArgumentNamePrefixes(), options.ArgumentNamePrefixesOrDefault.ToArray());
        Assert.AreEqual(NameTransform.None, options.ArgumentNameTransformOrDefault);
        Assert.IsTrue(options.AutoHelpArgumentOrDefault);
        Assert.IsTrue(options.AutoPrefixAliasesOrDefault);
        Assert.IsTrue(options.AutoVersionArgumentOrDefault);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.ArgumentNameComparisonOrDefault);
        Assert.AreEqual(ErrorMode.Error, options.DuplicateArgumentsOrDefault);
        Assert.AreEqual("--", options.LongArgumentNamePrefixOrDefault);
        Assert.AreEqual(ParsingMode.Default, options.ModeOrDefault);
        CollectionAssert.AreEqual(new[] { ':', '=' }, options.NameValueSeparatorsOrDefault.ToArray());
        Assert.AreEqual(NameTransform.None, options.ValueDescriptionTransformOrDefault);
    }

    [TestMethod]
    public void TestIsPosix()
    {
        var options = new ParseOptions()
        {
            IsPosix = true
        };

        Assert.IsTrue(options.IsPosix);
        Assert.AreEqual(ParsingMode.LongShort, options.Mode);
        Assert.AreEqual(StringComparison.InvariantCulture, options.ArgumentNameComparison);
        Assert.AreEqual(NameTransform.DashCase, options.ArgumentNameTransform);
        Assert.AreEqual(NameTransform.DashCase, options.ValueDescriptionTransform);
        options.ArgumentNameComparison = StringComparison.CurrentCultureIgnoreCase;
        Assert.IsFalse(options.IsPosix);
        options.ArgumentNameComparison = StringComparison.CurrentCulture;
        Assert.IsTrue(options.IsPosix);

        options.IsPosix = false;
        Assert.AreEqual(ParsingMode.Default, options.Mode);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.ArgumentNameComparison);
        Assert.AreEqual(NameTransform.None, options.ArgumentNameTransform);
        Assert.AreEqual(NameTransform.None, options.ValueDescriptionTransform);

        options = new ParseOptions()
        {
            Mode = ParsingMode.LongShort,
            ArgumentNameComparison = StringComparison.InvariantCulture,
            ArgumentNameTransform = NameTransform.DashCase,
            ValueDescriptionTransform = NameTransform.DashCase
        };

        Assert.IsTrue(options.IsPosix);
    }

    [TestMethod]
    public void TestMerge()
    {
        var options = new ParseOptions();
        var attribute = new ParseOptionsAttribute();
        options.Merge(attribute);
        Assert.IsTrue(options.AllowWhiteSpaceValueSeparator);
        Assert.IsNull(options.ArgumentNamePrefixes);
        Assert.AreEqual(NameTransform.None, options.ArgumentNameTransform);
        Assert.IsTrue(options.AutoHelpArgument);
        Assert.IsTrue(options.AutoPrefixAliases);
        Assert.IsTrue(options.AutoVersionArgument);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.ArgumentNameComparison);
        Assert.AreEqual(ErrorMode.Error, options.DuplicateArguments);
        Assert.IsFalse(options.IsPosix);
        Assert.IsNull(options.LongArgumentNamePrefix);
        Assert.AreEqual(ParsingMode.Default, options.Mode);
        Assert.IsNull(options.NameValueSeparators);
        Assert.AreEqual(NameTransform.None, options.ValueDescriptionTransform);

        options = new ParseOptions();
        attribute = new ParseOptionsAttribute()
        {
            CaseSensitive = true,
            ArgumentNamePrefixes = new[] { "+", "++" },
            LongArgumentNamePrefix = "+++",
        };

        options.Merge(attribute);
        Assert.AreEqual(StringComparison.InvariantCulture, options.ArgumentNameComparison);
        CollectionAssert.AreEqual(new[] { "+", "++" }, options.ArgumentNamePrefixes!.ToArray());
        Assert.AreEqual("+++", options.LongArgumentNamePrefix);

        options = new ParseOptions();
        attribute = new ParseOptionsAttribute()
        {
            IsPosix = true,
        };

        options.Merge(attribute);
        Assert.IsTrue(options.IsPosix);
        Assert.AreEqual(ParsingMode.LongShort, options.Mode);
        Assert.AreEqual(StringComparison.InvariantCulture, options.ArgumentNameComparison);
        Assert.AreEqual(NameTransform.DashCase, options.ArgumentNameTransform);
        Assert.AreEqual(NameTransform.DashCase, options.ValueDescriptionTransform);
    }
}
