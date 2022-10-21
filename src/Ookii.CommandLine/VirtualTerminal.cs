using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    public static class VirtualTerminal
    {

        #region Nested Types

        public enum StandardStream
        {
            Output,
            Input,
            Error
        }

        public enum TextFormat
        {
            Default = 0,
            BoldBright = 1,
            Underline = 4,
            NoUnderline = 24,
            Negative = 7,
            Positive = 27,
            ForegroundBlack = 30,
            ForegroundRed = 31,
            ForegroundGreen = 32,
            ForegroundYellow = 33,
            ForegroundBlue = 34,
            ForegroundMagenta = 35,
            ForegroundCyan = 36,
            ForegroundWhite = 37,
            ForegroundExtended = 38,  // Do not use this directly, use SetExtendedForegroundColor instead.
            ForegroundDefault = 39,
            BackgroundBlack = 40,
            BackgroundRed = 41,
            BackgroundGreen = 42,
            BackgroundYellow = 43,
            BackgroundBlue = 44,
            BackgroundMagenta = 45,
            BackgroundCyan = 46,
            BackgroundWhite = 47,
            BackgroundExtended = 48, // Do not use this directly, use SetExtendedBackgroundColor instead.
            BackgroundDefault = 49,
            BrightForegroundBlack = 90,
            BrightForegroundRed = 91,
            BrightForegroundGreen = 92,
            BrightForegroundYellow = 93,
            BrightForegroundBlue = 94,
            BrightForegroundMagenta = 95,
            BrightForegroundCyan = 96,
            BrightForegroundWhite = 97,
            BrightBackgroundBlack = 100,
            BrightBackgroundRed = 101,
            BrightBackgroundGreen = 102,
            BrightBackgroundYellow = 103,
            BrightBackgroundBlue = 104,
            BrightBackgroundMagenta = 105,
            BrightBackgroundCyan = 106,
            BrightBackgroundWhite = 107
        }

        #endregion

        public const string Escape = "\x1b";
        public const string ControlSequenceIntroducer = Escape + "[";


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

        public static void DisableVirtualTerminalSequences(StandardStream stream)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                NativeMethods.EnableVirtualTerminalSequences(stream, false);
        }

        public static bool EnableColor(StandardStream stream)
        {
            if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
                return false;

            return EnableVirtualTerminalSequences(stream);
        }

        public static string GetTextFormatSequence(TextFormat format)
        {
            return $"{ControlSequenceIntroducer}{(int)format}m";
        }
    }
}
