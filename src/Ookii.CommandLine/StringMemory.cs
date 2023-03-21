#if !NET6_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    internal struct StringMemory
    {
        public StringMemory(char value)
        {
            Span = new StringSpan(value);
        }

        public StringMemory(char[] buffer, int offset, int length)
        {
            Span = new StringSpan(buffer, offset, length);
        }

        public StringMemory(string value)
        {
            Span = new StringSpan(value);
        }

        public StringSpan Span;

        public StringMemory Slice(int start, int length)
            => new() { Span = Span.Slice(start, length) };

        public StringMemory Slice(int start)
            => new() { Span = Span.Slice(start) };

        public async Task WriteToAsync(TextWriter writer)
            => await Span.WriteToAsync(writer);
    }
}

#endif
