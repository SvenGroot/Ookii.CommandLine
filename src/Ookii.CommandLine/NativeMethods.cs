using Ookii.CommandLine.Terminal;
using System;
using System.Runtime.InteropServices;

namespace Ookii.CommandLine;

static partial class NativeMethods
{
    static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

    public static (bool, ConsoleModes?) EnableVirtualTerminalSequences(StandardStream stream, bool enable)
    {
        if (stream == StandardStream.Input)
        {
            throw new ArgumentException(Properties.Resources.InvalidStandardStream, nameof(stream));
        }

        var handle = GetStandardHandle(stream);
        if (handle == INVALID_HANDLE_VALUE)
        {
            return (false, null);
        }

        if (!GetConsoleMode(handle, out ConsoleModes mode))
        {
            return (false, null);
        }

        var oldMode = mode;
        if (enable)
        {
            mode |= ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        }
        else
        {
            mode &= ~ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        }

        if (oldMode == mode)
        {
            return (true, null);
        }

        if (!SetConsoleMode(handle, mode))
        {
            return (false, null);
        }

        return (true, oldMode);
    }

    public static IntPtr GetStandardHandle(StandardStream stream)
    {
        var stdHandle = stream switch
        {
            StandardStream.Output => StandardHandle.STD_OUTPUT_HANDLE,
            StandardStream.Input => StandardHandle.STD_INPUT_HANDLE,
            StandardStream.Error => StandardHandle.STD_ERROR_HANDLE,
            _ => throw new ArgumentException(Properties.Resources.InvalidStandardStream, nameof(stream)),
        };

        return GetStdHandle(stdHandle);
    }

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(StandardHandle nStdHandle);
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
#endif

    [Flags]
    public enum ConsoleModes : uint
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

#pragma warning disable CA1069 // Enums values should not be duplicated
        ENABLE_PROCESSED_OUTPUT = 0x0001,
        ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
        ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
        DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
        ENABLE_LVB_GRID_WORLDWIDE = 0x0010
#pragma warning restore CA1069 // Enums values should not be duplicated
    }

    private enum StandardHandle
    {
        STD_OUTPUT_HANDLE = -11,
        STD_INPUT_HANDLE = -10,
        STD_ERROR_HANDLE = -12,
    }
}
