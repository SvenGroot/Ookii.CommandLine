using Microsoft.CodeAnalysis;
using Ookii.CommandLine.Generator.Properties;

namespace Ookii.CommandLine.Generator;

internal static class Diagnostics
{
    private const string Category = "Ookii.CommandLine";

    public static DiagnosticDescriptor TypeNotReferenceTypeDescriptor = CreateDiagnosticDescriptor(
        "OCL0001",
        nameof(Resources.TypeNotReferenceTypeTitle),
        nameof(Resources.TypeNotReferenceTypeMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic TypeNotReferenceType(INamedTypeSymbol symbol, string attributeName) => Diagnostic.Create(
        TypeNotReferenceTypeDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        attributeName);

    public static DiagnosticDescriptor ClassNotPartialDescriptor = CreateDiagnosticDescriptor(
        "OCL0002",
        nameof(Resources.ClassNotPartialTitle),
        nameof(Resources.ClassNotPartialMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic ClassNotPartial(INamedTypeSymbol symbol, string attributeName) => Diagnostic.Create(
        ClassNotPartialDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        attributeName);

    public static DiagnosticDescriptor ClassIsGenericDescriptor = CreateDiagnosticDescriptor(
        "OCL0003",
        nameof(Resources.ClassIsGenericTitle),
        nameof(Resources.ClassIsGenericMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic ClassIsGeneric(INamedTypeSymbol symbol, string attributeName) => Diagnostic.Create(
        ClassIsGenericDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        attributeName);

    public static DiagnosticDescriptor ClassIsNestedDescriptor = CreateDiagnosticDescriptor(
        "OCL0004",
        nameof(Resources.ClassIsNestedTitle),
        nameof(Resources.ClassIsNestedMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic ClassIsNested(INamedTypeSymbol symbol, string attributeName) => Diagnostic.Create(
        ClassIsNestedDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        attributeName);


    public static DiagnosticDescriptor InvalidArrayRankDescriptor = CreateDiagnosticDescriptor(
        "OCL0005",
        nameof(Resources.InvalidArrayRankTitle),
        nameof(Resources.InvalidArrayRankMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic InvalidArrayRank(IPropertySymbol property) => Diagnostic.Create(
        InvalidArrayRankDescriptor,
        property.Locations.FirstOrDefault(),
        property.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor PropertyIsReadOnlyDescriptor = CreateDiagnosticDescriptor(
        "OCL0006",
        nameof(Resources.PropertyIsReadOnlyTitle),
        nameof(Resources.PropertyIsReadOnlyMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic PropertyIsReadOnly(IPropertySymbol property) => Diagnostic.Create(
        PropertyIsReadOnlyDescriptor,
        property.Locations.FirstOrDefault(),
        property.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor NoConverterDescriptor = CreateDiagnosticDescriptor(
        "OCL0007",
        nameof(Resources.NoConverterTitle),
        nameof(Resources.NoConverterMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic NoConverter(ISymbol member, ITypeSymbol elementType) => Diagnostic.Create(
        NoConverterDescriptor,
        member.Locations.FirstOrDefault(),
        elementType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor InvalidMethodSignatureDescriptor = CreateDiagnosticDescriptor(
        "OCL0008",
        nameof(Resources.InvalidMethodSignatureTitle),
        nameof(Resources.InvalidMethodSignatureMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic InvalidMethodSignature(ISymbol method) => Diagnostic.Create(
        InvalidMethodSignatureDescriptor,
        method.Locations.FirstOrDefault(),
        method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor NonRequiredInitOnlyPropertyDescriptor = CreateDiagnosticDescriptor(
        "OCL0009",
        nameof(Resources.NonRequiredInitOnlyPropertyTitle),
        nameof(Resources.NonRequiredInitOnlyPropertyMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic NonRequiredInitOnlyProperty(IPropertySymbol property) => Diagnostic.Create(
        NonRequiredInitOnlyPropertyDescriptor,
        property.Locations.FirstOrDefault(),
        property.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor GeneratedCustomParsingCommandDescriptor = CreateDiagnosticDescriptor(
        "OCL0010",
        nameof(Resources.GeneratedCustomParsingCommandTitle),
        nameof(Resources.GeneratedCustomParsingCommandMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic GeneratedCustomParsingCommand(INamedTypeSymbol symbol) => Diagnostic.Create(
        GeneratedCustomParsingCommandDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor PositionalArgumentAfterMultiValueDescriptor = CreateDiagnosticDescriptor(
        "OCL0011",
        nameof(Resources.PositionalArgumentAfterMultiValueTitle),
        nameof(Resources.PositionalArgumentAfterMultiValueMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic PositionalArgumentAfterMultiValue(ISymbol symbol, string other) => Diagnostic.Create(
        PositionalArgumentAfterMultiValueDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        other);

    public static DiagnosticDescriptor PositionalRequiredArgumentAfterOptionalDescriptor = CreateDiagnosticDescriptor(
        "OCL0012",
        nameof(Resources.PositionalRequiredArgumentAfterOptionalTitle),
        nameof(Resources.PositionalRequiredArgumentAfterOptionalMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic PositionalRequiredArgumentAfterOptional(ISymbol symbol, string other) => Diagnostic.Create(
        PositionalRequiredArgumentAfterOptionalDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        other);

    public static DiagnosticDescriptor InvalidAssemblyNameDescriptor = CreateDiagnosticDescriptor(
        "OCL0013",
        nameof(Resources.InvalidAssemblyNameTitle),
        nameof(Resources.InvalidAssemblyNameMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic InvalidAssemblyName(ISymbol symbol, string name) => Diagnostic.Create(
        InvalidAssemblyNameDescriptor,
        symbol.Locations.FirstOrDefault(),
        name);

    public static DiagnosticDescriptor UnknownAssemblyNameDescriptor = CreateDiagnosticDescriptor(
        "OCL0014",
        nameof(Resources.UnknownAssemblyNameTitle),
        nameof(Resources.UnknownAssemblyNameMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic UnknownAssemblyName(ISymbol symbol, string name) => Diagnostic.Create(
        UnknownAssemblyNameDescriptor,
        symbol.Locations.FirstOrDefault(),
        name);

    public static DiagnosticDescriptor ArgumentConverterStringNotSupportedDescriptor = CreateDiagnosticDescriptor(
        "OCL0015",
        nameof(Resources.ArgumentConverterStringNotSupportedTitle),
        nameof(Resources.ArgumentConverterStringNotSupportedMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic ArgumentConverterStringNotSupported(AttributeData attribute, ISymbol symbol) => Diagnostic.Create(
        ArgumentConverterStringNotSupportedDescriptor,
        attribute.GetLocation(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor ParentCommandStringNotSupportedDescriptor = CreateDiagnosticDescriptor(
        "OCL0015", // Intentionally the same as above.
        nameof(Resources.ParentCommandStringNotSupportedTitle),
        nameof(Resources.ParentCommandStringNotSupportedMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic ParentCommandStringNotSupported(AttributeData attribute, ISymbol symbol) => Diagnostic.Create(
        ParentCommandStringNotSupportedDescriptor,
        attribute.GetLocation(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IgnoredTypeConverterAttributeDescriptor = CreateDiagnosticDescriptor(
        "OCL0016",
        nameof(Resources.IgnoredTypeConverterAttributeTitle),
        nameof(Resources.IgnoredTypeConverterAttributeMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IgnoredTypeConverterAttribute(ISymbol symbol, AttributeData attribute) => Diagnostic.Create(
        IgnoredTypeConverterAttributeDescriptor,
        attribute.GetLocation(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor NonPublicStaticMethodDescriptor = CreateDiagnosticDescriptor(
        "OCL0017",
        nameof(Resources.NonPublicStaticMethodTitle),
        nameof(Resources.NonPublicStaticMethodMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic NonPublicStaticMethod(ISymbol method) => Diagnostic.Create(
        NonPublicStaticMethodDescriptor,
        method.Locations.FirstOrDefault(),
        method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor NonPublicInstancePropertyDescriptor = CreateDiagnosticDescriptor(
        "OCL0018",
        nameof(Resources.NonPublicInstancePropertyTitle),
        nameof(Resources.NonPublicInstancePropertyMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic NonPublicInstanceProperty(ISymbol property) => Diagnostic.Create(
        NonPublicInstancePropertyDescriptor,
        property.Locations.FirstOrDefault(),
        property.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor CommandAttributeWithoutInterfaceDescriptor = CreateDiagnosticDescriptor(
        "OCL0019",
        nameof(Resources.CommandAttributeWithoutInterfaceTitle),
        nameof(Resources.CommandAttributeWithoutInterfaceMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic CommandAttributeWithoutInterface(INamedTypeSymbol symbol) => Diagnostic.Create(
        CommandAttributeWithoutInterfaceDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor CommandInterfaceWithoutAttributeDescriptor = CreateDiagnosticDescriptor(
        "OCL0019", // Intentionally the same as above.
        nameof(Resources.CommandInterfaceWithoutAttributeTitle),
        nameof(Resources.CommandInterfaceWithoutAttributeMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic CommandInterfaceWithoutAttribute(INamedTypeSymbol symbol) => Diagnostic.Create(
        CommandInterfaceWithoutAttributeDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor DefaultValueWithRequiredDescriptor = CreateDiagnosticDescriptor(
        "OCL0020",
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithRequiredMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic DefaultValueWithRequired(ISymbol symbol) => Diagnostic.Create(
        DefaultValueWithRequiredDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor DefaultValueWithMultiValueDescriptor = CreateDiagnosticDescriptor(
        "OCL0020", // Intentionally the same as above.
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithMultiValueMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic DefaultValueWithMultiValue(ISymbol symbol) => Diagnostic.Create(
        DefaultValueWithMultiValueDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor DefaultValueWithMethodDescriptor = CreateDiagnosticDescriptor(
        "OCL0020", // Intentionally the same as above.
        nameof(Resources.DefaultValueIgnoredTitle),
        nameof(Resources.DefaultValueWithMethodMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic DefaultValueWithMethod(ISymbol symbol) => Diagnostic.Create(
        DefaultValueWithMethodDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IsRequiredWithRequiredPropertyDescriptor = CreateDiagnosticDescriptor(
        "OCL0021",
        nameof(Resources.IsRequiredWithRequiredPropertyTitle),
        nameof(Resources.IsRequiredWithRequiredPropertyMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IsRequiredWithRequiredProperty(ISymbol symbol) => Diagnostic.Create(
        IsRequiredWithRequiredPropertyDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor DuplicatePositionDescriptor = CreateDiagnosticDescriptor(
        "OCL0022",
        nameof(Resources.DuplicatePositionTitle),
        nameof(Resources.DuplicatePositionMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic DuplicatePosition(ISymbol symbol, string otherName) => Diagnostic.Create(
        DuplicatePositionDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        otherName);

    public static DiagnosticDescriptor ShortAliasWithoutShortNameDescriptor = CreateDiagnosticDescriptor(
        "OCL0023",
        nameof(Resources.ShortAliasWithoutShortNameTitle),
        nameof(Resources.ShortAliasWithoutShortNameMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic ShortAliasWithoutShortName(AttributeData attribute, ISymbol symbol) => Diagnostic.Create(
        ShortAliasWithoutShortNameDescriptor,
        attribute.GetLocation(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor AliasWithoutLongNameDescriptor = CreateDiagnosticDescriptor(
        "OCL0024",
        nameof(Resources.AliasWithoutLongNameTitle),
        nameof(Resources.AliasWithoutLongNameMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic AliasWithoutLongName(AttributeData attribute, ISymbol symbol) => Diagnostic.Create(
        AliasWithoutLongNameDescriptor,
        attribute.GetLocation(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IsHiddenWithPositionalOrRequiredDescriptor = CreateDiagnosticDescriptor(
        "OCL0025",
        nameof(Resources.IsHiddenWithPositionalOrRequiredTitle),
        nameof(Resources.IsHiddenWithPositionalOrRequiredMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IsHiddenWithPositionalOrRequired(ISymbol symbol) => Diagnostic.Create(
        IsHiddenWithPositionalOrRequiredDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor InvalidGeneratedConverterNamespaceDescriptor = CreateDiagnosticDescriptor(
        "OCL0026",
        nameof(Resources.InvalidGeneratedConverterNamespaceTitle),
        nameof(Resources.InvalidGeneratedConverterNamespaceMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic InvalidGeneratedConverterNamespace(string ns, AttributeData attribute) => Diagnostic.Create(
        InvalidGeneratedConverterNamespaceDescriptor,
        attribute.GetLocation(),
        ns);

    public static DiagnosticDescriptor IgnoredAttributeForNonDictionaryDescriptor = CreateDiagnosticDescriptor(
        "OCL0027",
        nameof(Resources.IgnoredAttributeForNonDictionaryTitle),
        nameof(Resources.IgnoredAttributeForNonDictionaryMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IgnoredAttributeForNonDictionary(ISymbol member, AttributeData attribute) => Diagnostic.Create(
        IgnoredAttributeForNonDictionaryDescriptor,
        attribute.GetLocation(),
        attribute.AttributeClass?.Name,
        member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IgnoredAttributeForDictionaryWithConverterDescriptor = CreateDiagnosticDescriptor(
        "OCL0028",
        nameof(Resources.IgnoredAttributeForDictionaryWithConverterTitle),
        nameof(Resources.IgnoredAttributeForDictionaryWithConverterMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IgnoredAttributeForDictionaryWithConverter(ISymbol member, AttributeData attribute)
        => Diagnostic.Create(
            IgnoredAttributeForDictionaryWithConverterDescriptor,
            attribute.GetLocation(),
            attribute.AttributeClass?.Name,
            member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IgnoredAttributeForNonMultiValueDescriptor = CreateDiagnosticDescriptor(
        "OCL0029",
        nameof(Resources.IgnoredAttributeForNonMultiValueTitle),
        nameof(Resources.IgnoredAttributeForNonMultiValueMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IgnoredAttributeForNonMultiValue(ISymbol member, AttributeData attribute) => Diagnostic.Create(
        IgnoredAttributeForNonMultiValueDescriptor,
        attribute.GetLocation(),
        attribute.AttributeClass?.Name,
        member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor ArgumentStartsWithNumberDescriptor = CreateDiagnosticDescriptor(
        "OCL0030",
        nameof(Resources.ArgumentStartsWithNumberTitle),
        nameof(Resources.ArgumentStartsWithNumberMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic ArgumentStartsWithNumber(ISymbol member, string name) => Diagnostic.Create(
        ArgumentStartsWithNumberDescriptor,
        member.Locations.FirstOrDefault(),
        name,
        member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor NoLongOrShortNameDescriptor = CreateDiagnosticDescriptor(
        "OCL0031",
        nameof(Resources.NoLongOrShortNameTitle),
        nameof(Resources.NoLongOrShortNameMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic NoLongOrShortName(ISymbol member, AttributeData attribute) => Diagnostic.Create(
        NoLongOrShortNameDescriptor,
        attribute.GetLocation(),
        member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IsShortIgnoredDescriptor = CreateDiagnosticDescriptor(
        "OCL0032",
        nameof(Resources.IsShortIgnoredTitle),
        nameof(Resources.IsShortIgnoredMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IsShortIgnored(ISymbol member, AttributeData attribute) => Diagnostic.Create(
        IsShortIgnoredDescriptor,
        attribute.GetLocation(),
        member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor ArgumentWithoutDescriptionDescriptor = CreateDiagnosticDescriptor(
        "OCL0033",
        nameof(Resources.ArgumentWithoutDescriptionTitle),
        nameof(Resources.ArgumentWithoutDescriptionMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic ArgumentWithoutDescription(ISymbol member) => Diagnostic.Create(
        ArgumentWithoutDescriptionDescriptor,
        member.Locations.FirstOrDefault(),
        member.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor CommandWithoutDescriptionDescriptor = CreateDiagnosticDescriptor(
        "OCL0034",
        nameof(Resources.CommandWithoutDescriptionTitle),
        nameof(Resources.CommandWithoutDescriptionMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic CommandWithoutDescription(ISymbol symbol) => Diagnostic.Create(
        CommandWithoutDescriptionDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IgnoredAttributeForNonCommandDescriptor = CreateDiagnosticDescriptor(
        "OCL0035",
        nameof(Resources.IgnoredAttributeForNonCommandTitle),
        nameof(Resources.IgnoredAttributeForNonCommandMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IgnoredAttributeForNonCommand(ISymbol symbol, AttributeData attribute) => Diagnostic.Create(
        IgnoredAttributeForNonCommandDescriptor,
        attribute.GetLocation(),
        attribute.AttributeClass?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor IgnoredFriendlyNameAttributeDescriptor = CreateDiagnosticDescriptor(
        "OCL0036",
        nameof(Resources.IgnoredFriendlyNameAttributeTitle),
        nameof(Resources.IgnoredFriendlyNameAttributeMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic IgnoredFriendlyNameAttribute(ISymbol symbol, AttributeData attribute) => Diagnostic.Create(
        IgnoredFriendlyNameAttributeDescriptor,
        attribute.GetLocation(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor UnsupportedLanguageVersionDescriptor = CreateDiagnosticDescriptor(
        "OCL0037",
        nameof(Resources.UnsupportedLanguageVersionTitle),
        nameof(Resources.UnsupportedLanguageVersionMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic UnsupportedLanguageVersion(ISymbol symbol, string attributeName) => Diagnostic.Create(
        UnsupportedLanguageVersionDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        attributeName);

    public static DiagnosticDescriptor MixedImplicitExplicitPositionsDescriptor = CreateDiagnosticDescriptor(
        "OCL0038",
        nameof(Resources.MixedImplicitExplicitPositionsTitle),
        nameof(Resources.MixedImplicitExplicitPositionsMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic MixedImplicitExplicitPositions(ISymbol symbol) => Diagnostic.Create(
        MixedImplicitExplicitPositionsDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor UnsupportedInitializerSyntaxDescriptor = CreateDiagnosticDescriptor(
        "OCL0039",
        nameof(Resources.UnsupportedInitializerSyntaxTitle),
        nameof(Resources.UnsupportedInitializerSyntaxMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic UnsupportedInitializerSyntax(ISymbol symbol, Location location) => Diagnostic.Create(
        UnsupportedInitializerSyntaxDescriptor,
        location,
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static readonly DiagnosticDescriptor ParserShouldBeGeneratedDescriptor = CreateDiagnosticDescriptor(
        "OCL0040",
        nameof(Resources.ParserShouldBeGeneratedTitle),
        nameof(Resources.ParserShouldBeGeneratedMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic ParserShouldBeGenerated(ISymbol symbol)
        => Diagnostic.Create(
            ParserShouldBeGeneratedDescriptor,
            symbol.Locations.FirstOrDefault(),
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor ValidateEnumInvalidTypeDescriptor = CreateDiagnosticDescriptor(
        "OCL0041",
        nameof(Resources.ValidateEnumInvalidTypeTitle),
        nameof(Resources.ValidateEnumInvalidTypeMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic ValidateEnumInvalidType(ISymbol symbol, ITypeSymbol elementType) => Diagnostic.Create(
        ValidateEnumInvalidTypeDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
        elementType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static DiagnosticDescriptor ValidateEnumWithCustomConverterDescriptor = CreateDiagnosticDescriptor(
        "OCL0042",
        nameof(Resources.ValidateEnumWithCustomConverterTitle),
        nameof(Resources.ValidateEnumWithCustomConverterMessageFormat),
        DiagnosticSeverity.Warning);

    public static Diagnostic ValidateEnumWithCustomConverter(ISymbol symbol) => Diagnostic.Create(
        ValidateEnumWithCustomConverterDescriptor,
        symbol.Locations.FirstOrDefault(),
        symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));


    public static readonly DiagnosticDescriptor CategoryNotEnumDescriptor = CreateDiagnosticDescriptor(
        "OCL0043",
        nameof(Resources.CategoryNotEnumTitle),
        nameof(Resources.CategoryNotEnumMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic CategoryNotEnum(ISymbol symbol)
        => Diagnostic.Create(
            CategoryNotEnumDescriptor,
            symbol.Locations.FirstOrDefault(),
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    public static readonly DiagnosticDescriptor DefaultCategoryNotEnumDescriptor = CreateDiagnosticDescriptor(
        "OCL0043", // Deliberately the same as above.
        nameof(Resources.CategoryNotEnumTitle),
        nameof(Resources.DefaultCategoryNotEnumMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic DefaultCategoryNotEnum(INamedTypeSymbol symbol, AttributeData optionsAttribute)
        => Diagnostic.Create(
            DefaultCategoryNotEnumDescriptor,
            optionsAttribute.GetLocation(),
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));


    public static readonly DiagnosticDescriptor MismatchedCategoryTypeDescriptor = CreateDiagnosticDescriptor(
        "OCL0044",
        nameof(Resources.MismatchedCategoryTypeTitle),
        nameof(Resources.MismatchedCategoryTypeMessageFormat),
        DiagnosticSeverity.Error);

    public static Diagnostic MismatchedCategoryType(ISymbol symbol, ITypeSymbol actualType, ITypeSymbol expectedType)
        => Diagnostic.Create(
            MismatchedCategoryTypeDescriptor,
            symbol.Locations.FirstOrDefault(),
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            actualType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            expectedType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    private static DiagnosticDescriptor CreateDiagnosticDescriptor(string id, string titleResource, string messageResource, DiagnosticSeverity severity)
        => new(
            id,
            new LocalizableResourceString(titleResource, Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(messageResource, Resources.ResourceManager, typeof(Resources)),
            Category,
            severity,
            isEnabledByDefault: true,
            helpLinkUri: $"https://www.ookii.org/Link/CommandLineGeneratorError#{id.ToLowerInvariant()}");
}
