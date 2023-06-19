using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Ookii.CommandLine.Generator;

[Generator]
public class ParserIncrementalGenerator : IIncrementalGenerator
{
    private enum ClassKind
    {
        Arguments,
        CommandManager,
        Command,
    }

    private record struct ClassInfo(ClassDeclarationSyntax Syntax, ClassKind ClassKind);

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

    private static void Execute(Compilation compilation, ImmutableArray<ClassInfo?> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var typeHelper = new TypeHelper(compilation);
        var converterGenerator = new ConverterGenerator(typeHelper, context);
        var commandGenerator = new CommandGenerator(typeHelper, context);
        foreach (var cls in classes)
        {
            var info = cls!.Value;
            var syntax = info.Syntax;
            context.CancellationToken.ThrowIfCancellationRequested();
            var languageVersion = (info.Syntax.SyntaxTree.Options as CSharpParseOptions)?.LanguageVersion ?? LanguageVersion.CSharp1;
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(syntax, context.CancellationToken) is not INamedTypeSymbol symbol)
            {
                continue;
            }

            // If this is a command without the GeneratedParserAttribute, add it and do nothing
            // else.
            if (info.ClassKind == ClassKind.Command)
            {
                if (symbol.ImplementsInterface(typeHelper.ICommand))
                {
                    commandGenerator.AddCommand(symbol);
                }
                else
                {
                    context.ReportDiagnostic(Diagnostics.CommandAttributeWithoutInterface(symbol));
                }

                continue;
            }

            var attributeName = info.ClassKind == ClassKind.CommandManager
                ? typeHelper.GeneratedCommandManagerAttribute!.Name
                : typeHelper.GeneratedParserAttribute!.Name;

            if (languageVersion < LanguageVersion.CSharp8)
            {
                context.ReportDiagnostic(Diagnostics.UnsupportedLanguageVersion(symbol, attributeName));
                continue;
            }

            if (!symbol.IsReferenceType)
            {
                context.ReportDiagnostic(Diagnostics.TypeNotReferenceType(symbol, attributeName));
                continue;
            }

            if (!syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostics.ClassNotPartial(symbol, attributeName));
                continue;
            }

            if (symbol.IsGenericType)
            {
                context.ReportDiagnostic(Diagnostics.ClassIsGeneric(symbol, attributeName));
                continue;
            }

            if (symbol.ContainingType != null)
            {
                context.ReportDiagnostic(Diagnostics.ClassIsNested(symbol, attributeName));
                continue;
            }

            if (info.ClassKind == ClassKind.CommandManager)
            {
                commandGenerator.AddManager(symbol);
                continue;
            }

            var source = ParserGenerator.Generate(context, symbol, typeHelper, converterGenerator, commandGenerator,
                languageVersion);

            if (source != null)
            {
                context.AddSource(symbol.ToDisplayString().ToIdentifier(".g.cs"), SourceText.From(source, Encoding.UTF8));
            }
        }

        var converterSource = converterGenerator.Generate();
        if (converterSource != null)
        {
            context.AddSource("GeneratedConverters.g.cs", SourceText.From(converterSource, Encoding.UTF8));
        }

        commandGenerator.Generate();
    }

    private static ClassInfo? GetClassToGenerate(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var typeHelper = new TypeHelper(context.SemanticModel.Compilation);
        var generatedParserType = typeHelper.GeneratedParserAttribute;
        var GeneratedCommandManagerType = typeHelper.GeneratedCommandManagerAttribute;
        var commandType = typeHelper.CommandAttribute;
        var isCommand = false;
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
                if (attributeType.SymbolEquals(generatedParserType))
                {
                    return new(classDeclaration, ClassKind.Arguments);
                }

                if (attributeType.SymbolEquals(GeneratedCommandManagerType))
                {
                    return new(classDeclaration, ClassKind.CommandManager);
                }

                if (attributeType.SymbolEquals(commandType))
                {
                    isCommand = true;
                }
            }
        }

        return isCommand ? new(classDeclaration, ClassKind.Command) : null;
    }
}
