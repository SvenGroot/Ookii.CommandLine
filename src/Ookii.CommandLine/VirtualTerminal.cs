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

        public static class TextFormat
        {
            public const string Default = "\x1b[0m";
            public const string BoldBright = "\x1b[1m";
            public const string Underline = "\x1b[4m";
            public const string NoUnderline = "\x1b[24m";
            public const string Negative = "\x1b[7m";
            public const string Positive = "\x1b[27m";
            public const string ForegroundBlack = "\x1b[30m";
            public const string ForegroundRed = "\x1b[31m";
            public const string ForegroundGreen = "\x1b[32m";
            public const string ForegroundYellow = "\x1b[33m";
            public const string ForegroundBlue = "\x1b[34m";
            public const string ForegroundMagenta = "\x1b[35m";
            public const string ForegroundCyan = "\x1b[36m";
            public const string ForegroundWhite = "\x1b[37m";
            public const string ForegroundExtended = "\x1b[38m";  // Do not use this directly, use SetExtendedForegroundColor instead.
            public const string ForegroundDefault = "\x1b[39m";
            public const string BackgroundBlack = "\x1b[40m";
            public const string BackgroundRed = "\x1b[41m";
            public const string BackgroundGreen = "\x1b[42m";
            public const string BackgroundYellow = "\x1b[43m";
            public const string BackgroundBlue = "\x1b[44m";
            public const string BackgroundMagenta = "\x1b[45m";
            public const string BackgroundCyan = "\x1b[46m";
            public const string BackgroundWhite = "\x1b[47m";
            public const string BackgroundExtended = "\x1b[48m"; // Do not use this directly, use SetExtendedBackgroundColor instead.
            public const string BackgroundDefault = "\x1b[49m";
            public const string BrightForegroundBlack = "\x1b[90m";
            public const string BrightForegroundRed = "\x1b[91m";
            public const string BrightForegroundGreen = "\x1b[92m";
            public const string BrightForegroundYellow = "\x1b[93m";
            public const string BrightForegroundBlue = "\x1b[94m";
            public const string BrightForegroundMagenta = "\x1b[95m";
            public const string BrightForegroundCyan = "\x1b[96m";
            public const string BrightForegroundWhite = "\x1b[97m";
            public const string BrightBackgroundBlack = "\x1b[100m";
            public const string BrightBackgroundRed = "\x1b[101m";
            public const string BrightBackgroundGreen = "\x1b[102m";
            public const string BrightBackgroundYellow = "\x1b[103m";
            public const string BrightBackgroundBlue = "\x1b[104m";
            public const string BrightBackgroundMagenta = "\x1b[105m";
            public const string BrightBackgroundCyan = "\x1b[106m";
            public const string BrightBackgroundWhite = "\x1b[107m";
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
    }
}
