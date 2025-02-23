using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Ookii.CommandLine.Generator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParserAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        Diagnostics.ParserShouldBeGeneratedDescriptor,
        Diagnostics.CategoryNotEnumDescriptor,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var symbol = (INamedTypeSymbol)context.Symbol;
        var tree = symbol.Locations[0].SourceTree;
        var languageVersion = (tree?.Options as CSharpParseOptions)?.LanguageVersion ?? LanguageVersion.CSharp1;
        if (languageVersion < LanguageVersion.CSharp8 ||
            symbol.IsAbstract ||
            symbol.ContainingType != null ||
            symbol.IsGenericType ||
            !symbol.IsReferenceType)
        {
            // Unsupported.
            return;
        }

        var typeHelper = new TypeHelper(context.Compilation);
        var argumentAttributeType = typeHelper.CommandLineArgumentAttribute;
        if (typeHelper.GeneratedParserAttribute == null || argumentAttributeType == null)
        {
            // Required types don't exist somehow.
            return;
        }

        // This deliberately excludes base class attributes so the error only gets emitted once.
        // N.B. This check is performed here even for types with the GeneratedParserAttribute.
        var optionsAttribute = symbol.GetAttribute(typeHelper.ParseOptionsAttribute!);
        if (optionsAttribute != null)
        {
            var defaultCategory = optionsAttribute.GetNamedArgument("DefaultArgumentCategory");
            if (defaultCategory is TypedConstant category && !category.IsNull && category.Kind is not TypedConstantKind.Enum or TypedConstantKind.Error)
            {
                context.ReportDiagnostic(Diagnostics.DefaultCategoryNotEnum(symbol, optionsAttribute));
            }
        }

        if (symbol.GetAttribute(typeHelper.GeneratedParserAttribute) != null)
        {
            // Class is already using the attribute.
            return;
        }

        bool reportMissingGeneratedAttribute = true;
        foreach (var member in symbol.GetMembers())
        {
            if (member.DeclaredAccessibility == Accessibility.Public &&
                member.Kind is SymbolKind.Property or SymbolKind.Method)
            {
                var attribute = member.GetAttribute(argumentAttributeType);
                if (attribute == null)
                {
                    continue;
                }

                // Found a member with the CommandLineArgumentAttribute on a type that doesn't
                // have the GeneratedParserAttribute.
                if (reportMissingGeneratedAttribute)
                {
                    context.ReportDiagnostic(Diagnostics.ParserShouldBeGenerated(symbol));
                    reportMissingGeneratedAttribute = false;
                }

                // Check for non-enum category.
                // Matching types is only checked by the generator, as that would require checking
                // the base types too.
                var info = new CommandLineArgumentAttributeInfo(attribute);
                if (!info.Category.IsNull && info.Category.Kind is not (TypedConstantKind.Enum or TypedConstantKind.Error))
                {
                    context.ReportDiagnostic(Diagnostics.CategoryNotEnum(member));
                }
            }
        }
    }
}
