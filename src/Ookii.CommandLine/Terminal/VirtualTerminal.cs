using System;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using StringSpan = System.ReadOnlySpan<char>;
#endif

namespace Ookii.CommandLine.Terminal
{
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

        // Returns the index of the character after the end of the sequence.
        internal static int FindSequenceEnd(StringSpan value, StringSpan value2 = default)
        {
            if (value.Length == 0)
            {
                return -1;
            }

            return value[0] switch
            {
                '[' => FindCsiEnd(value.Slice(1), value2),
                ']' => FindOscEnd(value.Slice(1), value2),
                // If the character after ( isn't present, we haven't found the end yet.
                '(' => value.Length + value2.Length > 1 ? 3 : -1,
                _ => 1,
            };
        }


        private static int FindCsiEnd(StringSpan value, StringSpan value2)
        {
            int index = 0;
            if (FindCsiEnd(value, ref index) || FindCsiEnd(value2, ref index))
            {
                return index + 3;
            }

            return -1;
        }

        private static bool FindCsiEnd(StringSpan value, ref int index)
        {
            foreach (var ch in value)
            {
                if (!char.IsNumber(ch) && ch != ';' && ch != ' ')
                {
                    return true;
                }

                ++index;
            }

            return false;
        }

        private static int FindOscEnd(StringSpan value, StringSpan value2)
        {
            int index = 0;
            bool hasEscape = false;
            if (FindOscEnd(value, ref index, ref hasEscape) || FindOscEnd(value2, ref index, ref hasEscape))
            {
                return index + 3;
            }

            return -1;
        }

        private static bool FindOscEnd(StringSpan value, ref int index, ref bool hasEscape)
        {
            foreach (var ch in value)
            {
                if (ch == 0x7)
                {
                    return true;
                }

                if (hasEscape)
                {
                    if (ch == '\\')
                    {
                        return true;
                    }

                    hasEscape = false;
                }

                if (ch == Escape)
                {
                    hasEscape = true;
                }

                ++index;
            }

            return false;
        }
    }
}
