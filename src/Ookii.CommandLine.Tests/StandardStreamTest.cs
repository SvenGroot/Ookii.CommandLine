using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ookii.CommandLine.Terminal;
using System;
using System.IO;

namespace Ookii.CommandLine.Tests;

[TestClass]
public class StandardStreamTest
{
    [TestMethod]
    public void TestGetWriter()
    {
        Assert.AreSame(Console.Out, StandardStream.Output.GetWriter());
        Assert.AreSame(Console.Error, StandardStream.Error.GetWriter());
        Assert.ThrowsException<ArgumentException>(() => StandardStream.Input.GetWriter());
    }

    [TestMethod]
    public void TestOpenStream()
    {
        using var output = StandardStream.Output.OpenStream();
        using var error = StandardStream.Error.OpenStream();
        using var input = StandardStream.Input.OpenStream();
        Assert.AreNotSame(output, input);
        Assert.AreNotSame(output, error);
        Assert.AreNotSame(error, input);
    }

    [TestMethod]
    public void TestGetStandardStream()
    {
        Assert.AreEqual(StandardStream.Output, Console.Out.GetStandardStream());
        Assert.AreEqual(StandardStream.Error, Console.Error.GetStandardStream());
        Assert.AreEqual(StandardStream.Input, Console.In.GetStandardStream());
        using (var writer = new StringWriter())
        {
            Assert.IsNull(writer.GetStandardStream());
        }

        using (var writer = LineWrappingTextWriter.ForConsoleOut())
        {
            Assert.AreEqual(StandardStream.Output, writer.GetStandardStream());
        }

        using (var writer = LineWrappingTextWriter.ForStringWriter())
        {
            Assert.IsNull(writer.GetStandardStream());
        }

        using (var reader = new StringReader("foo"))
        {
            Assert.IsNull(reader.GetStandardStream());
        }
    }
}
