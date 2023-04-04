using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal static class ParserGenerator
{
    public static string Generate(SourceProductionContext context, INamedTypeSymbol symbol)
    {
        // TODO: Make sure it's a reference type and partial.
        if (symbol.IsGenericType)
        {
            // TODO: Helper for reporting diagnostics. Maybe use exceptions?
            // TODO: Use resources using LocalizableString
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("CL0001", "Generic arguments class", "The arguments class {0} may not be a generic class when the GeneratedParserAttribute is used.", "Ookii.CommandLine", DiagnosticSeverity.Error, true), symbol.Locations.FirstOrDefault(), symbol.ToDisplayString()));
            return string.Empty;
        }

        var builder = new SourceBuilder(symbol.ContainingNamespace);
        builder.AppendLine($"partial class {symbol.Name}");
        builder.OpenBlock();
        GenerateProvider(builder, symbol);
        builder.AppendLine($"public static Ookii.CommandLine.CommandLineParser<{symbol.Name}> CreateParser(Ookii.CommandLine.ParseOptions? options = null) => new(new GeneratedProvider(), options);");
        builder.AppendLine();
        var nullableType = symbol.WithNullableAnnotation(NullableAnnotation.Annotated);
        builder.AppendLine($"public static {nullableType.ToDisplayString()} Parse(Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling();");
        builder.AppendLine();
        builder.AppendLine($"public static {nullableType.ToDisplayString()} Parse(string[] args, Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling(args);");
        builder.AppendLine();
        builder.AppendLine($"public static {nullableType.ToDisplayString()} Parse(string[] args, int index, Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling(args, index);");
        builder.CloseBlock(); // class
        return builder.GetSource();
    }

    private static void GenerateProvider(SourceBuilder builder, INamedTypeSymbol symbol)
    {
        builder.AppendLine("private class GeneratedProvider : Ookii.CommandLine.Support.GeneratedArgumentProvider");
        builder.OpenBlock();
        // TODO: attributes
        builder.AppendLine($"public GeneratedProvider() : base(typeof({symbol.Name}), null, System.Linq.Enumerable.Empty<Ookii.CommandLine.Validation.ClassValidationAttribute>(), null, null) {{}}");
        builder.AppendLine();
        // TODO: IsCommand
        builder.AppendLine("public override bool IsCommand => false;");
        builder.AppendLine();
        // TODO: Injection
        builder.AppendLine($"public override object CreateInstance(Ookii.CommandLine.CommandLineParser parser) => new {symbol.Name}();");
        builder.AppendLine();
        builder.AppendLine("public override System.Collections.Generic.IEnumerable<Ookii.CommandLine.CommandLineArgument> GetArguments(Ookii.CommandLine.CommandLineParser parser)");
        builder.OpenBlock();

        //Debugger.Launch();
        foreach (var member in symbol.GetMembers())
        {
            GenerateArgument(builder, symbol.Name, member);
        }

        // Makes sure the function compiles if there are no arguments.
        builder.AppendLine("yield break;");
        builder.CloseBlock(); // GetArguments()
        builder.CloseBlock(); // GeneratedProvider class
    }

    private static void GenerateArgument(SourceBuilder builder, string className, ISymbol member)
    {
        // Check if the member can be an argument.
        if (member.DeclaredAccessibility != Accessibility.Public ||
            member.Kind is not (SymbolKind.Method or SymbolKind.Property))
        {
            return;
        }

        AttributeData? commandLineArgumentAttribute = null;
        foreach (var attribute in member.GetAttributes())
        {
            if (attribute.AttributeClass == null)
            {
                continue;
            }

            if (attribute.AttributeClass.DerivesFrom(AttributeNames.CommandLineArgument))
            {
                commandLineArgumentAttribute = attribute;
            }
        }

        // Check if it is an attribute.
        if (commandLineArgumentAttribute == null)
        {
            return;
        }

        var property = member as IPropertySymbol;
        var method = member as IMethodSymbol;
        if (method != null)
        {
            throw new NotImplementedException();
        }

        var argumentType = (INamedTypeSymbol)property!.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        var nullableArgumentType = argumentType.WithNullableAnnotation(NullableAnnotation.Annotated);
        string extra = string.Empty;
        if (!argumentType.IsReferenceType && !argumentType.IsNullableValueType())
        {
            extra = "!";
        }

        // The leading commas are not a formatting I like but it does make things easier here.
        builder.AppendLine($"yield return Ookii.CommandLine.Support.GeneratedArgument.Create(");
        builder.AppendLine("    parser");
        builder.AppendLine($"    , argumentType: typeof({argumentType.ToDisplayString()})");
        builder.AppendLine($"    , memberName: \"{member.Name}\"");
        builder.AppendLine($"    , attribute: {commandLineArgumentAttribute.CreateInstantiation()}");
        builder.AppendLine("    , converter: Ookii.CommandLine.Conversion.StringConverter.Instance");
        builder.AppendLine($"    , setProperty: (target, value) => (({className})target).{member.Name} = ({nullableArgumentType.ToDisplayString()})value{extra}");
        builder.AppendLine($");");
    }
}
