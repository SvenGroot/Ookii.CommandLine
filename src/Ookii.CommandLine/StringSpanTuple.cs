using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using StringSpan = System.ReadOnlySpan<char>;
#endif

namespace Ookii.CommandLine
{
    // Since StringSpan is a ReadOnlySpan<char> on .Net 6, it cannot be used in a regular tuple.
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    internal ref struct StringSpanTuple
#else
    internal struct StringSpanTuple
#endif
    {
        public StringSpanTuple(StringSpan span1, StringSpan span2)
        {
            Span1 = span1;
            Span2 = span2;
        }

        public StringSpan Span1;
        public StringSpan Span2;

        public void Deconstruct(out StringSpan span1, out StringSpan span2)
        {
            span1 = Span1;
            span2 = Span2;
        }
    }
}
