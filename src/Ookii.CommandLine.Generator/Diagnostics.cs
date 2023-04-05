using Microsoft.CodeAnalysis;
using Ookii.CommandLine.Generator.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class Diagnostics
{
    public static DiagnosticDescriptor ArgumentsClassIsGeneric => new(
        "CL1001",
        new LocalizableResourceString(nameof(Resources.ArgumentsClassIsGenericTitle), Resources.ResourceManager, typeof(Resources)),
        new LocalizableResourceString(nameof(Resources.ArgumentsClassIsGenericMessageFormat), Resources.ResourceManager, typeof(Resources)),
        "Ookii.CommandLine",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
