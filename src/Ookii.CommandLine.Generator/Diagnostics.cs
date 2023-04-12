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

    public static Diagnostic ArgumentsTypeNotReferenceType(INamedTypeSymbol symbol) => CreateDiagnostic(
        "CL0001",
        nameof(Resources.ArgumentsTypeNotReferenceTypeTitle),
        nameof(Resources.ArgumentsTypeNotReferenceTypeMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic ArgumentsClassNotPartial(INamedTypeSymbol symbol) => CreateDiagnostic(
        "CL0002",
        nameof(Resources.ArgumentsClassNotPartialTitle),
        nameof(Resources.ArgumentsClassNotPartialMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic ArgumentsClassIsGeneric(INamedTypeSymbol symbol) => CreateDiagnostic(
        "CL0003",
        nameof(Resources.ArgumentsClassIsGenericTitle),
        nameof(Resources.ArgumentsClassIsGenericMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic InvalidArrayRank(IPropertySymbol property) => CreateDiagnostic(
        "CL0004",
        nameof(Resources.InvalidArrayRankTitle),
        nameof(Resources.InvalidArrayRankMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic PropertyIsReadOnly(IPropertySymbol property) => CreateDiagnostic(
        "CL0005",
        nameof(Resources.PropertyIsReadOnlyTitle),
        nameof(Resources.PropertyIsReadOnlyMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(), property.Name);

    public static Diagnostic NoConverter(ISymbol member, ITypeSymbol elementType) => CreateDiagnostic(
        "CL0006",
        nameof(Resources.NoConverterTitle),
        nameof(Resources.NoConverterMessageFormat),
        DiagnosticSeverity.Error,
        member.Locations.FirstOrDefault(),
        elementType.ToDisplayString(),
        member.ContainingType?.ToDisplayString(),
        member.Name);

    public static Diagnostic InvalidMethodSignature(ISymbol method) => CreateDiagnostic(
        "CL0007",
        nameof(Resources.InvalidMethodSignatureTitle),
        nameof(Resources.InvalidMethodSignatureMessageFormat),
        DiagnosticSeverity.Error,
        method.Locations.FirstOrDefault(),
        method.ContainingType?.ToDisplayString(),
        method.Name);

    public static Diagnostic ArgumentsClassIsNested(INamedTypeSymbol symbol) => CreateDiagnostic(
        "CL0008",
        nameof(Resources.ArgumentsClassIsNestedTitle),
        nameof(Resources.ArgumentsClassIsNestedMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic UnknownAttribute(AttributeData attribute) => CreateDiagnostic(
        "CLW0001",
        nameof(Resources.UnknownAttributeTitle),
        nameof(Resources.UnknownAttributeMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.ApplicationSyntaxReference?.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
        attribute.AttributeClass?.Name);

    public static Diagnostic NonPublicStaticMethod(ISymbol method) => CreateDiagnostic(
        "CLW0002",
        nameof(Resources.NonPublicStaticMethodTitle),
        nameof(Resources.NonPublicStaticMethodMessageFormat),
        DiagnosticSeverity.Warning,
        method.Locations.FirstOrDefault(),
        method.ContainingType?.ToDisplayString(),
        method.Name);

    public static Diagnostic NonPublicInstanceProperty(ISymbol property) => CreateDiagnostic(
        "CLW0003",
        nameof(Resources.NonPublicInstancePropertyTitle),
        nameof(Resources.NonPublicInstancePropertyMessageFormat),
        DiagnosticSeverity.Warning,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    private static Diagnostic CreateDiagnostic(string id, string titleResource, string messageResource, DiagnosticSeverity severity, Location? location, params object?[]? messageArgs)
        => Diagnostic.Create(
            new DiagnosticDescriptor(
                id,
                new LocalizableResourceString(titleResource, Resources.ResourceManager, typeof(Resources)),
                new LocalizableResourceString(messageResource, Resources.ResourceManager, typeof(Resources)),
                Category,
                severity,
                isEnabledByDefault: true,
                helpLinkUri: $"https://www.ookii.org/Link/CommandLineGeneratorError#{id}"),
            location, messageArgs);
}
