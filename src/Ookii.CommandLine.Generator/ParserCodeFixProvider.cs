using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;

namespace Ookii.CommandLine.Generator;
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParserCodeFixProvider)), Shared]
public class ParserCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Diagnostics.ParserShouldBeGeneratedDescriptor.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var span = diagnostic.Location.SourceSpan;

        // Find the type declaration.
        var declaration = root?.FindToken(span.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (declaration == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                Properties.Resources.GeneratedParserCodeFixTitle,
                (token) => AddGeneratedParserAttribute(context.Document, declaration, token),
                nameof(Properties.Resources.GeneratedParserCodeFixTitle)),
            diagnostic);
    }

    private static async Task<Document> AddGeneratedParserAttribute(Document document, TypeDeclarationSyntax declaration,
        CancellationToken token)
    {
        var attr = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("GeneratedParser"));
        var attrList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attr));
        
        // Add the attribute.
        var newDeclaration = declaration.AddAttributeLists(attrList);

        // Add partial keyword if not already there.
        if (!newDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        { 
            newDeclaration = newDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        }

        newDeclaration = newDeclaration.WithAdditionalAnnotations(Formatter.Annotation);
        if (await document.GetSyntaxRootAsync(token).ConfigureAwait(false) is not CompilationUnitSyntax oldRoot)
        {
            return document;
        }

        var newRoot = oldRoot.ReplaceNode(declaration, newDeclaration);

        // Add a using statement if needed.
        if (!oldRoot.Usings.Any(u => u.Name.ToString() == "Ookii.CommandLine"))
        {
            newRoot = newRoot.AddUsings(
                SyntaxFactory.UsingDirective(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName("Ookii"),
                        SyntaxFactory.IdentifierName("CommandLine"))));
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
