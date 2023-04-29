﻿using Microsoft.CodeAnalysis;
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
    private struct MethodArgumentInfo
    {
        public ITypeSymbol ArgumentType { get; set; }
        public bool HasValueParameter { get; set; }
        public bool HasParserParameter { get; set; }
        public bool HasBooleanReturn { get; set; }
    }

    private readonly TypeHelper _typeHelper;
    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    private readonly INamedTypeSymbol _argumentsClass;
    private readonly SourceBuilder _builder;
    private readonly ConverterGenerator _converterGenerator;
    private readonly CommandGenerator _commandGenerator;

    public ParserGenerator(SourceProductionContext context, INamedTypeSymbol argumentsClass, TypeHelper typeHelper, ConverterGenerator converterGenerator, CommandGenerator commandGenerator)
    {
        _typeHelper = typeHelper;
        _compilation = typeHelper.Compilation;
        _context = context;
        _argumentsClass = argumentsClass;
        _builder = new(argumentsClass.ContainingNamespace);
        _converterGenerator = converterGenerator;
        _commandGenerator = commandGenerator;
    }

    public static string? Generate(SourceProductionContext context, INamedTypeSymbol argumentsClass, TypeHelper typeHelper, ConverterGenerator converterGenerator, CommandGenerator commandGenerator)
    {
        var generator = new ParserGenerator(context, argumentsClass, typeHelper, converterGenerator, commandGenerator);
        return generator.Generate();
    }

    public string? Generate()
    {
        _builder.AppendLine($"partial class {_argumentsClass.Name}");
        if (_typeHelper.IParser != null)
        {
            _builder.AppendLine($"    : Ookii.CommandLine.IParser<{_argumentsClass.Name}>");
        }

        _builder.OpenBlock();
        GenerateProvider();
        _builder.AppendLine($"public static Ookii.CommandLine.CommandLineParser<{_argumentsClass.Name}> CreateParser(Ookii.CommandLine.ParseOptions? options = null) => new(new GeneratedProvider(), options);");
        _builder.AppendLine();
        var nullableType = _argumentsClass.WithNullableAnnotation(NullableAnnotation.Annotated);
        // TODO: Optionally implement these.
        // We cannot rely on default implementations, because that makes the methods uncallable
        // without a generic type argument.
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
        // Find the attributes that can apply to an arguments class.
        // This code also finds attributes that inherit from those attribute. By instantiating the
        // possibly derived attribute classes, we can support for example a class that derives from
        // DescriptionAttribute that gets the description from a resource.
        var attributes = new ArgumentsClassAttributes(_argumentsClass, _typeHelper, _context);

        // TODO: Warn if AliasAttribute without CommandAttribute.
        var isCommand = false;
        if (attributes.Command != null)
        {
            if (_argumentsClass.ImplementsInterface(_typeHelper.ICommand))
            {
                isCommand = true;
                _commandGenerator.AddGeneratedCommand(_argumentsClass, attributes);
            }
            else
            {
                // The other way around (interface without attribute) doesn't need a warning since
                // it could be a base class for a command (though it's kind of weird that the
                // GeneratedParserAttribute was used on a base class).
                _context.ReportDiagnostic(Diagnostics.CommandAttributeWithoutInterface(_argumentsClass));
            }
        }

        _builder.AppendLine("private class GeneratedProvider : Ookii.CommandLine.Support.GeneratedArgumentProvider");
        _builder.OpenBlock();
        _builder.AppendLine("public GeneratedProvider()");
        _builder.IncreaseIndent();
        _builder.AppendLine($": base(typeof({_argumentsClass.Name}),");
        _builder.AppendLine($"       {attributes.ParseOptions?.CreateInstantiation() ?? "null"},");
        if (attributes.ClassValidators == null)
        {
            _builder.AppendLine($"       null,");
        }
        else
        {
            _builder.AppendLine($"       new Ookii.CommandLine.Validation.ClassValidationAttribute[] {{ {string.Join(", ", attributes.ClassValidators.Select(v => v.CreateInstantiation()))} }},");
        }

        _builder.AppendLine($"       {attributes.ApplicationFriendlyName?.CreateInstantiation() ?? "null"},");
        _builder.AppendLine($"       {attributes.Description?.CreateInstantiation() ?? "null"})");
        _builder.DecreaseIndent();
        _builder.AppendLine("{}");
        _builder.AppendLine();
        _builder.AppendLine($"public override bool IsCommand => {isCommand.ToCSharpString()};");
        _builder.AppendLine();
        _builder.AppendLine("public override System.Collections.Generic.IEnumerable<Ookii.CommandLine.CommandLineArgument> GetArguments(Ookii.CommandLine.CommandLineParser parser)");
        _builder.OpenBlock();

        var current = _argumentsClass;
        List<(string, string, string)>? requiredProperties = null;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                GenerateArgument(member, ref requiredProperties);
            }

            current = current.BaseType;
        }

        // Makes sure the function compiles if there are no arguments.
        _builder.AppendLine("yield break;");
        _builder.CloseBlock(); // GetArguments()
        _builder.AppendLine();
        _builder.AppendLine("public override object CreateInstance(Ookii.CommandLine.CommandLineParser parser, object?[]? requiredPropertyValues)");
        _builder.OpenBlock();
        if (_argumentsClass.FindConstructor(_typeHelper.CommandLineParser) != null)
        {
            _builder.Append($"return new {_argumentsClass.Name}(parser)");
        }
        else
        {
            _builder.Append($"return new {_argumentsClass.Name}()");
        }

        if (requiredProperties == null)
        {
            _builder.AppendLine(";");
        }
        else
        {
            _builder.AppendLine();
            _builder.OpenBlock();
            for (int i = 0; i < requiredProperties.Count; ++i)
            {
                var property = requiredProperties[i];
                _builder.Append($"{property.Item1} = ({property.Item2})requiredPropertyValues![{i}]{property.Item3}");
                if (i < requiredProperties.Count - 1)
                {
                    _builder.Append(",");
                }

                _builder.AppendLine();
            }

            _builder.DecreaseIndent();
            _builder.AppendLine("};");
        }

        _builder.CloseBlock(); // CreateInstance()
        _builder.CloseBlock(); // GeneratedProvider class
    }

    private void GenerateArgument(ISymbol member, ref List<(string, string, string)>? requiredProperties)
    {
        // This shouldn't happen because of attribute targets, but check anyway.
        if (member.Kind is not (SymbolKind.Method or SymbolKind.Property))
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
            if (attribute.CheckType(_typeHelper.CommandLineArgumentAttribute, ref commandLineArgumentAttribute) ||
                attribute.CheckType(_typeHelper.MultiValueSeparatorAttribute, ref multiValueSeparator) ||
                attribute.CheckType(_typeHelper.DescriptionAttribute, ref description) ||
                attribute.CheckType(_typeHelper.AllowDuplicateDictionaryKeysAttribute, ref allowDuplicateDictionaryKeys) ||
                attribute.CheckType(_typeHelper.KeyValueSeparatorAttribute, ref keyValueSeparator) ||
                attribute.CheckType(_typeHelper.ArgumentConverterAttribute, ref converterAttribute) ||
                attribute.CheckType(_typeHelper.KeyConverterAttribute, ref keyConverterAttribute) ||
                attribute.CheckType(_typeHelper.ValueConverterAttribute, ref valueConverterAttribute) ||
                attribute.CheckType(_typeHelper.AliasAttribute, ref aliases) ||
                attribute.CheckType(_typeHelper.ShortAliasAttribute, ref shortAliases) ||
                attribute.CheckType(_typeHelper.ArgumentValidationAttribute, ref validators))
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

        ITypeSymbol originalArgumentType;
        MethodArgumentInfo? methodInfo = null;
        var property = member as IPropertySymbol;
        if (property != null)
        {
            if (property.DeclaredAccessibility != Accessibility.Public || property.IsStatic)
            {
                _context.ReportDiagnostic(Diagnostics.NonPublicInstanceProperty(property));
                return;
            }

            originalArgumentType = property.Type;
        }
        else if (member is IMethodSymbol method)
        {
            if (method.DeclaredAccessibility != Accessibility.Public || !method.IsStatic)
            {
                _context.ReportDiagnostic(Diagnostics.NonPublicStaticMethod(method));
                return;
            }

            methodInfo = DetermineMethodArgumentInfo(method);
            if (methodInfo is not MethodArgumentInfo methodInfoValue)
            {
                _context.ReportDiagnostic(Diagnostics.InvalidMethodSignature(method));
                return;
            }

            originalArgumentType = methodInfoValue.ArgumentType;
        }
        else
        {
            // How did we get here? Already checked above.
            return;
        }

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
                allowsNull = multiValueElementType!.AllowsNull();
                elementTypeWithNullable = multiValueElementType!.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                namedElementTypeWithNullable = elementTypeWithNullable as INamedTypeSymbol;
            }

            if (property.SetMethod != null && property.SetMethod.IsInitOnly && !property.IsRequired)
            {
                _context.ReportDiagnostic(Diagnostics.NonRequiredInitOnlyProperty(property));
                return;
            }

            if (property.IsRequired)
            {
                requiredProperties ??= new();
                requiredProperties.Add((member.Name, property.Type.ToDisplayString(), notNullAnnotation));
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

        if (property != null)
        {
            if (property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public && !property.SetMethod.IsInitOnly)
            {
                _builder.AppendLine($", setProperty: (target, value) => (({_argumentsClass.ToDisplayString()})target).{member.Name} = ({originalArgumentType.ToDisplayString()})value{notNullAnnotation}");
            }

            _builder.AppendLine($", getProperty: (target) => (({_argumentsClass.ToDisplayString()})target).{member.Name}");
            _builder.AppendLine($", requiredProperty: {property.IsRequired.ToCSharpString()}");
        }

        if (methodInfo is MethodArgumentInfo info)
        {
            string arguments = string.Empty;
            if (info.HasValueParameter)
            {
                if (info.HasParserParameter)
                {
                    arguments = $"({originalArgumentType.ToDisplayString()})value{notNullAnnotation}, parser";
                }
                else
                {
                    arguments = $"({originalArgumentType.ToDisplayString()})value{notNullAnnotation}";
                }    
            }
            else if (info.HasParserParameter)
            {
                arguments = "parser";
            }

            if (info.HasBooleanReturn)
            {
                _builder.AppendLine($", callMethod: (value, parser) => {_argumentsClass.ToDisplayString()}.{member.Name}({arguments})");
            }
            else
            {
                _builder.AppendLine($", callMethod: (value, parser) => {{ {_argumentsClass.ToDisplayString()}.{member.Name}({arguments}); return true; }}");
            }
        }

        _builder.DecreaseIndent();
        _builder.AppendLine(");");
    }

    private (ITypeSymbol?, INamedTypeSymbol?, ITypeSymbol?)? DetermineMultiValueType(IPropertySymbol property, ITypeSymbol argumentType)
    {
        if (argumentType is INamedTypeSymbol namedType)
        {
            // If the type is Dictionary<TKey, TValue> it doesn't matter if the property is
            // read-only or not.
            if (namedType.IsGenericType && namedType.ConstructedFrom.SymbolEquals(_typeHelper.Dictionary))
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

        var dictionaryType = argumentType.FindGenericInterface(_typeHelper.IDictionary);
        if (dictionaryType != null)
        {
            var keyValuePair = _compilation.GetTypeByMetadataName(typeof(KeyValuePair<,>).FullName)!;
            var elementType = keyValuePair.Construct(dictionaryType.TypeArguments, dictionaryType.TypeArgumentNullableAnnotations);
            return (null, dictionaryType, elementType);
        }

        var collectionType = argumentType.FindGenericInterface(_typeHelper.ICollection);
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

        if (elementType.SpecialType == SpecialType.System_String)
        {
            return "Ookii.CommandLine.Conversion.StringConverter.Instance";
        }
        else if (elementType.SpecialType == SpecialType.System_Boolean)
        {
            return "Ookii.CommandLine.Conversion.BooleanConverter.Instance";
        }

        if (elementType.TypeKind == TypeKind.Enum)
        {
            return $"new Ookii.CommandLine.Conversion.EnumConverter(typeof({elementType.ToDisplayString()}))";
        }

        if (elementType.ImplementsInterface(_typeHelper.ISpanParsable?.Construct(elementType)))
        {
            return $"new Ookii.CommandLine.Conversion.SpanParsableConverter<{elementType.ToDisplayString()}>()";
        }

        if (elementType.ImplementsInterface(_typeHelper.IParsable?.Construct(elementType)))
        {
            return $"new Ookii.CommandLine.Conversion.ParsableConverter<{elementType.ToDisplayString()}>()";
        }

        return _converterGenerator.GetConverter(elementType);
    }

    private MethodArgumentInfo? DetermineMethodArgumentInfo(IMethodSymbol method)
    {
        var parameters = method.Parameters;
        if (!method.IsStatic || parameters.Length > 2)
        {
            return null;
        }

        var info = new MethodArgumentInfo();
        if (method.ReturnType.SpecialType == SpecialType.System_Boolean)
        {
            info.HasBooleanReturn = true;
        }
        else if (method.ReturnType.SpecialType != SpecialType.System_Void)
        {
            return null;
        }

        if (parameters.Length == 2)
        {
            info.ArgumentType = parameters[0].Type;
            if (!parameters[1].Type.SymbolEquals(_typeHelper.CommandLineParser))
            {
                return null;
            }

            info.HasValueParameter = true;
            info.HasParserParameter = true;
        }
        else if (parameters.Length == 1)
        {
            if (parameters[0].Type.SymbolEquals(_typeHelper.CommandLineParser))
            {
                info.ArgumentType = _typeHelper.Boolean!;
                info.HasParserParameter = true;
            }
            else
            {
                info.ArgumentType = parameters[0].Type;
                info.HasValueParameter = true;
            }
        }
        else
        {
            info.ArgumentType = _typeHelper.Boolean!;
        }

        return info;
    }
}
