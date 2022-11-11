using Ookii.CommandLine;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArgumentDependencies;

// This sample shows how you can have arguments that require or prohibit usage combined with
// other aruments. In this sample, "-Path" and "-Ip" are mutually exclusive, and "-Port" can
// only be used in combination with "-Ip".
//
// Because "-Path" and "-Ip" can't be used together, we can't make either of them required.
// Doing so would make it impossible to use the other argument. Instead, we use the RequiresAny
// validator on the class to specify that a valid invocation requires either "-Path" or "-Ip".
//
// This sample uses nameof() to refer to the arguments. This is a good idea because there's
// compile-time checks if you change a name, but take note: these attributes all require the
// argument name, *not* the member name, so nameof() only works if they're the same.
//
// If you use a NameTransform that changes the argument names, or use any explicit argument
// names, you CANNOT use nameof()!
[ApplicationFriendlyName("Ookii.CommandLine Dependency Sample")]
[Description("Sample command line application with argument dependencies. The application parses the command line and prints the results, but otherwise does nothing and none of the arguments are actually used for anything.")]
[RequiresAny(nameof(Path), nameof(Ip))]
internal class ProgramArguments
{
    [CommandLineArgument(Position = 0)]
    [Description("The path to use.")]
    public FileInfo? Path { get; set; }

    [CommandLineArgument]
    [Description("The IP address to connect to.")]
    [Prohibits(nameof(Path))]
    public IPAddress? Ip { get; set; }

    [CommandLineArgument(DefaultValue = 80)]
    [Description("The port to connect to.")]
    [Requires(nameof(Ip))]
    public int Port { get; set; }

    public static ProgramArguments? Parse()
    {
        return CommandLineParser.Parse<ProgramArguments>();
    }
}
