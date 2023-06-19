# Utility types

Ookii.CommandLine comes with a few utilities that it uses internally, but which may be of use to
anyone writing console applications. These are the [`LineWrappingTextWriter`][] class and virtual
terminal support.

## LineWrappingTextWriter

The [`LineWrappingTextWriter`][] class is a [`TextWriter`][] implementation that allows you to write
text to another [`TextWriter`][], white-space wrapping the text at the specified line length, and
supporting hanging indents.

Ookii.CommandLine uses this class to wrap and indent error messages and usage help when writing to
the console.

The [`LineWrappingTextWriter`][] can be created to wrap any [`TextWriter`][] and with any line
length, using its constructor. If you use a line length of less than 1 or greater than
`ushort.MaxValue`, this is treated as an infinite length, and lines will not be wrapped. The
[`LineWrappingTextWriter`][] can still be used to create indented text if you use an unrestricted
line length.

Most of the time, you will probably want to use the [`LineWrappingTextWriter.ForConsoleOut()`][] or
[`LineWrappingTextWriter.ForConsoleError()`][] methods to create a writer for the standard output or
error streams, automatically wrapping at the console width.

> Both methods actually use `Console.WindowWidth - 1` for their maximum line length, because using
> [`Console.WindowWidth`][] exactly can lead to extra blank lines if a line is exactly the width of
> the console.

Lines will be wrapped at white-space characters only. If a line does not have a suitable place to
wrap, it will be wrapped at the maximum line length. You can temporarily disable line wrapping by
setting the [`LineWrappingTextWriter.Wrapping`][] property to [`WrappingMode.Disabled`][], or disable
breaking lines without a suitable place to wrap by setting it to [`WrappingMode.EnabledNoForce`][].

If you write virtual terminal sequences to a [`LineWrappingTextWriter`][], by default these will not
be included when calculating the length of the current line, so inserting VT sequences, e.g. for
colors, will not affect how the text is wrapped.

### Indentation

The [`LineWrappingTextWriter`][] class uses hanging indents, also called negative indents, where all
lines except the first one are indented. The indentation level can be set using the
[`LineWrappingTextWriter.Indent`][] property, which indicates the number of spaces to indent by.

When this property is set, it will apply to the next line that needs to be indented. The first line
of text, and any line after a blank line, is not indented. Indentation is applied both to lines that
were wrapped, and lines created by explicit new lines in the text.

You can change the [`Indent`][] property at any time to change the size of the indentation to use.

Additionally, you can use the [`LineWrappingTextWriter.ResetIndent()`][] method to indicate you do not
want to indent the current line, even if it didn't follow a blank line. Note that the [`ResetIndent()`][]
method will insert a line break if the current line is not empty.

For example:

```csharp
using var writer = LineWrappingTextWriter.ForConsoleOut();
writer.Indent = 4;
writer.WriteLine("The first line is not indented. This line is pretty long, so it'll probably be wrapped, and the wrapped portion will be indented.");
writer.WriteLine("A line after an explicit line break is also indented.");
writer.WriteLine();
writer.WriteLine("After a blank line, no indentation is used.");
writer.WriteLine("The next line is indented again.");
writer.ResetIndent();
writer.WriteLine("This line is not.");
writer.WriteLine("And this one is.");
```

This produces the following output:

```text
The first line is not indented. This line is pretty long, so it'll probably be wrapped, and the wrapped portion
    will be indented.
    A line after an explicit line break is also indented.

After a blank line, no indentation is used.
    The next line is indented again.
This line is not.
    And this one is.
```

## Virtual terminal support

Virtual terminal (VT) sequences are a method to manipulate the console utilized, supported by many
console applications on many operating systems. It is supported by the console host on recent
versions of Windows, by [Windows Terminal](https://learn.microsoft.com/windows/terminal/install),
and many console applications on other platforms.

A VT sequence consists of an escape character, followed by a string that specifies what action to
take. They can be used to set colors and other formatting options, but also to do things like move
the cursor.

Ookii.CommandLine uses VT sequences to add color to the usage help and error messages. To let you
customize the colors used by the [`UsageWriter`][], and to use color in your own console
applications, a few types are provided in the [`Ookii.CommandLine.Terminal`][] namespace.

The [`VirtualTerminal`][] class allows you to determine whether virtual terminal sequences are
supported, and to enable them. The [`UsageWriter`][] class uses this internally to enable color output
when possible.

The [`TextFormat`][] class provides a number of constants for the predefined background and foreground
colors and formats supported by the console, as well as a method to create a VT sequence for any
24-bit color. These can be used to change the default usage help colors, or to apply color to your
own text.

For example, you can use the following to write in color when supported:

```csharp
using var support = VirtualTerminal.EnableColor(StandardStream.Output);
if (support.IsSupported)
{
    Console.Write(TextFormat.ForegroundGreen + TextFormat.Underline);
}

Console.Write("This text is green and underlined.");
if (support.IsSupported)
{
    Console.Write(TextFormat.Default);
}

Console.WriteLine();
```

On Windows, VT support must be enabled for a process. In addition to checking for support, the
[`EnableVirtualTerminalSequences()`][] and [`EnableColor()`][] methods also enable it if necessary,
and they return a disposable type that will revert the console mode when disposed or garbage
collected. On other platforms, it only checks for support and disposing the returned instance does
nothing.

[`Console.WindowWidth`]: https://learn.microsoft.com/dotnet/api/system.console.windowwidth
[`EnableColor()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Terminal_VirtualTerminal_EnableColor.htm
[`EnableVirtualTerminalSequences()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_Terminal_VirtualTerminal_EnableVirtualTerminalSequences.htm
[`Indent`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_LineWrappingTextWriter_Indent.htm
[`LineWrappingTextWriter.ForConsoleError()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_ForConsoleError.htm
[`LineWrappingTextWriter.ForConsoleOut()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_ForConsoleOut.htm
[`LineWrappingTextWriter.Indent`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_LineWrappingTextWriter_Indent.htm
[`LineWrappingTextWriter.ResetIndent()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_ResetIndent.htm
[`LineWrappingTextWriter`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_LineWrappingTextWriter.htm
[`Ookii.CommandLine.Terminal`]: https://www.ookii.org/docs/commandline-4.0/html/N_Ookii_CommandLine_Terminal.htm
[`ResetIndent()`]: https://www.ookii.org/docs/commandline-4.0/html/M_Ookii_CommandLine_LineWrappingTextWriter_ResetIndent.htm
[`TextFormat`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Terminal_TextFormat.htm
[`TextWriter`]: https://learn.microsoft.com/dotnet/api/system.io.textwriter
[`UsageWriter`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_UsageWriter.htm
[`VirtualTerminal`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_Terminal_VirtualTerminal.htm
[`LineWrappingTextWriter.Wrapping`]: https://www.ookii.org/docs/commandline-4.0/html/P_Ookii_CommandLine_LineWrappingTextWriter_Wrapping.htm
[`WrappingMode.Disabled`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_WrappingMode.htm
[`WrappingMode.EnabledNoForce`]: https://www.ookii.org/docs/commandline-4.0/html/T_Ookii_CommandLine_WrappingMode.htm
