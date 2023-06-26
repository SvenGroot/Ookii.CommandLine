using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests;

[TestClass]
public class TextFormatTest
{
    [TestMethod]
    public void TestDefault()
    {
        var value = new TextFormat();
        Assert.AreEqual("", value.Value);

        var value2 = default(TextFormat);
        Assert.AreEqual("", value2.Value);
    }

    [TestMethod]
    public void TestAddition()
    {
        var value = TextFormat.ForegroundRed + TextFormat.BackgroundGreen;
        Assert.AreEqual("\x1b[31m\x1b[42m", value.Value);
    }

    [TestMethod]
    public void TestEquality()
    {
        Assert.AreEqual(TextFormat.ForegroundRed, TextFormat.ForegroundRed);
        Assert.AreNotEqual(TextFormat.ForegroundGreen, TextFormat.ForegroundRed);
        var value1 = TextFormat.ForegroundRed;
        var value2 = TextFormat.ForegroundRed;
        Assert.IsTrue(value1 == value2);
        Assert.IsFalse(value1 != value2);
        value2 = TextFormat.ForegroundGreen;
        Assert.IsFalse(value1 == value2);
        Assert.IsTrue(value1 != value2);
    }
}
