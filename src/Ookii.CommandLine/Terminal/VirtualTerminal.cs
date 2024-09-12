using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Console;

namespace Ookii.CommandLine.Terminal;

/// <summary>
/// Provides helper methods for console Virtual Terminal sequences.
/// </summary>
/// <remarks>
/// <para>
///   Virtual terminal sequences are used to add color to various aspects of the usage help,
///   if enabled by the <see cref="UsageWriter"/> class.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public static class VirtualTerminal
{
    /// <summary>
    /// The escape character that begins all Virtual Terminal sequences.
    /// </summary>
    public const char Escape = '\x1b';

    /// <summary>
    /// Enables virtual terminal sequences for the console attached to the specified stream.
    /// </summary>
    /// <param name="stream">The <see cref="StandardStream"/> to enable VT sequences for.</param>
    /// <returns>
    ///   An instance of the <see cref="VirtualTerminalSupport"/> class that will disable
    ///   virtual terminal support when disposed or finalized. Use the
    ///   <see cref="VirtualTerminalSupport.IsSupported" qualifyHint="true"/> property to check if
    ///   virtual terminal sequences are supported.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   Virtual terminal sequences are supported if the specified stream is not redirected,
    ///   and the TERM environment variable is not set to "dumb". On Windows, enabling VT
    ///   support has to succeed. On non-Windows platforms, VT support is assumed if the TERM
    ///   environment variable is defined.
    /// </para>
    /// <para>
    ///   If you also want to check for a NO_COLOR environment variable, use the
    ///   <see cref="EnableColor"/> method instead.
    /// </para>
    /// <para>
    ///   For <see cref="StandardStream.Input" qualifyHint="true"/>, this method does nothing and
    ///   always returns <see langword="false"/>.
    /// </para>
    /// </remarks>
    public static VirtualTerminalSupport EnableVirtualTerminalSequences(StandardStream stream)
    {
        bool supported = stream != StandardStream.Input && !stream.IsRedirected();
        if (!supported)
        {
            return new VirtualTerminalSupport(false);
        }

        var term = Environment.GetEnvironmentVariable("TERM");
        if (term == "dumb")
        {
            return new VirtualTerminalSupport(false);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var result = SetVirtualTerminalSequences(stream, true);
            if (result.NeedRestore)
            {
                Debug.Assert(result.Supported);
                return new VirtualTerminalSupport(stream);
            }

            // VT sequences are either not supported, or were already enabled so we don't need to
            // disable them.
            return new VirtualTerminalSupport(result.Supported);
        }

        // Support is assumed on non-Windows platforms if TERM is set.
        return new VirtualTerminalSupport(term != null);
    }

    /// <summary>
    /// Enables color support using virtual terminal sequences for the console attached to the
    /// specified stream.
    /// </summary>
    /// <param name="stream">The <see cref="StandardStream"/> to enable color sequences for.</param>
    /// <returns>
    ///   An instance of the <see cref="VirtualTerminalSupport"/> class that will disable
    ///   virtual terminal support when disposed or destructed. Use the <see cref="VirtualTerminalSupport.IsSupported" qualifyHint="true"/>
    ///   property to check if virtual terminal sequences are supported.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If an environment variable named "NO_COLOR" exists, this function will not enable VT
    ///   sequences. Otherwise, this function calls the <see cref="EnableVirtualTerminalSequences"/>
    ///   method and returns its result.
    /// </para>
    /// </remarks>
    public static VirtualTerminalSupport EnableColor(StandardStream stream)
    {
        if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
        {
            return new VirtualTerminalSupport(false);
        }

        return EnableVirtualTerminalSequences(stream);
    }

    /// <summary>
    /// Writes a line to the standard output stream which, if virtual terminal sequences are
    /// supported, will use the specified formatting.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="textFormat">The formatting that should be applied to the text.</param>
    /// <param name="reset">
    /// The VT sequence that should be used to undo the formatting, or <see langword="null"/> to
    /// use <see cref="TextFormat.Default" qualifyHint="true"/>.
    /// </param>
    /// <remarks>
    /// <para>
    ///   This method takes care of checking whether VT sequences are supported by using the
    ///   <see cref="EnableColor"/> method, and on Windows, will reset the console mode afterwards
    ///   if needed.
    /// </para>
    /// <para>
    ///   The <paramref name="textFormat"/> and <paramref name="reset"/> parameters will be ignored
    ///   if the standard output stream does not support VT sequences. In that case, the value of
    ///   <paramref name="text"/> will be written without formatting.
    /// </para>
    /// <para>
    ///   This method uses the <see cref="LineWrappingTextWriter"/> to ensure that the text is
    ///   properly white-space wrapped at the console width.
    /// </para>
    /// </remarks>
    public static void WriteLineFormatted(string text, TextFormat textFormat, TextFormat? reset = null)
        => WriteLineFormatted(StandardStream.Output, text, textFormat, reset ?? TextFormat.Default);

    /// <summary>
    /// Writes a line to the standard error stream which, if virtual terminal sequences are
    /// supported, will use the specified formatting.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="textFormat">
    /// The formatting that should be applied to the text, or <see langword="null"/> to use
    /// <see cref="TextFormat.ForegroundRed" qualifyHint="true"/>.
    /// </param>
    /// <param name="reset">
    /// The VT sequence that should be used to undo the formatting, or <see langword="null"/> to
    /// use <see cref="TextFormat.Default" qualifyHint="true"/>.
    /// </param>
    /// <remarks>
    /// <para>
    ///   This method takes care of checking whether VT sequences are supported by using the
    ///   <see cref="EnableColor"/> method, and on Windows, will reset the console mode afterwards
    ///   if needed.
    /// </para>
    /// <para>
    ///   The <paramref name="textFormat"/> and <paramref name="reset"/> parameters will be ignored
    ///   if the standard error stream does not support VT sequences. In that case, the value of
    ///   <paramref name="text"/> will be written without formatting.
    /// </para>
    /// <para>
    ///   This method uses the <see cref="LineWrappingTextWriter"/> to ensure that the text is
    ///   properly white-space wrapped at the console width.
    /// </para>
    /// </remarks>
    public static void WriteLineErrorFormatted(string text, TextFormat? textFormat = null, TextFormat? reset = null)
        => WriteLineFormatted(StandardStream.Error, text, textFormat ?? TextFormat.ForegroundRed, reset ?? TextFormat.Default);

    // Returns the index of the character after the end of the sequence.
    internal static int FindSequenceEnd(ReadOnlySpan<char> value, ref StringSegmentType type)
    {
        if (value.Length == 0)
        {
            return -1;
        }

        return type switch
        {
            StringSegmentType.PartialFormattingUnknown => value[0] switch
            {
                '[' => FindCsiEnd(value, ref type),
                ']' => FindOscEnd(value, ref type),
                // If the character after ( isn't present, we haven't found the end yet.
                '(' => value.Length > 1 ? 1 : -1,
                _ => 0,
            },
            StringSegmentType.PartialFormattingSimple => value.Length > 0 ? 0 : -1,
            StringSegmentType.PartialFormattingCsi => FindCsiEndPartial(value, ref type),
            StringSegmentType.PartialFormattingOsc or StringSegmentType.PartialFormattingOscWithEscape => FindOscEndPartial(value, ref type),
            _ => throw new ArgumentException("Invalid type for this operation.", nameof(type)),
        };
    }

    private static void WriteLineFormatted(StandardStream stream, string text, TextFormat textFormat, TextFormat reset)
    {
        using var writer = LineWrappingTextWriter.ForStandardStream(stream);
        using var support = EnableColor(stream);
        if (support.IsSupported)
        {
            writer.Write(textFormat);
        }

        writer.Write(text);
        if (support.IsSupported)
        {
            writer.Write(reset);
        }

        writer.WriteLine();
    }

    private static int FindCsiEnd(ReadOnlySpan<char> value, ref StringSegmentType type)
    {
        int result = FindCsiEndPartial(value.Slice(1), ref type);
        return result < 0 ? result : result + 1;
    }

    private static int FindCsiEndPartial(ReadOnlySpan<char> value, ref StringSegmentType type)
    {
        int index = 0;
        foreach (var ch in value)
        {
            if (!char.IsNumber(ch) && ch != ';' && ch != ' ')
            {
                return index;
            }

            ++index;
        }

        type = StringSegmentType.PartialFormattingCsi;
        return -1;
    }

    private static int FindOscEnd(ReadOnlySpan<char> value, ref StringSegmentType type)
    {
        int result = FindOscEndPartial(value.Slice(1), ref type);
        return result < 0 ? result : result + 1;
    }

    private static int FindOscEndPartial(ReadOnlySpan<char> value, ref StringSegmentType type)
    {
        int index = 0;
        bool hasEscape = type == StringSegmentType.PartialFormattingOscWithEscape;
        foreach (var ch in value)
        {
            if (ch == 0x7)
            {
                return index;
            }

            if (hasEscape)
            {
                if (ch == '\\')
                {
                    return index;
                }

                hasEscape = false;
            }

            if (ch == Escape)
            {
                hasEscape = true;
            }

            ++index;
        }

        type = hasEscape ? StringSegmentType.PartialFormattingOscWithEscape : StringSegmentType.PartialFormattingOsc;
        return -1;
    }

    internal static (bool Supported, bool NeedRestore) SetVirtualTerminalSequences(StandardStream stream, bool enable)
    {
        if (stream == StandardStream.Input)
        {
            throw new ArgumentException(Properties.Resources.InvalidStandardStream, nameof(stream));
        }

        // Dispose should not close the handle here, but use it anyway.
        using var handle = GetStandardHandle(stream);
        if (handle.IsInvalid)
        {
            return (false, false);
        }

        if (!PInvoke.GetConsoleMode(handle, out var mode))
        {
            return (false, false);
        }

        var oldMode = mode;
        if (enable)
        {
            mode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        }
        else
        {
            mode &= ~CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        }

        if (oldMode == mode)
        {
            return (true, false);
        }

        if (!PInvoke.SetConsoleMode(handle, mode))
        {
            return (false, false);
        }

        return (true, true);
    }

    private static SafeFileHandle GetStandardHandle(StandardStream stream)
    {
        var stdHandle = stream switch
        {
            StandardStream.Output => STD_HANDLE.STD_OUTPUT_HANDLE,
            StandardStream.Input => STD_HANDLE.STD_INPUT_HANDLE,
            StandardStream.Error => STD_HANDLE.STD_ERROR_HANDLE,
            _ => throw new ArgumentException(Properties.Resources.InvalidStandardStream, nameof(stream)),
        };

        // Generated function uses ownsHandle: false so the standard handle is not closed, as
        // expected.
        return PInvoke.GetStdHandle_SafeHandle(stdHandle);
    }
}
