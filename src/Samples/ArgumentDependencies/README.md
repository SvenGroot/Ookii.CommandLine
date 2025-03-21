# Argument dependencies sample

This sample shows how to use the [argument dependency validators](../../../docs/Validation.md#argument-dependencies-and-restrictions).
These validators let you specify that certain arguments must or cannot be used together. It also
makes it possible to specify that the user must use one of a set of arguments, something that can't
be expressed with regular required arguments.

The validators in question are the [`RequiresAttribute`][], the [`ProhibitsAttribute`][], and the
[`RequiresAnyAttribute`][]. You can see them in action in
[ProgramArguments.cs](ProgramArguments.cs).

This is the usage help output for this sample:

```text
Sample command line application with argument dependencies. The application parses the command line
and prints the results, but otherwise does nothing and none of the arguments are actually used for
anything.

Usage: ArgumentDependencies [[-Path] <FileInfo>] [-Help] [-Ip <IPAddress>] [-Port <Int32>]
   [-Version]

You must use at least one of: -Path, -Ip.

    -Path <FileInfo>
        The path to use.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Ip <IPAddress>
        The IP address to connect to. Cannot be used with: -Path.

    -Port <Int32>
        The port to connect to. Must be used with: -Ip. Default value: 80.

    -Version [<Boolean>]
        Displays version information.
```

The validators add their own help messages to the usage help. [`RequiresAnyAttribute`][] does so
before the command list, and the [`RequiresAttribute`][] and [`ProhibitsAttribute`][] added text to
the descriptions of the arguments they were applied to.

This is, as always fully customizable. You can disable automatic validator help entirely with the
[`UsageWriter.IncludeValidatorsInDescription`][] property (note: this also applies to regular
validators like [`ValidateRangeAttribute`][]), and all the included validators can be included on a
case-by-case basis with the [`IncludeInUsageHelp`][IncludeInUsageHelp_0] property on each validator
attribute.

[`ProhibitsAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Validation_ProhibitsAttribute.htm
[`RequiresAnyAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Validation_RequiresAnyAttribute.htm
[`RequiresAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Validation_RequiresAttribute.htm
[`UsageWriter.IncludeValidatorsInDescription`]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_UsageWriter_IncludeValidatorsInDescription.htm
[`ValidateRangeAttribute`]: https://www.ookii.org/docs/commandline-5.0/html/T_Ookii_CommandLine_Validation_ValidateRangeAttribute.htm
[IncludeInUsageHelp_0]: https://www.ookii.org/docs/commandline-5.0/html/P_Ookii_CommandLine_Validation_ArgumentValidationWithHelpAttribute_IncludeInUsageHelp.htm
