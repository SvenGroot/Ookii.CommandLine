using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Terminal;
using System;

namespace Ookii.CommandLine.Tests;

[TestClass]
public class ParseOptionsAttributeTest
{
    [TestMethod]
    public void TestConstructor()
    {
        var options = new ParseOptionsAttribute();
        Assert.IsTrue(options.AllowWhiteSpaceValueSeparator);
        Assert.IsNull(options.ArgumentNamePrefixes);
        Assert.AreEqual(NameTransform.None, options.ArgumentNameTransform);
        Assert.IsTrue(options.AutoHelpArgument);
        Assert.IsTrue(options.AutoPrefixAliases);
        Assert.IsTrue(options.AutoVersionArgument);
        Assert.IsFalse(options.CaseSensitive);
        Assert.AreEqual(ErrorMode.Error, options.DuplicateArguments);
        Assert.IsFalse(options.IsPosix);
        Assert.IsNull(options.LongArgumentNamePrefix);
        Assert.AreEqual(ParsingMode.Default, options.Mode);
        Assert.IsNull(options.NameValueSeparators);
        Assert.AreEqual(NameTransform.None, options.ValueDescriptionTransform);
    }

    [TestMethod]
    public void TestIsPosix()
    {
        var options = new ParseOptionsAttribute()
        {
            IsPosix = true
        };

        Assert.IsTrue(options.IsPosix);
        Assert.AreEqual(ParsingMode.LongShort, options.Mode);
        Assert.IsTrue(options.CaseSensitive);
        Assert.AreEqual(NameTransform.DashCase, options.ArgumentNameTransform);
        Assert.AreEqual(NameTransform.DashCase, options.ValueDescriptionTransform);
        options.CaseSensitive = false;
        Assert.IsFalse(options.IsPosix);
        options.CaseSensitive = true;
        Assert.IsTrue(options.IsPosix);

        options.IsPosix = false;
        Assert.AreEqual(ParsingMode.Default, options.Mode);
        Assert.IsFalse(options.CaseSensitive);
        Assert.AreEqual(NameTransform.None, options.ArgumentNameTransform);
        Assert.AreEqual(NameTransform.None, options.ValueDescriptionTransform);

        options = new ParseOptionsAttribute()
        {
            Mode = ParsingMode.LongShort,
            CaseSensitive = true,
            ArgumentNameTransform = NameTransform.DashCase,
            ValueDescriptionTransform = NameTransform.DashCase
        };

        Assert.IsTrue(options.IsPosix);
    }
}
