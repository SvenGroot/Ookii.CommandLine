using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class AttributeNames
{
    public const string NamespacePrefix = "Ookii.CommandLine.";
    public const string GeneratedParser = NamespacePrefix + "GeneratedParserAttribute";
    public const string CommandLineArgument = NamespacePrefix + "CommandLineArgumentAttribute";
    public const string ParseOptions = NamespacePrefix + "ParseOptionsAttribute";
    public const string ApplicationFriendlyName = NamespacePrefix + "ApplicationFriendlyNameAttribute";
    public const string Command = NamespacePrefix + "Commands.CommandAttribute";
    public const string ClassValidation = NamespacePrefix + "Validation.ClassValidationAttribute";

    public const string Description = "System.ComponentModel.DescriptionAttribute";
}
