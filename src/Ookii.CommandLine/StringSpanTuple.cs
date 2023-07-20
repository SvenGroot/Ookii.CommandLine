using System;

namespace Ookii.CommandLine;

// Since ReadOnlySpan<char> is a ref struct, it cannot be used in a regular tuple.
internal ref struct StringSpanTuple
{
    public StringSpanTuple(ReadOnlySpan<char> span1, ReadOnlySpan<char> span2)
    {
        Span1 = span1;
        Span2 = span2;
    }

    public ReadOnlySpan<char> Span1;
    public ReadOnlySpan<char> Span2;

    public void Deconstruct(out ReadOnlySpan<char> span1, out ReadOnlySpan<char> span2)
    {
        span1 = Span1;
        span2 = Span2;
    }
}
