using System;
using System.Runtime.InteropServices;

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
    ///   virtual terminal support when disposed or destructed. Use the <see cref="VirtualTerminalSupport.IsSupported" qualifyHint="true"/>
    ///   property to check if virtual terminal sequences are supported.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   Virtual terminal sequences are supported if the specified stream is not redirected,
    ///   and the TERM environment variable is not set to "dumb". On Windows, enabling VT
    ///   support has to succeed. On non-Windows platforms, VT support is assumed if the TERM
    ///   environment variable is defined.
    /// </para>
    /// <para>
    ///   For <see cref="StandardStream.Input" qualifyHint="true"/>, this method does nothing and always returns
    ///   <see langword="false"/>.
    /// </para>
    /// </remarks>
    public static VirtualTerminalSupport EnableVirtualTerminalSequences(StandardStream stream)
    {
        bool supported = stream switch
        {
            StandardStream.Output => !Console.IsOutputRedirected,
            StandardStream.Error => !Console.IsErrorRedirected,
            _ => false,
        };

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
            var (enabled, previousMode) = NativeMethods.EnableVirtualTerminalSequences(stream, true);
            if (!enabled)
            {
                return new VirtualTerminalSupport(false);
            }

            if (previousMode is NativeMethods.ConsoleModes mode)
            {
                return new VirtualTerminalSupport(NativeMethods.GetStandardHandle(stream), mode);
            }

            // Support was already enabled externally, so don't change the console mode on dispose.
            return new VirtualTerminalSupport(true);
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
}
