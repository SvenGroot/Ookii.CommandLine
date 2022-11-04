using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ookii.CommandLine.Terminal
{
    /// <summary>
    /// Provides helper methods for console Virtual Terminal sequences.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Virtual terminal sequences are used to add color to various aspects of the usage help,
    ///   if enabled by the <see cref="WriteUsageOptions.UseColor"/> property.
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
        ///   virtual terminal support when disposed or destructed. Use the <see cref="VirtualTerminalSupport.IsSupported"/>
        ///   property to check if virtual terminal sequences are supported.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   Do not write VT sequences to the console unless this method returns <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This function will return <see langword="true"/> if the specified stream is not
        ///   redirected, and the TERM environment variable is not set to "dumb". On Windows, 
        ///   enabling VT support has to succeed. On non-Windows platforms, VT support is assumed
        ///   if the TERM environment variable is defined.
        /// </para>
        /// <para>
        ///   For <see cref="StandardStream.Input"/>, this method does nothing and always returns
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
                var previousMode = NativeMethods.EnableVirtualTerminalSequences(stream, true);
                if (previousMode == null)
                {
                    return new VirtualTerminalSupport(false);
                }

                return new VirtualTerminalSupport(NativeMethods.GetStandardHandle(stream), previousMode.Value);
            }

            // Support is assumed on non-Windows platforms if TERM is set.
            return new VirtualTerminalSupport(term != null);
        }

        /// <summary>
        /// Enables color  for the console attached to the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="StandardStream"/> to enable color sequences for.</param>
        /// <returns>
        ///   An instance of the <see cref="VirtualTerminalSupport"/> class that will disable
        ///   virtual terminal support when disposed or destructed. Use the <see cref="VirtualTerminalSupport.IsSupported"/>
        ///   property to check if virtual terminal sequences are supported.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   If an environment variable named "NO_COLOR" exists, this function always returns
        ///   <see langword="false"/>. Otherwise, this function calls the <see cref="EnableVirtualTerminalSequences"/>
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
        /// Returns the virtual terminal sequence to the foreground or background color to an RGB
        /// color.
        /// </summary>
        /// <param name="color">The color to use.</param>
        /// <param name="foreground">
        ///   <see langword="true"/> to apply the color to the background; otherwise, it's applied
        ///   to the background.
        /// </param>
        /// <returns>A string with the virtual terminal sequence.</returns>
        public static string GetExtendedColor(Color color, bool foreground = true)
        {
            return FormattableString.Invariant($"{Escape}[{(foreground ? 38 : 48)};2;{color.R};{color.G};{color.B}m");
        }

        internal static int FindSequenceEnd(IEnumerable<char> value)
        {
            return value.First() switch
            {
                '[' => FindCsiEnd(value),
                ']' => FindOscEnd(value),
                '(' => 2,
                _ => 1,
            };
        }

        private static int FindCsiEnd(IEnumerable<char> value)
        {
            int index = 0;
            foreach (var ch in value.Skip(1))
            {
                if (!char.IsNumber(ch) && ch != ';' && ch != ' ')
                {
                    return index + 2;
                }

                ++index;
            }

            return -1;
        }

        private static int FindOscEnd(IEnumerable<char> value)
        {
            int index = 0;
            bool hasEscape = false;
            foreach (var ch in value.Skip(1))
            {
                if (ch == 0x7)
                {
                    return index + 2;
                }

                if (hasEscape && ch == '\\')
                {
                    return index + 2;
                }

                if (ch == Escape)
                {
                    hasEscape = true;
                }

                ++index;
            }

            return -1;
        }
    }
}
