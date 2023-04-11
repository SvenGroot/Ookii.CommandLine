using Microsoft.CodeAnalysis;
using Ookii.CommandLine.Generator.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine.Generator;

// TODO: Help URIs.
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

    public static Diagnostic InvalidArrayRank(IPropertySymbol property) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1004",
            new LocalizableResourceString(nameof(Resources.InvalidArrayRankTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidArrayRankMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        property.Locations.FirstOrDefault(), property.ContainingType?.ToDisplayString(), property.Name);

    public static Diagnostic PropertyIsReadOnly(IPropertySymbol property) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1005",
            new LocalizableResourceString(nameof(Resources.PropertyIsReadOnlyTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PropertyIsReadOnlyMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        property.Locations.FirstOrDefault(), property.ContainingType?.ToDisplayString(), property.Name);

    public static Diagnostic NoConverter(ISymbol member, ITypeSymbol elementType) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1006",
            new LocalizableResourceString(nameof(Resources.NoConverterTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.NoConverterMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        member.Locations.FirstOrDefault(), elementType.ToDisplayString(), member.ContainingType?.ToDisplayString(), member.Name);

    public static Diagnostic InvalidMethodSignature(ISymbol method) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1007",
            new LocalizableResourceString(nameof(Resources.InvalidMethodSignatureTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.InvalidMethodSignatureMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        method.Locations.FirstOrDefault(), method.ContainingType?.ToDisplayString(), method.Name);

    public static Diagnostic ArgumentsClassIsNested(INamedTypeSymbol symbol) => Diagnostic.Create(
        new DiagnosticDescriptor(
            "CL1008",
            new LocalizableResourceString(nameof(Resources.ArgumentsClassIsNestedTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ArgumentsClassIsNestedMessageFormat), Resources.ResourceManager, typeof(Resources)),
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
