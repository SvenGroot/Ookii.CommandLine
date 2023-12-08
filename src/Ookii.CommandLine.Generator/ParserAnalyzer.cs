using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Ookii.CommandLine.Generator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParserAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostics.ParserShouldBeGeneratedDescriptor);

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
        if (typeHelper.GeneratedParserAttribute == null || typeHelper.CommandLineArgumentAttribute == null)
        {
            // Required types don't exist somehow.
            return;
        }

        if (symbol.GetAttribute(typeHelper.GeneratedParserAttribute) != null)
        {
            // Class is already using the attribute.
            return;
        }

        var argumentAttribute = typeHelper.CommandLineArgumentAttribute;
        foreach (var member in symbol.GetMembers())
        {
            if (member.DeclaredAccessibility == Accessibility.Public &&
                member.Kind is SymbolKind.Property or SymbolKind.Method)
            {
                if (member.GetAttribute(argumentAttribute) != null)
                {
                    // Found a member with the CommandLineArgumentAttribute on a type that doesn't
                    // have the GeneratedParserAttribute.
                    context.ReportDiagnostic(Diagnostics.ParserShouldBeGenerated(symbol));
                    break;
                }
            }
        }
    }
}
