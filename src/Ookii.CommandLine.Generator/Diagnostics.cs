using Microsoft.CodeAnalysis;
using Ookii.CommandLine.Generator.Properties;

namespace Ookii.CommandLine.Generator;

internal static class Diagnostics
{
    private const string Category = "Ookii.CommandLine";

    public static Diagnostic TypeNotReferenceType(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "OCL0001",
        nameof(Resources.TypeNotReferenceTypeTitle),
        nameof(Resources.TypeNotReferenceTypeMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);

    public static Diagnostic ClassNotPartial(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "OCL0002",
        nameof(Resources.ClassNotPartialTitle),
        nameof(Resources.ClassNotPartialMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);

    public static Diagnostic ClassIsGeneric(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "OCL0003",
        nameof(Resources.ClassIsGenericTitle),
        nameof(Resources.ClassIsGenericMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);

    public static Diagnostic ClassIsNested(INamedTypeSymbol symbol, string attributeName) => CreateDiagnostic(
        "OCL0004",
        nameof(Resources.ClassIsNestedTitle),
        nameof(Resources.ClassIsNestedMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);


    public static Diagnostic InvalidArrayRank(IPropertySymbol property) => CreateDiagnostic(
        "OCL0005",
        nameof(Resources.InvalidArrayRankTitle),
        nameof(Resources.InvalidArrayRankMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic PropertyIsReadOnly(IPropertySymbol property) => CreateDiagnostic(
        "OCL0006",
        nameof(Resources.PropertyIsReadOnlyTitle),
        nameof(Resources.PropertyIsReadOnlyMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic NoConverter(ISymbol member, ITypeSymbol elementType) => CreateDiagnostic(
        "OCL0007",
        nameof(Resources.NoConverterTitle),
        nameof(Resources.NoConverterMessageFormat),
        DiagnosticSeverity.Error,
        member.Locations.FirstOrDefault(),
        elementType.ToDisplayString(),
        member.ContainingType?.ToDisplayString(),
        member.Name);

    public static Diagnostic InvalidMethodSignature(ISymbol method) => CreateDiagnostic(
        "OCL0008",
        nameof(Resources.InvalidMethodSignatureTitle),
        nameof(Resources.InvalidMethodSignatureMessageFormat),
        DiagnosticSeverity.Error,
        method.Locations.FirstOrDefault(),
        method.ContainingType?.ToDisplayString(),
        method.Name);

    public static Diagnostic NonRequiredInitOnlyProperty(IPropertySymbol property) => CreateDiagnostic(
        "OCL0009",
        nameof(Resources.NonRequiredInitOnlyPropertyTitle),
        nameof(Resources.NonRequiredInitOnlyPropertyMessageFormat),
        DiagnosticSeverity.Error,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic GeneratedCustomParsingCommand(INamedTypeSymbol symbol) => CreateDiagnostic(
        "OCL0010",
        nameof(Resources.GeneratedCustomParsingCommandTitle),
        nameof(Resources.GeneratedCustomParsingCommandMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic PositionalArgumentAfterMultiValue(ISymbol symbol, string other) => CreateDiagnostic(
        "OCL0011",
        nameof(Resources.PositionalArgumentAfterMultiValueTitle),
        nameof(Resources.PositionalArgumentAfterMultiValueMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        other);

    public static Diagnostic PositionalRequiredArgumentAfterOptional(ISymbol symbol, string other) => CreateDiagnostic(
        "OCL0012",
        nameof(Resources.PositionalRequiredArgumentAfterOptionalTitle),
        nameof(Resources.PositionalRequiredArgumentAfterOptionalMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        other);

    public static Diagnostic InvalidAssemblyName(ISymbol symbol, string name) => CreateDiagnostic(
        "OCL0013",
        nameof(Resources.InvalidAssemblyNameTitle),
        nameof(Resources.InvalidAssemblyNameMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        name);

    public static Diagnostic UnknownAssemblyName(ISymbol symbol, string name) => CreateDiagnostic(
        "OCL0014",
        nameof(Resources.UnknownAssemblyNameTitle),
        nameof(Resources.UnknownAssemblyNameMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        name);

    public static Diagnostic ArgumentConverterStringNotSupported(AttributeData attribute, ISymbol symbol) => CreateDiagnostic(
        "OCL0015",
        nameof(Resources.ArgumentConverterStringNotSupportedTitle),
        nameof(Resources.ArgumentConverterStringNotSupportedMessageFormat),
        DiagnosticSeverity.Error,
        attribute.GetLocation(),
        symbol.ToDisplayString());

    public static Diagnostic ParentCommandStringNotSupported(AttributeData attribute, ISymbol symbol) => CreateDiagnostic(
        "OCL0015", // Intentially the same as above.
        nameof(Resources.ParentCommandStringNotSupportedTitle),
        nameof(Resources.ParentCommandStringNotSupportedMessageFormat),
        DiagnosticSeverity.Error,
        attribute.GetLocation(),
        symbol.ToDisplayString());

    public static Diagnostic IgnoredAttribute(ISymbol symbol, AttributeData attribute) => CreateDiagnostic(
        "OCL0016",
        nameof(Resources.UnknownAttributeTitle),
        nameof(Resources.UnknownAttributeMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        attribute.AttributeClass?.ToDisplayString(),
        symbol.ToDisplayString());

    public static Diagnostic NonPublicStaticMethod(ISymbol method) => CreateDiagnostic(
        "OCL0017",
        nameof(Resources.NonPublicStaticMethodTitle),
        nameof(Resources.NonPublicStaticMethodMessageFormat),
        DiagnosticSeverity.Warning,
        method.Locations.FirstOrDefault(),
        method.ContainingType?.ToDisplayString(),
        method.Name);

    public static Diagnostic NonPublicInstanceProperty(ISymbol property) => CreateDiagnostic(
        "OCL0018",
        nameof(Resources.NonPublicInstancePropertyTitle),
        nameof(Resources.NonPublicInstancePropertyMessageFormat),
        DiagnosticSeverity.Warning,
        property.Locations.FirstOrDefault(),
        property.ContainingType?.ToDisplayString(),
        property.Name);

    public static Diagnostic CommandAttributeWithoutInterface(INamedTypeSymbol symbol) => CreateDiagnostic(
        "OCL0019",
        nameof(Resources.CommandAttributeWithoutInterfaceTitle),
        nameof(Resources.CommandAttributeWithoutInterfaceMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DefaultValueWithRequired(ISymbol symbol) => CreateDiagnostic(
        "OCL0020",
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithRequiredMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DefaultValueWithMultiValue(ISymbol symbol) => CreateDiagnostic(
        "OCL0020", // Deliberately the same as above.
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithMultiValueMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DefaultValueWithMethod(ISymbol symbol) => CreateDiagnostic(
        "OCL0020", // Deliberately the same as above.
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithMethodMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic IsRequiredWithRequiredProperty(ISymbol symbol) => CreateDiagnostic(
        "OCL0021",
        nameof(Resources.IsRequiredWithRequiredPropertyTitle),
        nameof(Resources.IsRequiredWithRequiredPropertyMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic DuplicatePosition(ISymbol symbol, string otherName) => CreateDiagnostic(
        "OCL0022",
        nameof(Resources.DuplicatePositionTitle),
        nameof(Resources.DuplicatePositionMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        otherName);

    public static Diagnostic ShortAliasWithoutShortName(AttributeData attribute, ISymbol symbol) => CreateDiagnostic(
        "OCL0023",
        nameof(Resources.ShortAliasWithoutShortNameTitle),
        nameof(Resources.ShortAliasWithoutShortNameMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        symbol.ToDisplayString());

    public static Diagnostic AliasWithoutLongName(AttributeData attribute, ISymbol symbol) => CreateDiagnostic(
        "OCL0024",
        nameof(Resources.AliasWithoutLongNameTitle),
        nameof(Resources.AliasWithoutLongNameMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        symbol.ToDisplayString());

    public static Diagnostic IsHiddenWithPositional(ISymbol symbol) => CreateDiagnostic(
        "OCL0025",
        nameof(Resources.IsHiddenWithPositionalTitle),
        nameof(Resources.IsHiddenWithPositionalMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic InvalidGeneratedConverterNamespace(string ns, AttributeData attribute) => CreateDiagnostic(
        "OCL0026",
        nameof(Resources.InvalidGeneratedConverterNamespaceTitle),
        nameof(Resources.InvalidGeneratedConverterNamespaceMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        ns);

    public static Diagnostic IgnoredAttributeForNonDictionary(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "OCL0027",
        nameof(Resources.IgnoredAttributeForNonDictionaryTitle),
        nameof(Resources.IgnoredAttributeForNonDictionaryMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        attribute.AttributeClass?.Name,
        member.ToDisplayString());

    public static Diagnostic IgnoredAttributeForDictionaryWithConverter(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "OCL0028",
        nameof(Resources.IgnoredAttributeForDictionaryWithConverterTitle),
        nameof(Resources.IgnoredAttributeForDictionaryWithConverterMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        attribute.AttributeClass?.Name,
        member.ToDisplayString());

    public static Diagnostic IgnoredAttributeForNonMultiValue(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "OCL0029",
        nameof(Resources.IgnoredAttributeForNonMultiValueTitle),
        nameof(Resources.IgnoredAttributeForNonMultiValueMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        attribute.AttributeClass?.Name,
        member.ToDisplayString());

    public static Diagnostic ArgumentStartsWithNumber(ISymbol member, string name) => CreateDiagnostic(
        "OCL0030",
        nameof(Resources.ArgumentStartsWithNumberTitle),
        nameof(Resources.ArgumentStartsWithNumberMessageFormat),
        DiagnosticSeverity.Warning,
        member.Locations.FirstOrDefault(),
        name,
        member.ToDisplayString());

    public static Diagnostic NoLongOrShortName(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "OCL0031",
        nameof(Resources.NoLongOrShortNameTitle),
        nameof(Resources.NoLongOrShortNameMessageFormat),
        DiagnosticSeverity.Error,
        attribute.GetLocation(),
        member.ToDisplayString());

    public static Diagnostic IsShortIgnored(ISymbol member, AttributeData attribute) => CreateDiagnostic(
        "OCL0032",
        nameof(Resources.IsShortIgnoredTitle),
        nameof(Resources.IsShortIgnoredMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        member.ToDisplayString());

    public static Diagnostic ArgumentWithoutDescription(ISymbol member) => CreateDiagnostic(
        "OCL0033",
        nameof(Resources.ArgumentWithoutDescriptionTitle),
        nameof(Resources.ArgumentWithoutDescriptionMessageFormat),
        DiagnosticSeverity.Warning,
        member.Locations.FirstOrDefault(),
        member.ToDisplayString());

    public static Diagnostic CommandWithoutDescription(ISymbol symbol) => CreateDiagnostic(
        "OCL0034",
        nameof(Resources.CommandWithoutDescriptionTitle),
        nameof(Resources.CommandWithoutDescriptionMessageFormat),
        DiagnosticSeverity.Warning,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString());

    public static Diagnostic IgnoredAttributeForNonCommand(ISymbol symbol, AttributeData attribute) => CreateDiagnostic(
        "OCL0035",
        nameof(Resources.IgnoredAttributeForNonCommandTitle),
        nameof(Resources.IgnoredAttributeForNonCommandMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        attribute.AttributeClass?.ToDisplayString(),
        symbol.ToDisplayString());

    public static Diagnostic IgnoredFriendlyNameAttribute(ISymbol symbol, AttributeData attribute) => CreateDiagnostic(
        "OCL0036",
        nameof(Resources.IgnoredFriendlyNameAttributeTitle),
        nameof(Resources.IgnoredFriendlyNameAttributeMessageFormat),
        DiagnosticSeverity.Warning,
        attribute.GetLocation(),
        symbol.ToDisplayString());

    public static Diagnostic UnsupportedLanguageVersion(ISymbol symbol, string attributeName) => CreateDiagnostic(
        "OCL0037",
        nameof(Resources.UnsupportedLanguageVersionTitle),
        nameof(Resources.UnsupportedLanguageVersionMessageFormat),
        DiagnosticSeverity.Error,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(),
        attributeName);

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
