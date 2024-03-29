﻿using Ookii.CommandLine;
using Ookii.CommandLine.Conversion;
using System.ComponentModel;
using System.Text;

namespace TopLevelArguments;

[GeneratedParser]
[Description("Subcommands with top-level arguments sample for Ookii.CommandLine.")]
partial class TopLevelArguments
{
    // A required, positional argument to specify the file name.
    [CommandLineArgument(IsPositional = true)]
    [Description("The path of the file to read or write.")]
    public required FileInfo Path { get; set; }

    // A required, positional argument to specify what command to run.
    //
    // When this argument is encountered, parsing is canceled, returning success using the arguments
    // so far. The Main() method will then pass the remaining arguments to the specified command.
    [CommandLineArgument(IsPositional = true, CancelParsing = CancelMode.Success)]
    [Description("The command to run. After this argument, all remaining arguments are passed to the command.")]
    public required string Command { get; set; }

    // An argument to specify the encoding.
    // Because Encoding doesn't have a default ArgumentConverter, we use a custom one provided in
    // this sample.
    // Encoding's ToString() implementation just gives the class name, so don't include the default
    // value in the usage help; we'll write it ourself instead.
    [CommandLineArgument(IsShort = true, IncludeDefaultInUsageHelp = false)]
    [Description("The encoding to use to read the file. The default value is utf-8.")]
    [ArgumentConverter(typeof(EncodingConverter))]
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
