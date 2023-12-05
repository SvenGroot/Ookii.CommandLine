using System;

namespace Ookii.CommandLine.Tests;

internal class CustomUsageWriter : UsageWriter
{
    public CustomUsageWriter() { }

    public CustomUsageWriter(LineWrappingTextWriter writer) : base(writer) { }

    protected override void WriteParserUsageFooter()
    {
        WriteLine("This is a custom footer.");
    }

    protected override void WriteCommandListUsageFooter()
    {
        base.WriteCommandListUsageFooter();
        WriteLine("This is the command list footer.");
    }
}
