using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Ookii.CommandLine.Generator;

[Generator]
public class ParserIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                static (ctx, _) => GetClassToGenerate(ctx)
            )
            .Where(static c => c != null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var converterGenerator = new ConverterGenerator(compilation);
        foreach (var syntax in classes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken) is not INamedTypeSymbol symbol)
            {
                continue;
            }

            if (!symbol.IsReferenceType)
            {
                context.ReportDiagnostic(Diagnostics.ArgumentsTypeNotReferenceType(symbol));
                continue;
            }

            if (!syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostics.ArgumentsClassNotPartial(symbol));
                continue;
            }

            if (symbol.IsGenericType)
            {
                context.ReportDiagnostic(Diagnostics.ArgumentsClassIsGeneric(symbol));
                continue;
            }

            var source = ParserGenerator.Generate(compilation, context, symbol, converterGenerator);
            if (source != null)
            {
                // TODO: File name should include namespace.
                context.AddSource(symbol.Name + ".g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        var converterSource = converterGenerator.Generate();
        if (converterSource != null)
        {
            context.AddSource("GeneratedConverters.g.cs", SourceText.From(converterSource, Encoding.UTF8));
        }
    }

    private static ClassDeclarationSyntax? GetClassToGenerate(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // No symbol for the attribute for some reason.
                    continue;
                }

                var attributeType = attributeSymbol.ContainingType;
                var name = attributeType.ToDisplayString();
                if (name == AttributeNames.GeneratedParser)
                {
                    return classDeclaration;
                }
            }
        }

        return null;
    }
}
