﻿using Microsoft.CodeAnalysis;
using System;
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
        // Find the attribute that can apply to an arguments class.
        // This code also finds attributes that inherit from those attribute. By instantiating the
        // possibly derived attribute classes, we can support for example a class that derives from
        // DescriptionAttribute that gets the description from a resource.
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
                CheckAttribute(attribute, AttributeNames.Command, ref commandAttribute) ||
                CheckAttribute(attribute, AttributeNames.ClassValidation, ref classValidators))
            {
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
        AttributeData? multiValueSeparator = null;
        AttributeData? description = null;
        AttributeData? allowDuplicateDictionaryKeys = null;
        AttributeData? keyValueSeparator = null;
        AttributeData? converter = null;
        List<AttributeData>? aliases = null;
        List<AttributeData>? shortAliases = null;
        List<AttributeData>? validators = null;
        foreach (var attribute in member.GetAttributes())
        {
            if (CheckAttribute(attribute, AttributeNames.CommandLineArgument, ref commandLineArgumentAttribute) ||
                CheckAttribute(attribute, AttributeNames.MultiValueSeparator, ref multiValueSeparator) ||
                CheckAttribute(attribute, AttributeNames.Description, ref description) ||
                CheckAttribute(attribute, AttributeNames.AllowDuplicateDictionaryKeys, ref allowDuplicateDictionaryKeys) ||
                CheckAttribute(attribute, AttributeNames.KeyValueSeparator, ref keyValueSeparator) ||
                CheckAttribute(attribute, AttributeNames.ArgumentConverter, ref converter) ||
                CheckAttribute(attribute, AttributeNames.Alias, ref aliases) ||
                CheckAttribute(attribute, AttributeNames.ShortAlias, ref shortAliases) ||
                CheckAttribute(attribute, AttributeNames.ArgumentValidation, ref validators))
            {
                continue;
            }

            _context.ReportDiagnostic(Diagnostics.UnknownAttribute(attribute));
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

        // TODO: key/value converters
        // TODO: AllowsNull

        // The leading commas are not a formatting I like but it does make things easier here.
        _builder.AppendLine($"yield return Ookii.CommandLine.Support.GeneratedArgument.Create(");
        _builder.IncreaseIndent();
        _builder.AppendLine("parser");
        _builder.AppendLine($", argumentType: typeof({argumentType.ToDisplayString()})");
        _builder.AppendLine($", memberName: \"{member.Name}\"");
        _builder.AppendLine($", attribute: {commandLineArgumentAttribute.CreateInstantiation()}");
        _builder.AppendLine(", converter: Ookii.CommandLine.Conversion.StringConverter.Instance");
        if (multiValueSeparator != null)
        {
            _builder.AppendLine($", multiValueSeparatorAttribute: {multiValueSeparator.CreateInstantiation()}");
        }

        if (description != null)
        {
            _builder.AppendLine($", descriptionAttribute: {description.CreateInstantiation()}");
        }

        if (allowDuplicateDictionaryKeys != null)
        {
            _builder.AppendLine(", allowDuplicateDictionaryKeys: true");
        }

        if (keyValueSeparator != null)
        {
            _builder.AppendLine($", keyValueSeparatorAttribute: {keyValueSeparator.CreateInstantiation()}");
        }

        if (aliases != null)
        {
            _builder.AppendLine($", aliasAttributes: new Ookii.CommandLine.AliasAttribute[] {{ {string.Join(", ", aliases.Select(a => a.CreateInstantiation()))} }}");
        }

        if (shortAliases != null)
        {
            _builder.AppendLine($", shortAliasAttributes: new Ookii.CommandLine.ShortAliasAttribute[] {{ {string.Join(", ", shortAliases.Select(a => a.CreateInstantiation()))} }}");
        }

        if (validators != null)
        {
            _builder.AppendLine($", validationAttributes: new Ookii.CommandLine.Validation.ArgumentValidationAttribute[] {{ {string.Join(", ", validators.Select(a => a.CreateInstantiation()))} }}");
        }

        _builder.AppendLine($", setProperty: (target, value) => (({_argumentsClass.Name})target).{member.Name} = ({nullableArgumentType.ToDisplayString()})value{extra}");
        _builder.DecreaseIndent();
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

    // Using a ref parameter with bool return allows me to chain these together.
    private static bool CheckAttribute(AttributeData data, string name, ref List<AttributeData>? attributes)
    {
        if (!(data.AttributeClass?.DerivesFrom(name) ?? false))
        {
            return false;
        }

        attributes ??= new();
        attributes.Add(data);
        return true;
    }
}
