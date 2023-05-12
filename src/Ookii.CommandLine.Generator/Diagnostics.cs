using Microsoft.CodeAnalysis;
using Ookii.CommandLine.Generator.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class Diagnostics
{
    private const string Category = "Ookii.CommandLine";

    public static Diagnostic TypeNotReferenceType(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "CL0001",
        nameof(Resources.TypeNotReferenceTypeTitle),
        nameof(Resources.TypeNotReferenceTypeMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);

    public static Diagnostic ClassNotPartial(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "CL0002",
        nameof(Resources.ClassNotPartialTitle),
        nameof(Resources.ClassNotPartialMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);

    public static Diagnostic ClassIsGeneric(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "CL0003",
        nameof(Resources.ClassIsGenericTitle),
        nameof(Resources.ClassIsGenericMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);

    public static Diagnostic ClassIsNested(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "CL0004",
        nameof(Resources.ClassIsNestedTitle),
        nameof(Resources.ClassIsNestedMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);


    public static Diagnostic InvalidArrayRank(IPropertySymbol property) => CreateDiagnostic(
        "CL0005",
        nameof(Resources.InvalidArrayRankTitle),
        nameof(Resources.InvalidArrayRankMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic PropertyIsReadOnly(IPropertySymbol property) => CreateDiagnostic(
        "CL0006",
        nameof(Resources.PropertyIsReadOnlyTitle),
        nameof(Resources.PropertyIsReadOnlyMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic NoConverter(ISymbol member, ITypeSymbol elementType) => CreateDiagnostic(
        "CL0007",
        nameof(Resources.NoConverterTitle),
        nameof(Resources.NoConverterMessageFormat),
        DiagnosticSeverity.Error,
        member.Locations.FirstOrDefault(),
        elementType.ToDisplayString(),
        member.ContainingType?.ToDisplayString(),
        member.Name);

    public static Diagnostic InvalidMethodSignature(ISymbol method) => CreateDiagnostic(
        "CL0008",
        nameof(Resources.InvalidMethodSignatureTitle),
        nameof(Resources.InvalidMethodSignatureMessageFormat),
        DiagnosticSeverity.Error,
        method.Locations.FirstOrDefault(),
        method.ContainingType?.ToDisplayString(),
        method.Name);

    public static Diagnostic NonRequiredInitOnlyProperty(IPropertySymbol property) => CreateDiagnostic(
        "CL0009",
        nameof(Resources.NonRequiredInitOnlyPropertyTitle),
        nameof(Resources.NonRequiredInitOnlyPropertyMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic GeneratedCustomParsingCommand(INamedTypeSymbol symbol) => CreateDiagnostic(
        "CL0010",
        nameof(Resources.GeneratedCustomParsingCommandTitle),
        nameof(Resources.GeneratedCustomParsingCommandMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic PositionalArgumentAfterMultiValue(ISymbol symbol, string other) => CreateDiagnostic(
        "CL0011",
        nameof(Resources.PositionalArgumentAfterMultiValueTitle),
        nameof(Resources.PositionalArgumentAfterMultiValueMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        other);

    public static Diagnostic PositionalRequiredArgumentAfterOptional(ISymbol symbol, string other) => CreateDiagnostic(
        "CL0012",
        nameof(Resources.PositionalRequiredArgumentAfterOptionalTitle),
        nameof(Resources.PositionalRequiredArgumentAfterOptionalMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        other);

    public static Diagnostic InvalidAssemblyName(ISymbol symbol, string name) => CreateDiagnostic(
        "CL0013",
        nameof(Resources.InvalidAssemblyNameTitle),
        nameof(Resources.InvalidAssemblyNameMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        name);

    public static Diagnostic UnknownAssemblyName(ISymbol symbol, string name) => CreateDiagnostic(
        "CL0014",
        nameof(Resources.UnknownAssemblyNameTitle),
        nameof(Resources.UnknownAssemblyNameMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        name);

    public static Diagnostic ArgumentConverterStringNotSupported(ISymbol symbol) => CreateDiagnostic(
        "CL0015",
        nameof(Resources.ArgumentConverterStringNotSupportedTitle),
        nameof(Resources.ArgumentConverterStringNotSupportedMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic IgnoredAttribute(AttributeData attribute) => CreateDiagnostic(
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

    public static Diagnostic CommandAttributeWithoutInterface(INamedTypeSymbol symbol) => CreateDiagnostic(
        "CLW0004",
        nameof(Resources.CommandAttributeWithoutInterfaceTitle),
        nameof(Resources.CommandAttributeWithoutInterfaceMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DefaultValueWithRequired(ISymbol symbol) => CreateDiagnostic(
        "CLW0005",
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithRequiredMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DefaultValueWithMultiValue(ISymbol symbol) => CreateDiagnostic(
        "CLW0005", // Deliberately the same as above.
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithMultiValueMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DefaultValueWithMethod(ISymbol symbol) => CreateDiagnostic(
        "CLW0005", // Deliberately the same as above.
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithMethodMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic IsRequiredWithRequiredProperty(ISymbol symbol) => CreateDiagnostic(
        "CLW0006",
        nameof(Resources.IsRequiredWithRequiredPropertyTitle),
        nameof(Resources.IsRequiredWithRequiredPropertyMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DuplicatePosition(ISymbol symbol, string otherName) => CreateDiagnostic(
        "CLW0007",
        nameof(Resources.DuplicatePositionTitle),
        nameof(Resources.DuplicatePositionMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        otherName);

    public static Diagnostic ShortAliasWithoutShortName(ISymbol symbol) => CreateDiagnostic(
        "CLW0008",
        nameof(Resources.ShortAliasWithoutShortNameTitle),
        nameof(Resources.ShortAliasWithoutShortNameMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic AliasWithoutLongName(ISymbol symbol) => CreateDiagnostic(
        "CLW0009",
        nameof(Resources.AliasWithoutLongNameTitle),
        nameof(Resources.AliasWithoutLongNameMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic IsHiddenWithPositional(ISymbol symbol) => CreateDiagnostic(
        "CLW0010",
        nameof(Resources.IsHiddenWithPositionalTitle),
        nameof(Resources.IsHiddenWithPositionalMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic InvalidGeneratedConverterNamespace(string ns, AttributeData attribute) => CreateDiagnostic(
        "CLW0010",
        nameof(Resources.InvalidGeneratedConverterNamespaceTitle),
        nameof(Resources.InvalidGeneratedConverterNamespaceMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.ApplicationSyntaxReference?.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
        ns);

    public static Diagnostic IgnoredAttributeForNonDictionary(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "CLW0011",
        nameof(Resources.IgnoredAttributeForNonDictionaryTitle),
        nameof(Resources.IgnoredAttributeForNonDictionaryMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.ApplicationSyntaxReference?.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
        attribute.AttributeClass?.Name,
        member.ToDisplayString());

    public static Diagnostic IgnoredAttributeForDictionaryWithConverter(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "CLW0012",
        nameof(Resources.IgnoredAttributeForDictionaryWithConverterTitle),
        nameof(Resources.IgnoredAttributeForDictionaryWithConverterMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.ApplicationSyntaxReference?.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
        attribute.AttributeClass?.Name,
        member.ToDisplayString());

    public static Diagnostic IgnoredAttributeForNonMultiValue(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "CLW0013",
        nameof(Resources.IgnoredAttributeForNonMultiValueTitle),
        nameof(Resources.IgnoredAttributeForNonMultiValueMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.ApplicationSyntaxReference?.SyntaxTree.GetLocation(attribute.ApplicationSyntaxReference.Span),
        attribute.AttributeClass?.Name,
        member.ToDisplayString());

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
