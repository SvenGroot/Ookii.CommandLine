using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class ParserGenerator
{
    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    private readonly INamedTypeSymbol _argumentsClass;
    private readonly SourceBuilder _builder;
    private readonly ConverterGenerator _converterGenerator;

    public ParserGenerator(Compilation compilation, SourceProductionContext context, INamedTypeSymbol argumentsClass, ConverterGenerator converterGenerator)
    {
        _compilation = compilation;
        _context = context;
        _argumentsClass = argumentsClass;
        _builder = new(argumentsClass.ContainingNamespace);
        _converterGenerator = converterGenerator;
    }

    public static string? Generate(Compilation compilation, SourceProductionContext context, INamedTypeSymbol argumentsClass, ConverterGenerator converterGenerator)
    {
        var generator = new ParserGenerator(compilation, context, argumentsClass, converterGenerator);
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
        // Check if the member can be an argument. TODO: warning if private.
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
        AttributeData? converterAttribute = null;
        AttributeData? keyConverterAttribute = null;
        AttributeData? valueConverterAttribute = null;
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
                CheckAttribute(attribute, AttributeNames.ArgumentConverter, ref converterAttribute) ||
                CheckAttribute(attribute, AttributeNames.KeyConverter, ref keyConverterAttribute) ||
                CheckAttribute(attribute, AttributeNames.ValueConverter, ref valueConverterAttribute) ||
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

        if (property == null || property.IsStatic)
        {
            return;
        }

        var originalArgumentType = property!.Type;
        var argumentType = originalArgumentType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        var notNullAnnotation = string.Empty;
        var allowsNull = originalArgumentType.AllowsNull();
        if (allowsNull)
        {
            // Needed in case the original definition was in a context without NRT support.
            originalArgumentType = originalArgumentType.WithNullableAnnotation(NullableAnnotation.Annotated);
        }
        else
        {
            notNullAnnotation = "!";
        }

        var elementTypeWithNullable = argumentType;
        var namedElementTypeWithNullable = elementTypeWithNullable as INamedTypeSymbol;
        ITypeSymbol? keyType = null;
        ITypeSymbol? valueType = null;
        if (keyValueSeparator != null)
        {
            _builder.AppendLine($"var keyValueSeparatorAttribute{member.Name} = {keyValueSeparator.CreateInstantiation()};");
        }

        var kind = "Ookii.CommandLine.ArgumentKind.SingleValue";
        string? converter = null;
        if (property != null)
        {
            var multiValueType = DetermineMultiValueType(property, argumentType);
            if (multiValueType is not var (collectionType, dictionaryType, multiValueElementType))
            {
                return;
            }

            if (dictionaryType != null)
            {
                Debug.Assert(multiValueElementType != null);
                kind = "Ookii.CommandLine.ArgumentKind.Dictionary";
                elementTypeWithNullable = multiValueElementType!;
                // KeyValuePair is guaranteed a named type.
                namedElementTypeWithNullable = (INamedTypeSymbol)elementTypeWithNullable;
                keyType = namedElementTypeWithNullable.TypeArguments[0].WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                var rawValueType = namedElementTypeWithNullable.TypeArguments[1];
                allowsNull = rawValueType.AllowsNull();
                valueType = rawValueType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                if (converterAttribute == null)
                {
                    var keyConverter = DetermineConverter(keyType.GetUnderlyingType(), keyConverterAttribute, keyType.IsNullableValueType());
                    if (keyConverter == null)
                    {
                        _context.ReportDiagnostic(Diagnostics.NoConverter(member, keyType.GetUnderlyingType()));
                        return;
                    }

                    var valueConverter = DetermineConverter(valueType.GetUnderlyingType(), valueConverterAttribute, valueType.IsNullableValueType());
                    if (valueConverter == null)
                    {
                        _context.ReportDiagnostic(Diagnostics.NoConverter(member, keyType.GetUnderlyingType()));
                        return;
                    }

                    var separator = keyValueSeparator == null 
                        ? "null"
                        : $"keyValueSeparatorAttribute{member.Name}.Separator";

                    converter = $"new Ookii.CommandLine.Conversion.KeyValuePairConverter<{keyType.ToDisplayString()}, {rawValueType.ToDisplayString()}>({keyConverter}, {valueConverter}, {separator}, {allowsNull.ToCSharpString()})";
                }
            }
            else if (collectionType != null)
            {
                Debug.Assert(multiValueElementType != null);
                kind = "Ookii.CommandLine.ArgumentKind.MultiValue";
                elementTypeWithNullable = multiValueElementType!;
                namedElementTypeWithNullable = elementTypeWithNullable as INamedTypeSymbol;
                allowsNull = elementTypeWithNullable.AllowsNull();
            }
        }
        else
        {
            kind = "Ookii.CommandLine.ArgumentKind.Method";
        }

        var elementType = namedElementTypeWithNullable?.GetUnderlyingType() ?? elementTypeWithNullable;
        converter ??= DetermineConverter(elementType, converterAttribute, ((INamedTypeSymbol)elementTypeWithNullable).IsNullableValueType());
        if (converter == null)
        {
            _context.ReportDiagnostic(Diagnostics.NoConverter(member, elementType));
            return;
        }

        // TODO: Default value description. Can make DetermineValueDescription abstract and move
        // to ReflectionArgument when done.
        // The leading commas are not a formatting I like but it does make things easier here.
        _builder.AppendLine($"yield return Ookii.CommandLine.Support.GeneratedArgument.Create(");
        _builder.IncreaseIndent();
        _builder.AppendLine("parser");
        _builder.AppendLine($", argumentType: typeof({argumentType.ToDisplayString()})");
        _builder.AppendLine($", elementTypeWithNullable: typeof({elementTypeWithNullable.ToDisplayString()})");
        _builder.AppendLine($", elementType: typeof({elementType.ToDisplayString()})");
        _builder.AppendLine($", memberName: \"{member.Name}\"");
        _builder.AppendLine($", kind: {kind}");
        _builder.AppendLine($", attribute: {commandLineArgumentAttribute.CreateInstantiation()}");
        _builder.AppendLine($", converter: {converter}");
        _builder.AppendLine($", allowsNull: {(allowsNull.ToCSharpString())}");
        if (keyType != null)
        {
            _builder.AppendLine($", keyType: typeof({keyType.ToDisplayString()})");
        }

        if (valueType != null)
        {
            _builder.AppendLine($", valueType: typeof({valueType.ToDisplayString()})");
        }

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
            _builder.AppendLine($", keyValueSeparatorAttribute: keyValueSeparatorAttribute{member.Name}");
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

        if (property?.SetMethod?.DeclaredAccessibility == Accessibility.Public)
        {
            _builder.AppendLine($", setProperty: (target, value) => (({_argumentsClass.Name})target).{member.Name} = ({originalArgumentType.ToDisplayString()})value{notNullAnnotation}");
        }

        if (property != null)
        {
            _builder.AppendLine($", getProperty: (target) => (({_argumentsClass.Name})target).{member.Name}");
        }

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

    private (ITypeSymbol?, INamedTypeSymbol?, ITypeSymbol?)? DetermineMultiValueType(IPropertySymbol property, ITypeSymbol argumentType)
    {
        if (argumentType is INamedTypeSymbol namedType)
        {
            // If the type is Dictionary<TKey, TValue> it doesn't matter if the property is
            // read-only or not.
            if (namedType.IsGenericType && namedType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.Dictionary<TKey, TValue>")
            {
                var keyValuePair = _compilation.GetTypeByMetadataName(typeof(KeyValuePair<,>).FullName)!;
                var elementType = keyValuePair.Construct(namedType.TypeArguments, namedType.TypeArgumentNullableAnnotations);
                return (null, namedType, elementType);
            }
        }

        if (argumentType is IArrayTypeSymbol arrayType)
        {
            if (arrayType.Rank != 1)
            {
                _context.ReportDiagnostic(Diagnostics.InvalidArrayRank(property));
                return null;
            }

            if (property.SetMethod?.DeclaredAccessibility != Accessibility.Public)
            {
                _context.ReportDiagnostic(Diagnostics.PropertyIsReadOnly(property));
                return null;
            }

            var elementType = arrayType.ElementType;
            return (argumentType, null, elementType);
        }

        // The interface approach requires a read-only property. If it's read-write, treat it
        // like a non-multi-value argument.
        if (property.SetMethod?.DeclaredAccessibility == Accessibility.Public)
        {
            return (null, null, null);
        }

        var dictionaryType = argumentType.FindGenericInterface("System.Collections.Generic.IDictionary<TKey, TValue>");
        if (dictionaryType != null)
        {
            var keyValuePair = _compilation.GetTypeByMetadataName(typeof(KeyValuePair<,>).FullName)!;
            var elementType = keyValuePair.Construct(dictionaryType.TypeArguments, dictionaryType.TypeArgumentNullableAnnotations);
            return (null, dictionaryType, elementType);
        }

        var collectionType = argumentType.FindGenericInterface("System.Collections.Generic.ICollection<T>");
        if (collectionType != null)
        {
            var elementType = collectionType.TypeArguments[0];
            return (collectionType, null, elementType);
        }

        // This is a read-only property with an unsupported type.
        _context.ReportDiagnostic(Diagnostics.PropertyIsReadOnly(property));
        return null;
    }

    public string? DetermineConverter(ITypeSymbol elementType, AttributeData? converterAttribute, bool isNullableValueType)
    {
        var converter = DetermineElementConverter(elementType, converterAttribute);
        if (converter != null && isNullableValueType)
        {
            converter = $"new Ookii.CommandLine.Conversion.NullableConverter({converter})";
        }

        return converter;
    }

    public string? DetermineElementConverter(ITypeSymbol elementType, AttributeData? converterAttribute)
    {
        if (converterAttribute != null)
        {
            var argument = converterAttribute.ConstructorArguments[0];
            if (argument.Kind != TypedConstantKind.Type)
            {
                // TODO: Either support this or emit error.
                throw new NotSupportedException();
            }

            var converterType = (INamedTypeSymbol)argument.Value!;
            return $"new {converterType.ToDisplayString()}()";
        }

        var typeName = elementType.ToDisplayString();
        switch (typeName)
        {
        case "string":
            return "Ookii.CommandLine.Conversion.StringConverter.Instance";

        case "bool":
            return "Ookii.CommandLine.Conversion.BooleanConverter.Instance";
        }

        if (elementType.IsEnum())
        {
            return $"new Ookii.CommandLine.Conversion.EnumConverter(typeof({elementType.ToDisplayString()}))";
        }

        if (elementType.ImplementsInterface($"System.ISpanParsable<{elementType.ToDisplayString()}>"))
        {
            return $"new Ookii.CommandLine.Conversion.SpanParsableConverter<{elementType.ToDisplayString()}>()";
        }

        if (elementType.ImplementsInterface($"System.IParsable<{elementType.ToDisplayString()}>"))
        {
            return $"new Ookii.CommandLine.Conversion.ParsableConverter<{elementType.ToDisplayString()}>()";
        }

        return _converterGenerator.GetConverter(elementType);
    }
}
