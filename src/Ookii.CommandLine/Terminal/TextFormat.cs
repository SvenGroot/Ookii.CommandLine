using System;
using System.Drawing;

namespace Ookii.CommandLine.Terminal
{
    /// <summary>
    /// Provides constants for various virtual terminal sequences that control text format.
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
            return FormattableString.Invariant($"{VirtualTerminal.Escape}[{(foreground ? 38 : 48)};2;{color.R};{color.G};{color.B}m");
        }
    }
}
