using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    static class NativeMethods
    {
        static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

        public static bool EnableVirtualTerminalSequences(VirtualTerminal.StandardStream stream, bool enable)
        {
            if (stream == VirtualTerminal.StandardStream.Input)
                throw new ArgumentException(Properties.Resources.InvalidStandardStream, nameof(stream));

            var handle = GetStandardHandle(stream);
            if (handle == INVALID_HANDLE_VALUE)
                return false;

            if (!GetConsoleMode(handle, out ConsoleModes mode))
                return false;

            if (enable)
                mode |= ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            else
                mode &= ~ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            if (!SetConsoleMode(handle, mode))
                return false;

            return true;
        }

        public static IntPtr GetStandardHandle(VirtualTerminal.StandardStream stream)
        {
            var stdHandle = stream switch
            {
                VirtualTerminal.StandardStream.Output => StandardHandle.STD_OUTPUT_HANDLE,
                VirtualTerminal.StandardStream.Input => StandardHandle.STD_INPUT_HANDLE,
                VirtualTerminal.StandardStream.Error => StandardHandle.STD_ERROR_HANDLE,
                _ => throw new ArgumentException(Properties.Resources.InvalidStandardStream, nameof(stream)),
            };

            return GetStdHandle(stdHandle);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(StandardHandle nStdHandle);

        [Flags]
        private enum ConsoleModes : uint
        {
            ENABLE_PROCESSED_INPUT = 0x0001,
            ENABLE_LINE_INPUT = 0x0002,
            ENABLE_ECHO_INPUT = 0x0004,
            ENABLE_WINDOW_INPUT = 0x0008,
            ENABLE_MOUSE_INPUT = 0x0010,
            ENABLE_INSERT_MODE = 0x0020,
            ENABLE_QUICK_EDIT_MODE = 0x0040,
            ENABLE_EXTENDED_FLAGS = 0x0080,
            ENABLE_AUTO_POSITION = 0x0100,

            ENABLE_PROCESSED_OUTPUT = 0x0001,
            ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
            DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
            ENABLE_LVB_GRID_WORLDWIDE = 0x0010
        }

        private enum StandardHandle
        {
            STD_OUTPUT_HANDLE = -11,
            STD_INPUT_HANDLE = -10,
            STD_ERROR_HANDLE = -12,
        }


    }
}
