# What’s new in Ookii.CommandLine 2.0

Ookii.CommandLine 2.0 offers the following new features compared to Ookii.CommandLine 1.0:

* Improved argument parsing:
	* All arguments can be specified by name.
	* Support for using whitespace to separate an argument name from its value.
	* Support for multiple argument name prefixes.
	* Support for using a custom StringComparer for argument name matching (to allow case sensitive or insensitive matching).
	* Support for use a custom CultureInfo for argument value conversion.
	* Non-positional arguments can be required arguments.
* Properties can be used to define positional arguments.
* More customizable generation of usage help text.
* The new shell commands functionality lets you easily create shell utilities with multiple operations that each uses its own command line arguments.
* The LineWrappingTextWriter class provides support for writing word-wrapped text to any output stream, with greater flexibility than the SplitLines method provided in Ookii.CommandLine 1.0.
* Targets .Net 2.0 for wider applicability.

## Upgrading from Ookii.CommandLine 1.0

Ookii.CommandLine 2.0 has substantial changes from the previous version and is not designed to be backwards compatible. There are changes in argument parsing behavior and API names and usage.

Upgrading an existing project that is using Ookii.CommandLine 1.0 to Ookii.CommandLine 2.0 may require substantial code changes and may change how command lines are parsed.