namespace Ookii.CommandLine.Tests
{
    public partial class LineWrappingTextWriterTest
    {
        private static readonly string _input = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
quam pellentesque nec
nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum consectetur.
Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat. Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae sapien pellentesque habitant morbi tristique senectus et netus.
Augue neque gravida in
fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae elementum curabitur.

Lorem 01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789".ReplaceLineEndings();

        private static readonly string _expectedNoIndent = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus
nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
quam pellentesque nec
nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum consectetur.
Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat. Malesuada
fames ac turpis egestas maecenas pharetra. Fringilla est
ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae sapien
pellentesque habitant morbi tristique senectus et netus.
Augue neque gravida in
fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
elementum curabitur.

Lorem
01234567890123456789012345678901234567890123456789012345678901234567890123456789
01234567890123456789012345678901234567890123456789012345678901234567890123456789
0123456789012345678901234567890123456789
".ReplaceLineEndings();

        private static readonly string _expectedIndent = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
        incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
        risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
        consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
        Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
        sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
        elementum curabitur.

Lorem
        012345678901234567890123456789012345678901234567890123456789012345678901
        234567890123456789012345678901234567890123456789012345678901234567890123
        45678901234567890123456789012345678901234567890123456789
".ReplaceLineEndings();

        private static readonly string _expectedIndentChanges = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
    incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
    risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
    quam pellentesque nec
    nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
    consectetur.
    Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
    Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
    ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
    sapien pellentesque habitant morbi tristique senectus et netus.
    Augue neque gravida in
    fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
    elementum curabitur.

Lorem
    0123456789012345678901234567890123456789012345678901234567890123456789012345
    6789012345678901234567890123456789012345678901234567890123456789012345678901
    234567890123456789012345678901234567890123456789
    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing
        tristique risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
        consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
        Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
        sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
        elementum curabitur.

Lorem
        012345678901234567890123456789012345678901234567890123456789012345678901
        234567890123456789012345678901234567890123456789012345678901234567890123
        45678901234567890123456789012345678901234567890123456789
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
        incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
        risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum
        consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat.
        Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae
        sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae
        elementum curabitur.

Lorem
        012345678901234567890123456789012345678901234567890123456789012345678901
        234567890123456789012345678901234567890123456789012345678901234567890123
        45678901234567890123456789012345678901234567890123456789
".ReplaceLineEndings();

        private static readonly string _expectedIndentNoMaximum = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.

Tincidunt vitae semper quis lectus nulla at volutpat diam ut. Vitae tempus
        quam pellentesque nec
        nam aliquam. Porta non pulvinar neque laoreet suspendisse interdum consectetur.
        Arcu risus quis varius quam. Cursus mattis molestie a iaculis at erat. Malesuada fames ac turpis egestas maecenas pharetra. Fringilla est
        ullamcorper eget nulla facilisi etiam dignissim diam. Condimentum vitae sapien pellentesque habitant morbi tristique senectus et netus.
        Augue neque gravida in
        fermentum et sollicitudin ac orci. Aliquam malesuada bibendum arcu vitae elementum curabitur.

Lorem 01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789".ReplaceLineEndings();

        private static readonly string _inputFormatting = "\x1b[34mLorem \x1b[34mipsum \x1b[34mdolor \x1b[34msit \x1b[34mamet, \x1b[34mconsectetur \x1b[34madipiscing \x1b[34melit, \x1b]0;new title\x1b\\sed do \x1b]0;new title2\x0007eiusmod \x1b(Btempor\x1bH incididunt\nut labore et dolore magna aliqua. Donec\x1b[38;2;1;2;3m adipiscing tristique risus nec feugiat in fermentum.\x1b[0m".ReplaceLineEndings();

        private static readonly string _expectedFormatting = @"[34mLorem [34mipsum [34mdolor [34msit [34mamet, [34mconsectetur [34madipiscing [34melit, ]0;new title\sed do ]0;new title2eiusmod (BtemporH
        incididunt
        ut labore et dolore magna aliqua. Donec[38;2;1;2;3m adipiscing tristique risus nec
        feugiat in fermentum.[0m
".ReplaceLineEndings();

        private static readonly string _expectedFormattingCounted = @"[34mLorem [34mipsum [34mdolor [34msit [34mamet, [34mconsectetur
        [34madipiscing [34melit, ]0;new title\sed do ]0;new title2eiusmod
        (BtemporH incididunt
        ut labore et dolore magna aliqua. Donec[38;2;1;2;3m adipiscing
        tristique risus nec feugiat in fermentum.[0m
".ReplaceLineEndings();

        private const string _inputLongFormatting = "Lorem ipsum dolor sit amet, consectetur\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m\x1b[34m adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.";

        private static readonly string _expectedLongFormatting = @"Lorem ipsum dolor sit amet, consectetur[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m[34m adipiscing elit, sed do eiusmod tempor
        incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique
        risus nec feugiat in fermentum.
".ReplaceLineEndings();

        private const string _inputEnableWrapping = "Lorem ipsum dolor sit amet,\nconsectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.";

        private static readonly string _expectedEnableWrapping = @"Lorem ipsum dolor sit amet,
    consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et
    dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in
    fermentum.
Lorem ipsum dolor sit amet,
    consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.
Lorem ipsum dolor sit amet,
    consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et
    dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in
    fermentum.
".ReplaceLineEndings();

        private static readonly string _expectedEnableWrapping2 = @"Lorem ipsum dolor sit amet,
    consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et
    dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in
    fermentum.Lorem ipsum dolor sit amet,
    consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in fermentum.Lorem ipsum dolor sit amet,
    consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et
    dolore magna aliqua. Donec adipiscing tristique risus nec feugiat in
    fermentum.".ReplaceLineEndings();

    }
}
