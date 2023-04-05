using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class ParserGenerator
{
    private readonly SourceProductionContext _context;
    private readonly INamedTypeSymbol _argumentsClass;
    private readonly SourceBuilder _builder;

    public ParserGenerator(SourceProductionContext context, INamedTypeSymbol argumentsClass)
    {
        _context = context;
        _argumentsClass = argumentsClass;
        _builder = new(argumentsClass.ContainingNamespace);
    }

    public static string? Generate(SourceProductionContext context, INamedTypeSymbol _argumentsClass)
    {
        var generator = new ParserGenerator(context, _argumentsClass);
        return generator.Generate();
    }

    public string? Generate()
    {
        _builder.AppendLine($"partial class {_argumentsClass.Name}");
        _builder.OpenBlock();
        GenerateProvider();
        _builder.AppendLine($"public static Ookii.CommandLine.CommandLineParser<{_argumentsClass.Name}> CreateParser(Ookii.CommandLine.ParseOptions? options = null) => new(new GeneratedProvider(), options);");
        _builder.AppendLine();
        var nullableType = _argumentsClass.WithNullableAnnotation(NullableAnnotation.Annotated);
        _builder.AppendLine($"public static {nullableType.ToDisplayString()} Parse(Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling();");
        _builder.AppendLine();
        _builder.AppendLine($"public static {nullableType.ToDisplayString()} Parse(string[] args, Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling(args);");
        _builder.AppendLine();
        _builder.AppendLine($"public static {nullableType.ToDisplayString()} Parse(string[] args, int index, Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling(args, index);");
        _builder.CloseBlock(); // class
        return _builder.GetSource();
    }

    private void GenerateProvider()
    {
        AttributeData? parseOptions = null;
        AttributeData? description = null;
        AttributeData? applicationFriendlyName = null;
        AttributeData? commandAttribute = null;
        List<AttributeData>? classValidators = null;
        foreach (var attribute in _argumentsClass.GetAttributes())
        {
            if (CheckAttribute(attribute, AttributeNames.ParseOptions, ref parseOptions) ||
                CheckAttribute(attribute, AttributeNames.Description, ref description) ||
                CheckAttribute(attribute, AttributeNames.ApplicationFriendlyName, ref applicationFriendlyName) ||
                CheckAttribute(attribute, AttributeNames.Command, ref commandAttribute))
            {
                continue;
            }

            if (attribute.AttributeClass?.DerivesFrom(AttributeNames.ClassValidation) ?? false)
            {
                classValidators ??= new();
                classValidators.Add(attribute);
                continue;
            }

            if (!attribute.AttributeClass?.DerivesFrom(AttributeNames.GeneratedParser) ?? false)
            {
                _context.ReportDiagnostic(Diagnostics.UnknownAttribute(attribute));
            }
        }

        _builder.AppendLine("private class GeneratedProvider : Ookii.CommandLine.Support.GeneratedArgumentProvider");
        _builder.OpenBlock();
        _builder.AppendLine("public GeneratedProvider()");
        _builder.IncreaseIndent();
        _builder.AppendLine($": base(typeof({_argumentsClass.Name}),");
        _builder.AppendLine($"       {parseOptions?.CreateInstantiation() ?? "null"},");
        if (classValidators == null)
        {
            _builder.AppendLine($"       null,");
        }
        else
        {
            _builder.AppendLine($"       new Ookii.CommandLine.Validation.ClassValidationAttribute[] {{ {string.Join(", ", classValidators.Select(v => v.CreateInstantiation()))} }},");
        }

        _builder.AppendLine($"       {applicationFriendlyName?.CreateInstantiation() ?? "null"},");
        _builder.AppendLine($"       {description?.CreateInstantiation() ?? "null"})");
        _builder.DecreaseIndent();
        _builder.AppendLine("{}");
        _builder.AppendLine();
        // TODO: IsCommand
        _builder.AppendLine("public override bool IsCommand => false;");
        _builder.AppendLine();
        // TODO: Injection
        _builder.AppendLine($"public override object CreateInstance(Ookii.CommandLine.CommandLineParser parser) => new {_argumentsClass.Name}();");
        _builder.AppendLine();
        _builder.AppendLine("public override System.Collections.Generic.IEnumerable<Ookii.CommandLine.CommandLineArgument> GetArguments(Ookii.CommandLine.CommandLineParser parser)");
        _builder.OpenBlock();

        //Debugger.Launch();
        foreach (var member in _argumentsClass.GetMembers())
        {
            GenerateArgument(member);
        }

        // Makes sure the function compiles if there are no arguments.
        _builder.AppendLine("yield break;");
        _builder.CloseBlock(); // GetArguments()
        _builder.CloseBlock(); // GeneratedProvider class
    }

    private void GenerateArgument(ISymbol member)
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
        _builder.AppendLine($"yield return Ookii.CommandLine.Support.GeneratedArgument.Create(");
        _builder.AppendLine("    parser");
        _builder.AppendLine($"    , argumentType: typeof({argumentType.ToDisplayString()})");
        _builder.AppendLine($"    , memberName: \"{member.Name}\"");
        _builder.AppendLine($"    , attribute: {commandLineArgumentAttribute.CreateInstantiation()}");
        _builder.AppendLine("    , converter: Ookii.CommandLine.Conversion.StringConverter.Instance");
        _builder.AppendLine($"    , setProperty: (target, value) => (({_argumentsClass.Name})target).{member.Name} = ({nullableArgumentType.ToDisplayString()})value{extra}");
        _builder.AppendLine($");");
    }

    // Using a ref parameter with bool return allows me to chain these together.
    private static bool CheckAttribute(AttributeData data, string name, ref AttributeData? attribute)
    {
        if (attribute != null || !(data.AttributeClass?.DerivesFrom(name) ?? false))
        {
            return false;
        }

        attribute = data;
        return true;
    }
}
