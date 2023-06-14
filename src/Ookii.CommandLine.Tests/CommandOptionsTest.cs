using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Terminal;
using System;

namespace Ookii.CommandLine.Tests;

[TestClass]
public class CommandOptionsTest
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
        var options = new CommandOptions();
        // Values from ParseOptions that are the same.
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

        // Properties defined by CommandOptions itself
        Assert.IsTrue(options.AutoCommandPrefixAliases);
        Assert.IsTrue(options.AutoVersionCommand);
        Assert.IsNull(options.CommandFilter);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.CommandNameComparison);
        Assert.AreEqual(NameTransform.None, options.CommandNameTransform);
        Assert.IsNull(options.ParentCommand);
        Assert.AreEqual("Command", options.StripCommandNameSuffix);
    }

    [TestMethod]
    public void TestIsPosix()
    {
        var options = new CommandOptions()
        {
            IsPosix = true
        };

        Assert.IsTrue(options.IsPosix);
        Assert.AreEqual(ParsingMode.LongShort, options.Mode);
        Assert.AreEqual(StringComparison.InvariantCulture, options.ArgumentNameComparison);
        Assert.AreEqual(NameTransform.DashCase, options.ArgumentNameTransform);
        Assert.AreEqual(NameTransform.DashCase, options.ValueDescriptionTransform);
        Assert.AreEqual(StringComparison.InvariantCulture, options.CommandNameComparison);
        Assert.AreEqual(NameTransform.DashCase, options.CommandNameTransform);
        options.CommandNameComparison = StringComparison.CurrentCultureIgnoreCase;
        Assert.IsFalse(options.IsPosix);
        options.CommandNameComparison = StringComparison.CurrentCulture;
        Assert.IsTrue(options.IsPosix);

        options.IsPosix = false;
        Assert.AreEqual(ParsingMode.Default, options.Mode);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.ArgumentNameComparison);
        Assert.AreEqual(NameTransform.None, options.ArgumentNameTransform);
        Assert.AreEqual(NameTransform.None, options.ValueDescriptionTransform);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, options.CommandNameComparison);
        Assert.AreEqual(NameTransform.None, options.CommandNameTransform);

        options = new CommandOptions()
        {
            Mode = ParsingMode.LongShort,
            ArgumentNameComparison = StringComparison.InvariantCulture,
            ArgumentNameTransform = NameTransform.DashCase,
            ValueDescriptionTransform = NameTransform.DashCase,
            CommandNameComparison = StringComparison.InvariantCulture,
            CommandNameTransform = NameTransform.DashCase,
        };

        Assert.IsTrue(options.IsPosix);
    }
}
