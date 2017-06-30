# Line wrapping text writer

The Ookii.CommandLine library includes a utility class, the {{LineWrappingTextWriter}} class, which it uses for properly word-wrapping usage help when writing it to the console, but which can also be used in your own code.

The {{LineWrappingTextWriter}} class allows you to write text to another {{TextWriter}} class, wrapping that text at the specified line length, and optionally indenting subsequent lines.