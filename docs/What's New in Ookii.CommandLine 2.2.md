# What’s new in Ookii.CommandLine 2.2

Ookii.CommandLine 2.2 offers the following improvements over version 2.1:

* Added support for alternative names (aliases) for command line arguments.
* An argument’s aliases and default value can be included in the argument description when generating usage.
* Added code snippets.

Ookii.CommandLine 2.1 offers the following improvements over version 2.0:

* Added support for dictionary arguments; these are special multi-value arguments whose values take the form key=value.
* Multi-value arguments can be specified using a read-only property of any collection type (in addition to the previous array support).
* Multi-value properties can optionally use a separator character to allow multiple values to be specified without specifying the argument multiple times.
* Added support for specifying a custom type converter for individual arguments.
* When specifying the default value for an argument defined by a property you can now use any type that can be converted to the argument’s type using its type converter. This makes it possible to define default values for arguments with a type for which there are no literals.
* A CommandLineArgumentException is thrown when the argument type’s constructor or a property setter throws an exception (instead of a TargetInvocationException).
* The CommandLineParser no longer sets the property value for an unspecified argument with a default value of null.
* Shell commands can take their name from the type name.
* Shell commands can use custom argument parsing.
* Various minor bug fixes.

Ookii.CommandLine 2.0 offers the following improvements compared to Ookii.CommandLine 1.0:

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

Ookii.CommandLine 2.0 and newer version have substantial changes from version 1.0 and are not designed to be backwards compatible. There are changes in argument parsing behavior and API names and usage.

Upgrading an existing project that is using Ookii.CommandLine 1.0 to Ookii.CommandLine 2.0 or newer may require substantial code changes and may change how command lines are parsed.
