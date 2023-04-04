using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class AttributeNames
{
    public const string NamespacePrefix = "Ookii.CommandLine.";
    public const string GeneratedParser = NamespacePrefix + "GeneratedParserAttribute";
    public const string CommandLineArgument = NamespacePrefix + "CommandLineArgumentAttribute";
}
