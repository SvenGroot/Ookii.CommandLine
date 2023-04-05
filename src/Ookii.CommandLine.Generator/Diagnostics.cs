using Microsoft.CodeAnalysis;
using Ookii.CommandLine.Generator.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class Diagnostics
{
    private const string Category = "Ookii.CommandLine";

    public static Diagnostic ArgumentsTypeNotReferenceType(INamedTypeSymbol symbol) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1001",
            new LocalizableResourceString(nameof(Resources.ArgumentsTypeNotReferenceTypeTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ArgumentsTypeNotReferenceTypeMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic ArgumentsClassNotPartial(INamedTypeSymbol symbol) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1002",
            new LocalizableResourceString(nameof(Resources.ArgumentsClassNotPartialTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ArgumentsClassNotPartialMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic ArgumentsClassIsGeneric(INamedTypeSymbol symbol) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1003",
            new LocalizableResourceString(nameof(Resources.ArgumentsClassIsGenericTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ArgumentsClassIsGenericMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic UnknownAttribute(AttributeData attribute) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CLW1001",
            new LocalizableResourceString(nameof(Resources.UnknownAttributeTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.UnknownAttributeMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true),
        attribute.ApplicationSyntaxReference?.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span), attribute.AttributeClass?.Name);
}
