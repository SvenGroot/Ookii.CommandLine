using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
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

        #region Nested Types

        /// <summary>
        /// Represents one of the standard console stream.
        /// </summary>
        public enum StandardStream
        {
            /// <summary>
            /// The standard output stream.
            /// </summary>
            Output,
            /// <summary>
            /// The standard input stream.
            /// </summary>
            Input,
            /// <summary>
            /// The standard error stream.
            /// </summary>
            Error
        }

        /// <summary>
        /// Provides constants for various VT sequences that control text format.
        /// </summary>
        public static class TextFormat
        {
            /// <summary>
            /// Resets the text format to the settings before modification.
            /// </summary>
            public const string Default = "\x1b[0m";
            /// <summary>
            /// Applies the brightness/intensity flag to the foreground color.
            /// </summary>
            public const string BoldBright = "\x1b[1m";
            /// <summary>
            /// Removes the brightness/intensity flag to the foreground color.
            /// </summary>
            public const string NoBoldBright = "\x1b[22m";
            /// <summary>
            /// Adds underline.
            /// </summary>
            public const string Underline = "\x1b[4m";
            /// <summary>
            /// Removes underline.
            /// </summary>
            public const string NoUnderline = "\x1b[24m";
            /// <summary>
            /// Swaps foreground and background colors.
            /// </summary>
            public const string Negative = "\x1b[7m";
            /// <summary>
            /// Returns foreground and background colors to normal.
            /// </summary>
            public const string Positive = "\x1b[27m";
            /// <summary>
            /// Sets the foreground color to Black.
            /// </summary>
            public const string ForegroundBlack = "\x1b[30m";
            /// <summary>
            /// Sets the foreground color to Red.
            /// </summary>
            public const string ForegroundRed = "\x1b[31m";
            /// <summary>
            /// Sets the foreground color to Green.
            /// </summary>
            public const string ForegroundGreen = "\x1b[32m";
            /// <summary>
            /// Sets the foreground color to Yellow.
            /// </summary>
            public const string ForegroundYellow = "\x1b[33m";
            /// <summary>
            /// Sets the foreground color to Blue.
            /// </summary>
            public const string ForegroundBlue = "\x1b[34m";
            /// <summary>
            /// Sets the foreground color to Magenta.
            /// </summary>
            public const string ForegroundMagenta = "\x1b[35m";
            /// <summary>
            /// Sets the foreground color to Cyan.
            /// </summary>
            public const string ForegroundCyan = "\x1b[36m";
            /// <summary>
            /// Sets the foreground color to White.
            /// </summary>
            public const string ForegroundWhite = "\x1b[37m";
            /// <summary>
            /// Sets the foreground color to Default.
            /// </summary>
            public const string ForegroundDefault = "\x1b[39m";
            /// <summary>
            /// Sets the background color to Black.
            /// </summary>
            public const string BackgroundBlack = "\x1b[40m";
            /// <summary>
            /// Sets the background color to Red.
            /// </summary>
            public const string BackgroundRed = "\x1b[41m";
            /// <summary>
            /// Sets the background color to Green.
            /// </summary>
            public const string BackgroundGreen = "\x1b[42m";
            /// <summary>
            /// Sets the background color to Yellow.
            /// </summary>
            public const string BackgroundYellow = "\x1b[43m";
            /// <summary>
            /// Sets the background color to Blue.
            /// </summary>
            public const string BackgroundBlue = "\x1b[44m";
            /// <summary>
            /// Sets the background color to Magenta.
            /// </summary>
            public const string BackgroundMagenta = "\x1b[45m";
            /// <summary>
            /// Sets the background color to Cyan.
            /// </summary>
            public const string BackgroundCyan = "\x1b[46m";
            /// <summary>
            /// Sets the background color to White.
            /// </summary>
            public const string BackgroundWhite = "\x1b[47m";
            /// <summary>
            /// Sets the background color to Default.
            /// </summary>
            public const string BackgroundDefault = "\x1b[49m";
            /// <summary>
            /// Sets the foreground color to bright Black.
            /// </summary>
            public const string BrightForegroundBlack = "\x1b[90m";
            /// <summary>
            /// Sets the foreground color to bright Red.
            /// </summary>
            public const string BrightForegroundRed = "\x1b[91m";
            /// <summary>
            /// Sets the foreground color to bright Green.
            /// </summary>
            public const string BrightForegroundGreen = "\x1b[92m";
            /// <summary>
            /// Sets the foreground color to bright Yellow.
            /// </summary>
            public const string BrightForegroundYellow = "\x1b[93m";
            /// <summary>
            /// Sets the foreground color to bright Blue.
            /// </summary>
            public const string BrightForegroundBlue = "\x1b[94m";
            /// <summary>
            /// Sets the foreground color to bright Magenta.
            /// </summary>
            public const string BrightForegroundMagenta = "\x1b[95m";
            /// <summary>
            /// Sets the foreground color to bright Cyan.
            /// </summary>
            public const string BrightForegroundCyan = "\x1b[96m";
            /// <summary>
            /// Sets the foreground color to bright White.
            /// </summary>
            public const string BrightForegroundWhite = "\x1b[97m";
            /// <summary>
            /// Sets the background color to bright Black.
            /// </summary>
            public const string BrightBackgroundBlack = "\x1b[100m";
            /// <summary>
            /// Sets the background color to bright Red.
            /// </summary>
            public const string BrightBackgroundRed = "\x1b[101m";
            /// <summary>
            /// Sets the background color to bright Green.
            /// </summary>
            public const string BrightBackgroundGreen = "\x1b[102m";
            /// <summary>
            /// Sets the background color to bright Yellow.
            /// </summary>
            public const string BrightBackgroundYellow = "\x1b[103m";
            /// <summary>
            /// Sets the background color to bright Blue.
            /// </summary>
            public const string BrightBackgroundBlue = "\x1b[104m";
            /// <summary>
            /// Sets the background color to bright Magenta.
            /// </summary>
            public const string BrightBackgroundMagenta = "\x1b[105m";
            /// <summary>
            /// Sets the background color to bright Cyan.
            /// </summary>
            public const string BrightBackgroundCyan = "\x1b[106m";
            /// <summary>
            /// Sets the background color to bright White.
            /// </summary>
            public const string BrightBackgroundWhite = "\x1b[107m";
        }

        #endregion

        /// <summary>
        /// The escape character that begins all Virtual Terminal sequences.
        /// </summary>
        public const char Escape = '\x1b';

        /// <summary>
        /// Enables virtual terminal sequences for the console attached to the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="StandardStream"/> to enable VT sequences for.</param>
        /// <returns>
        ///   <see langword="true"/> if VT sequences are supported and were successfully enabled;
        ///   otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   Do not write VT sequences to the console unless this method returns <see langword="true"/>.
        /// </para>
        /// <para>
        ///   This function will return <see langword="true"/> if the specified stream is not
        ///   redirected, and, on Windows, if enabling VT support was successful. On non-Windows
        ///   platforms, VT support is assumed if the output is not redirected.
        /// </para>
        /// <para>
        ///   For <see cref="StandardStream.Input"/>, this method does nothing and always returns
        ///   <see langword="false"/>.
        /// </para>
        /// </remarks>
        public static bool EnableVirtualTerminalSequences(StandardStream stream)
        {
            bool supported = stream switch
            {
                StandardStream.Output => !Console.IsOutputRedirected,
                StandardStream.Error => !Console.IsErrorRedirected,
                _ => false,
            };

            if (!supported)
                return false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return NativeMethods.EnableVirtualTerminalSequences(stream, true);

            // Support is assumed on non-Windows platforms.
            return true;
        }

        /// <summary>
        /// Disables virtual terminal sequence support for the console attached to the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="StandardStream"/> to disable VT sequences for.</param>
        /// <remarks>
        /// <para>
        ///   On Windows, this function will disable virtual terminal sequences. No error is thrown
        ///   if it fails.
        /// </para>
        /// <para>
        ///   On other platforms, this function does nothing.
        /// </para>
        /// </remarks>
        public static void DisableVirtualTerminalSequences(StandardStream stream)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                NativeMethods.EnableVirtualTerminalSequences(stream, false);
        }

        /// <summary>
        /// Enables color  for the console attached to the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="StandardStream"/> to enable color sequences for.</param>
        /// <returns>
        ///   <see langword="true"/> if color is requested, VT sequences are supported and were
        ///   successfully enabled; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   If an environment variable named "NO_COLOR" exists, this function always returns
        ///   <see langword="false"/>. Otherwise, this function calls the <see cref="EnableVirtualTerminalSequences"/>
        ///   method and returns its result.
        /// </para>
        /// </remarks>
        public static bool EnableColor(StandardStream stream)
        {
            if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
                return false;

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
                '[' => FindCsiEnd(value.Skip(1)) + 1,
                ']' => FindOscEnd(value.Skip(1)) + 1,
                '(' => 2,
                _ => 1,
            };
        }

        private static int FindCsiEnd(IEnumerable<char> value)
        {
            int index = 0;
            foreach (var ch in value)
            {
                if (!char.IsNumber(ch) && ch != ';' && ch != ' ')
                    break;

                ++index;
            }

            return index + 1;
        }

        private static int FindOscEnd(IEnumerable<char> value)
        {
            int index = 0;
            bool hasEscape = false;
            foreach (var ch in value)
            {
                if (ch == 0x7)
                    break;
                if (hasEscape && ch == '\\')
                    break;
                if (ch == Escape)
                    hasEscape = true;

                ++index;
            }

            return index + 1;
        }
    }
}
