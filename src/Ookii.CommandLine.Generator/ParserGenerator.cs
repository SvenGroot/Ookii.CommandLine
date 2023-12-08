using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Ookii.CommandLine.Generator;

internal class ParserGenerator
{
    private enum ReturnType
    {
        Void,
        Boolean,
        CancelMode
    }

    private struct MethodArgumentInfo
    {
        public ITypeSymbol ArgumentType { get; set; }
        public bool HasValueParameter { get; set; }
        public bool HasParserParameter { get; set; }
        public ReturnType ReturnType { get; set; }
    }

    private struct PositionalArgumentInfo
    {
        public int Position { get; set; }
        public ISymbol Member { get; set; }
        public bool IsRequired { get; set; }
        public bool IsMultiValue { get; set; }
    }

    private readonly TypeHelper _typeHelper;
    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    private readonly INamedTypeSymbol _argumentsClass;
    private readonly SourceBuilder _builder;
    private readonly ConverterGenerator _converterGenerator;
    private readonly CommandGenerator _commandGenerator;
    private readonly LanguageVersion _languageVersion;
    private bool _hasImplicitPositions;
    private int _nextImplicitPosition;
    private Dictionary<int, string>? _positions;
    private List<PositionalArgumentInfo>? _positionalArguments;

    public ParserGenerator(SourceProductionContext context, INamedTypeSymbol argumentsClass, TypeHelper typeHelper,
        ConverterGenerator converterGenerator, CommandGenerator commandGenerator, LanguageVersion languageVersion)
    {
        _typeHelper = typeHelper;
        _compilation = typeHelper.Compilation;
        _context = context;
        _argumentsClass = argumentsClass;
        _builder = new(argumentsClass.ContainingNamespace);
        _converterGenerator = converterGenerator;
        _commandGenerator = commandGenerator;
        _languageVersion = languageVersion;
    }

    public static string? Generate(SourceProductionContext context, INamedTypeSymbol argumentsClass, TypeHelper typeHelper,
        ConverterGenerator converterGenerator, CommandGenerator commandGenerator, LanguageVersion languageVersion)
    {
        var generator = new ParserGenerator(context, argumentsClass, typeHelper, converterGenerator, commandGenerator,
            languageVersion);

        return generator.Generate();
    }

    public string? Generate()
    {
        // Find the attributes that can apply to an arguments class.
        // This code also finds attributes that inherit from those attribute. By instantiating the
        // possibly derived attribute classes, we can support for example a class that derives from
        // DescriptionAttribute that gets the description from a resource.
        var attributes = new ArgumentsClassAttributes(_argumentsClass, _typeHelper);

        var isCommand = false;
        if (attributes.Command != null)
        {
            if (_argumentsClass.ImplementsInterface(_typeHelper.ICommandWithCustomParsing))
            {
                _context.ReportDiagnostic(Diagnostics.GeneratedCustomParsingCommand(_argumentsClass));
                return null;
            }
            else if (_argumentsClass.ImplementsInterface(_typeHelper.ICommand))
            {
                isCommand = true;
                _commandGenerator.AddGeneratedCommand(_argumentsClass, attributes);
            }
            else
            {
                _context.ReportDiagnostic(Diagnostics.CommandAttributeWithoutInterface(_argumentsClass));
            }
        }
        else if (_argumentsClass.ImplementsInterface(_typeHelper.ICommand))
        {
            // Although this is a common pattern for base classes, it makes no sense to apply the
            // GeneratedParserAttribute to a base class.
            _context.ReportDiagnostic(Diagnostics.CommandInterfaceWithoutAttribute(_argumentsClass));
        }

        // Don't generate the parse methods for commands unless explicitly asked for.
        var generateParseMethods = !isCommand;
        foreach (var arg in attributes.GeneratedParser!.NamedArguments)
        {
            if (arg.Key == "GenerateParseMethods")
            {
                generateParseMethods = (bool)arg.Value.Value!;
                break;
            }
        }

        _builder.AppendLine($"partial class {_argumentsClass.Name}");
        // Static interface methods require not just .Net 7 but also C# 11.
        // There is no defined constant for C# 11 because the generator is built for .Net 6.0.
        if (_typeHelper.IParser != null && _languageVersion >= (LanguageVersion)1100)
        {
            if (generateParseMethods)
            {
                _builder.AppendLine($"    : Ookii.CommandLine.IParser<{_argumentsClass.Name}>");
            }
            else
            {
                _builder.AppendLine($"    : Ookii.CommandLine.IParserProvider<{_argumentsClass.Name}>");
            }
        }

        _builder.OpenBlock();
        if (!GenerateProvider(attributes, isCommand))
        {
            return null;
        }

        if (isCommand)
        {
            if (attributes.Description == null)
            {
                var commandInfo = new CommandAttributeInfo(attributes.Command!);
                if (!commandInfo.IsHidden)
                {
                    _context.ReportDiagnostic(Diagnostics.CommandWithoutDescription(_argumentsClass));
                }
            }

            if (attributes.ApplicationFriendlyName != null)
            {
                _context.ReportDiagnostic(Diagnostics.IgnoredFriendlyNameAttribute(_argumentsClass, attributes.ApplicationFriendlyName));
            }
        }
        else
        {
            if (attributes.ParentCommand != null)
            {
                _context.ReportDiagnostic(Diagnostics.IgnoredAttributeForNonCommand(_argumentsClass, attributes.ParentCommand));
            }
        }

        _builder.AppendLine();
        _builder.AppendLine("/// <summary>");
        _builder.AppendLine("/// Creates a <see cref=\"Ookii.CommandLine.CommandLineParser{T}\"/> instance using the specified options.");
        _builder.AppendLine("/// </summary>");
        _builder.AppendLine("/// <param name=\"options\">");
        _builder.AppendLine("/// The options that control parsing behavior, or <see langword=\"null\"/> to use the");
        _builder.AppendLine("/// default options.");
        _builder.AppendLine("/// </param>");
        _builder.AppendLine("/// <returns>");
        _builder.AppendLine($"/// An instance of the <see cref=\"Ookii.CommandLine.CommandLineParser{{T}}\"/> class for the <see cref=\"{_argumentsClass.ToQualifiedName()}\"/> class.");
        _builder.AppendLine("/// </returns>");
        _builder.AppendGeneratedCodeAttribute();
        _builder.AppendLine($"public static Ookii.CommandLine.CommandLineParser<{_argumentsClass.ToQualifiedName()}> CreateParser(Ookii.CommandLine.ParseOptions? options = null) => new Ookii.CommandLine.CommandLineParser<{_argumentsClass.ToQualifiedName()}>(new OokiiCommandLineArgumentProvider(), options);");
        _builder.AppendLine();
        var nullableType = _argumentsClass.WithNullableAnnotation(NullableAnnotation.Annotated);

        if (generateParseMethods)
        {
            // We cannot rely on default interface implementations, because that makes the methods
            // uncallable without a generic type argument.
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine("/// Parses the arguments returned by the <see cref=\"System.Environment.GetCommandLineArgs\" qualifyHint=\"true\"/>");
            _builder.AppendLine("/// method, handling errors and showing usage help as required.");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine("/// <param name=\"options\">");
            _builder.AppendLine("///   The options that control parsing behavior and usage help formatting. If");
            _builder.AppendLine("///   <see langword=\"null\" />, the default options are used.");
            _builder.AppendLine("/// </param>");
            _builder.AppendLine("/// <returns>");
            _builder.AppendLine($"///   An instance of the <see cref=\"{_argumentsClass.ToQualifiedName()}\"/> class, or <see langword=\"null\"/> if an");
            _builder.AppendLine("///   error occurred or argument parsing was canceled.");
            _builder.AppendLine("/// </returns>");
            _builder.AppendGeneratedCodeAttribute();
            _builder.AppendLine($"public static {nullableType.ToQualifiedName()} Parse(Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling();");
            _builder.AppendLine();
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine("/// Parses the specified command line arguments, handling errors and showing usage help as required.");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine("/// <param name=\"args\">The command line arguments.</param>");
            _builder.AppendLine("/// <param name=\"options\">");
            _builder.AppendLine("///   The options that control parsing behavior and usage help formatting. If");
            _builder.AppendLine("///   <see langword=\"null\" />, the default options are used.");
            _builder.AppendLine("/// </param>");
            _builder.AppendLine("/// <returns>");
            _builder.AppendLine($"///   An instance of the <see cref=\"{_argumentsClass.ToQualifiedName()}\"/> class, or <see langword=\"null\"/> if an");
            _builder.AppendLine("///   error occurred or argument parsing was canceled.");
            _builder.AppendLine("/// </returns>");
            _builder.AppendGeneratedCodeAttribute();
            _builder.AppendLine($"public static {nullableType.ToQualifiedName()} Parse(string[] args, Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling(args);");
            _builder.AppendLine();
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine("/// Parses the specified command line arguments, handling errors and showing usage help as required.");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine("/// <param name=\"args\">The command line arguments.</param>");
            _builder.AppendLine("/// <param name=\"options\">");
            _builder.AppendLine("///   The options that control parsing behavior and usage help formatting. If");
            _builder.AppendLine("///   <see langword=\"null\" />, the default options are used.");
            _builder.AppendLine("/// </param>");
            _builder.AppendLine("/// <returns>");
            _builder.AppendLine($"///   An instance of the <see cref=\"{_argumentsClass.ToQualifiedName()}\"/> class, or <see langword=\"null\"/> if an");
            _builder.AppendLine("///   error occurred or argument parsing was canceled.");
            _builder.AppendLine("/// </returns>");
            _builder.AppendGeneratedCodeAttribute();
            _builder.AppendLine($"public static {nullableType.ToQualifiedName()} Parse(System.ReadOnlyMemory<string> args, Ookii.CommandLine.ParseOptions? options = null) => CreateParser(options).ParseWithErrorHandling(args);");
            _builder.CloseBlock(); // class
        }

        return _builder.GetSource();
    }

    private bool GenerateProvider(ArgumentsClassAttributes attributes, bool isCommand)
    {
        _builder.AppendGeneratedCodeAttribute();
        _builder.AppendLine("private class OokiiCommandLineArgumentProvider : Ookii.CommandLine.Support.GeneratedArgumentProvider");
        _builder.OpenBlock();
        _builder.AppendLine("public OokiiCommandLineArgumentProvider()");
        _builder.IncreaseIndent();
        _builder.AppendLine(": base(");
        _builder.IncreaseIndent();
        _builder.AppendArgument($"typeof({_argumentsClass.Name})");
        AppendOptionalAttribute(attributes.ParseOptions, "options");
        AppendOptionalAttribute(attributes.ClassValidators, "validators", "Ookii.CommandLine.Validation.ClassValidationAttribute");
        AppendOptionalAttribute(attributes.ApplicationFriendlyName, "friendlyName");
        AppendOptionalAttribute(attributes.Description, "description");
        _builder.CloseArgumentList(false);
        _builder.DecreaseIndent();
        _builder.AppendLine("{}");
        _builder.AppendLine();
        _builder.AppendLine($"public override bool IsCommand => {isCommand.ToCSharpString()};");
        _builder.AppendLine();
        _builder.AppendLine("public override System.Collections.Generic.IEnumerable<Ookii.CommandLine.CommandLineArgument> GetArguments(Ookii.CommandLine.CommandLineParser parser)");
        _builder.OpenBlock();

        List<(string, string, string)>? requiredProperties = null;
        var hasError = false;

        // Build a stack with the base types because we have to consider them first to get the
        // correct order for auto positional arguments.
        var argumentTypes = new Stack<INamedTypeSymbol>();
        for (var current = _argumentsClass;
             current != null && current.SpecialType == SpecialType.None;
             current = current.BaseType)
        {
            argumentTypes.Push(current);
        }

        foreach (var type in argumentTypes)
        {
            foreach (var member in type.GetMembers())
            {
                if (!GenerateArgument(member, ref requiredProperties))
                {
                    hasError = true;
                }
            }
        }

        if (!VerifyPositionalArgumentRules())
        {
            return false;
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
        _builder.CloseBlock(); // OokiiCommandLineArgumentProvider class
        return !hasError;
    }

    private bool GenerateArgument(ISymbol member, ref List<(string, string, string)>? requiredProperties)
    {
        // This shouldn't happen because of attribute targets, but check anyway.
        if (member.Kind is not (SymbolKind.Method or SymbolKind.Property))
        {
            return true;
        }

        var attributes = new ArgumentAttributes(member, _typeHelper, _context);

        // Check if it is an argument.
        if (attributes.CommandLineArgument == null)
        {
            return true;
        }

        var argumentInfo = new CommandLineArgumentAttributeInfo(attributes.CommandLineArgument);
        if (!argumentInfo.IsLong && !argumentInfo.IsShort)
        {
            _context.ReportDiagnostic(Diagnostics.NoLongOrShortName(member, attributes.CommandLineArgument));
            return false;
        }

        ITypeSymbol originalArgumentType;
        MethodArgumentInfo? methodInfo = null;
        var property = member as IPropertySymbol;
        if (property != null)
        {
            if (property.DeclaredAccessibility != Accessibility.Public || property.IsStatic)
            {
                _context.ReportDiagnostic(Diagnostics.NonPublicInstanceProperty(property));
                return true;
            }

            originalArgumentType = property.Type;
        }
        else if (member is IMethodSymbol method)
        {
            if (method.DeclaredAccessibility != Accessibility.Public || !method.IsStatic)
            {
                _context.ReportDiagnostic(Diagnostics.NonPublicStaticMethod(method));
                return true;
            }

            methodInfo = DetermineMethodArgumentInfo(method);
            if (methodInfo is not MethodArgumentInfo methodInfoValue)
            {
                _context.ReportDiagnostic(Diagnostics.InvalidMethodSignature(method));
                return false;
            }

            originalArgumentType = methodInfoValue.ArgumentType;
        }
        else
        {
            // How did we get here? Already checked above.
            return true;
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
        if (attributes.KeyValueSeparator != null)
        {
            _builder.AppendLine($"var keyValueSeparatorAttribute{member.Name} = {attributes.KeyValueSeparator.CreateInstantiation()};");
        }

        var isMultiValue = false;
        var isDictionary = false;
        var isRequired = argumentInfo.IsRequired;
        var kind = "Ookii.CommandLine.ArgumentKind.SingleValue";
        string? converter = null;
        if (property != null)
        {
            var multiValueType = DetermineMultiValueType(property, argumentType);
            if (multiValueType is not var (collectionType, dictionaryType, multiValueElementType))
            {
                return false;
            }

            if (dictionaryType != null)
            {
                Debug.Assert(multiValueElementType != null);
                kind = "Ookii.CommandLine.ArgumentKind.Dictionary";
                isMultiValue = true;
                isDictionary = true;
                elementTypeWithNullable = multiValueElementType!;
                // KeyValuePair is guaranteed a named type.
                namedElementTypeWithNullable = (INamedTypeSymbol)elementTypeWithNullable;
                keyType = namedElementTypeWithNullable.TypeArguments[0].WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                var rawValueType = namedElementTypeWithNullable.TypeArguments[1];
                allowsNull = rawValueType.AllowsNull();
                valueType = rawValueType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                if (attributes.Converter == null)
                {
                    var keyConverter = DetermineConverter(member, keyType.GetUnderlyingType(), attributes.KeyConverter, keyType.IsNullableValueType());
                    if (keyConverter == null)
                    {
                        _context.ReportDiagnostic(Diagnostics.NoConverter(member, keyType.GetUnderlyingType()));
                        return false;
                    }

                    var valueConverter = DetermineConverter(member, valueType.GetUnderlyingType(), attributes.ValueConverter, valueType.IsNullableValueType());
                    if (valueConverter == null)
                    {
                        _context.ReportDiagnostic(Diagnostics.NoConverter(member, keyType.GetUnderlyingType()));
                        return false;
                    }

                    var separator = attributes.KeyValueSeparator == null
                        ? "null"
                        : $"keyValueSeparatorAttribute{member.Name}.Separator";

                    converter = $"new Ookii.CommandLine.Conversion.KeyValuePairConverter<{keyType.ToQualifiedName()}, {rawValueType.ToQualifiedName()}>({keyConverter}, {valueConverter}, {separator}, {allowsNull.ToCSharpString()})";
                }
            }
            else if (collectionType != null)
            {
                Debug.Assert(multiValueElementType != null);
                kind = "Ookii.CommandLine.ArgumentKind.MultiValue";
                isMultiValue = true;
                allowsNull = multiValueElementType!.AllowsNull();
                elementTypeWithNullable = multiValueElementType!.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                namedElementTypeWithNullable = elementTypeWithNullable as INamedTypeSymbol;
            }

            if (property.SetMethod != null && property.SetMethod.IsInitOnly && !property.IsRequired)
            {
                _context.ReportDiagnostic(Diagnostics.NonRequiredInitOnlyProperty(property));
                return false;
            }

            if (property.IsRequired)
            {
                isRequired = true;
                requiredProperties ??= new();
                requiredProperties.Add((member.Name, property.Type.ToQualifiedName(), notNullAnnotation));
            }
        }
        else
        {
            kind = "Ookii.CommandLine.ArgumentKind.Method";
        }

        var elementType = namedElementTypeWithNullable?.GetUnderlyingType() ?? elementTypeWithNullable;
        converter ??= DetermineConverter(member, elementType, attributes.Converter, elementTypeWithNullable.IsNullableValueType());
        if (converter == null)
        {
            _context.ReportDiagnostic(Diagnostics.NoConverter(member, elementType));
            return false;
        }

        // The leading commas are not a formatting I like but it does make things easier here.
        _builder.AppendLine($"yield return Ookii.CommandLine.Support.GeneratedArgument.Create(");
        _builder.IncreaseIndent();
        _builder.AppendArgument("parser");
        _builder.AppendArgument($"argumentType: typeof({argumentType.ToQualifiedName()})");
        _builder.AppendArgument($"elementTypeWithNullable: typeof({elementTypeWithNullable.ToQualifiedName()})");
        _builder.AppendArgument($"elementType: typeof({elementType.ToQualifiedName()})");
        _builder.AppendArgument($"memberName: \"{member.Name}\"");
        _builder.AppendArgument($"kind: {kind}");
        _builder.AppendArgument($"attribute: {attributes.CommandLineArgument.CreateInstantiation()}");
        _builder.AppendArgument($"converter: {converter}");
        _builder.AppendArgument($"allowsNull: {(allowsNull.ToCSharpString())}");
        var valueDescriptionFormat = new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters);
        if (keyType != null)
        {
            _builder.AppendArgument($"keyType: typeof({keyType.ToQualifiedName()})");
            _builder.AppendArgument($"defaultKeyDescription: \"{keyType.ToDisplayString(valueDescriptionFormat)}\"");
        }

        if (valueType != null)
        {
            _builder.AppendArgument($"valueType: typeof({valueType.ToQualifiedName()})");
            _builder.AppendArgument($"defaultValueDescription: \"{valueType.ToDisplayString(valueDescriptionFormat)}\"");
        }
        else
        {
            _builder.AppendArgument($"defaultValueDescription: \"{elementType.ToDisplayString(valueDescriptionFormat)}\"");
        }

        AppendOptionalAttribute(attributes.MultiValueSeparator, "multiValueSeparatorAttribute");
        AppendOptionalAttribute(attributes.Description, "descriptionAttribute");
        AppendOptionalAttribute(attributes.ValueDescription, "valueDescriptionAttribute");
        if (attributes.AllowDuplicateDictionaryKeys != null)
        {
            _builder.AppendArgument("allowDuplicateDictionaryKeys: true");
        }

        if (attributes.KeyValueSeparator != null)
        {
            _builder.AppendArgument($"keyValueSeparatorAttribute: keyValueSeparatorAttribute{member.Name}");
        }

        AppendOptionalAttribute(attributes.Aliases, "aliasAttributes", "Ookii.CommandLine.AliasAttribute");
        AppendOptionalAttribute(attributes.ShortAliases, "shortAliasAttributes", "Ookii.CommandLine.ShortAliasAttribute");
        AppendOptionalAttribute(attributes.Validators, "validationAttributes", "Ookii.CommandLine.Validation.ArgumentValidationAttribute");
        if (property != null)
        {
            if (property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public && !property.SetMethod.IsInitOnly)
            {
                _builder.AppendArgument($"setProperty: (target, value) => (({_argumentsClass.ToQualifiedName()})target).{member.Name} = ({originalArgumentType.ToQualifiedName()})value{notNullAnnotation}");
            }

            _builder.AppendArgument($"getProperty: (target) => (({_argumentsClass.ToQualifiedName()})target).{member.Name}");
            _builder.AppendArgument($"requiredProperty: {property.IsRequired.ToCSharpString()}");
            if (argumentInfo.DefaultValue != null)
            {
                if (isMultiValue)
                {
                    _context.ReportDiagnostic(Diagnostics.DefaultValueWithMultiValue(member));
                }
                else if (property.IsRequired || argumentInfo.IsRequired)
                {
                    _context.ReportDiagnostic(Diagnostics.DefaultValueWithRequired(member));
                }
            }

            if (argumentInfo.HasIsRequired && property.IsRequired)
            {
                _context.ReportDiagnostic(Diagnostics.IsRequiredWithRequiredProperty(member));
            }

            // Check if we should use the initializer for a default value.
            if (!isMultiValue && !property.IsRequired && !argumentInfo.IsRequired && argumentInfo.DefaultValue == null && argumentInfo.IncludeDefaultInUsageHelp)
            {
                var alternateDefaultValue = GetInitializerValue(property);
                if (alternateDefaultValue != null)
                {
                    _builder.AppendArgument($"alternateDefaultValue: {alternateDefaultValue}");
                }
            }
        }

        if (methodInfo is MethodArgumentInfo info)
        {
            string arguments = string.Empty;
            if (info.HasValueParameter)
            {
                if (info.HasParserParameter)
                {
                    arguments = $"({originalArgumentType.ToQualifiedName()})value{notNullAnnotation}, parser";
                }
                else
                {
                    arguments = $"({originalArgumentType.ToQualifiedName()})value{notNullAnnotation}";
                }
            }
            else if (info.HasParserParameter)
            {
                arguments = "parser";
            }

            var methodCall = info.ReturnType switch
            {
                ReturnType.CancelMode => $"callMethod: (value, parser) => {_argumentsClass.ToQualifiedName()}.{member.Name}({arguments})",
                ReturnType.Boolean => $"callMethod: (value, parser) => {_argumentsClass.ToQualifiedName()}.{member.Name}({arguments}) ? Ookii.CommandLine.CancelMode.None : Ookii.CommandLine.CancelMode.Abort",
                _ => $"callMethod: (value, parser) => {{ {_argumentsClass.ToQualifiedName()}.{member.Name}({arguments}); return Ookii.CommandLine.CancelMode.None; }}"
            };

            _builder.AppendArgument(methodCall);
            if (argumentInfo.DefaultValue != null)
            {
                _context.ReportDiagnostic(Diagnostics.DefaultValueWithMethod(member));
            }
        }

        if (argumentInfo.Position is int position)
        {
            if (_hasImplicitPositions)
            {
                _context.ReportDiagnostic(Diagnostics.MixedImplicitExplicitPositions(_argumentsClass));
                return false;
            }

            _positions ??= new();
            if (_positions.TryGetValue(position, out string name))
            {
                _context.ReportDiagnostic(Diagnostics.DuplicatePosition(member, name));
            }
            else
            {
                _positions.Add(position, member.Name);
            }

            _positionalArguments ??= new();
            _positionalArguments.Add(new PositionalArgumentInfo()
            {
                Member = member,
                Position = position,
                IsRequired = isRequired,
                IsMultiValue = isMultiValue
            });
        }
        else if (argumentInfo.IsPositional)
        {
            if (_positions != null)
            {
                _context.ReportDiagnostic(Diagnostics.MixedImplicitExplicitPositions(_argumentsClass));
                return false;
            }

            _hasImplicitPositions = true;
            _builder.AppendArgument($"position: {_nextImplicitPosition}");
            ++_nextImplicitPosition;
        }

        _builder.CloseArgumentList();
        _builder.AppendLine();

        // Can't check if long/short name is actually used, or whether the '-' prefix is used for
        // either style, since ParseOptions might change that. So, just warn either way.
        if (!string.IsNullOrEmpty(argumentInfo.ArgumentName) && char.IsDigit(argumentInfo.ArgumentName![0]))
        {
            _context.ReportDiagnostic(Diagnostics.ArgumentStartsWithNumber(member, argumentInfo.ArgumentName));
        }
        else if (char.IsDigit(argumentInfo.ShortName))
        {
            _context.ReportDiagnostic(Diagnostics.ArgumentStartsWithNumber(member, argumentInfo.ShortName.ToString()));
        }


        if (!argumentInfo.IsShort && attributes.ShortAliases != null)
        {
            _context.ReportDiagnostic(Diagnostics.ShortAliasWithoutShortName(attributes.ShortAliases.First(), member));
        }

        if (!argumentInfo.IsLong && attributes.Aliases != null)
        {
            _context.ReportDiagnostic(Diagnostics.AliasWithoutLongName(attributes.Aliases.First(), member));
        }

        bool isHidden = false;
        if (argumentInfo.IsHidden)
        {
            if (argumentInfo.IsPositional || argumentInfo.IsRequired || (property?.IsRequired ?? false))
            {
                _context.ReportDiagnostic(Diagnostics.IsHiddenWithPositionalOrRequired(member));
            }
            else
            {
                isHidden = true;
            }
        }

        if (!isHidden && attributes.Description == null)
        {
            _context.ReportDiagnostic(Diagnostics.ArgumentWithoutDescription(member));
        }

        CheckIgnoredDictionaryAttribute(member, isDictionary, attributes.Converter, attributes.KeyConverter);
        CheckIgnoredDictionaryAttribute(member, isDictionary, attributes.Converter, attributes.ValueConverter);
        CheckIgnoredDictionaryAttribute(member, isDictionary, attributes.Converter, attributes.KeyValueSeparator);
        if (!isMultiValue && attributes.MultiValueSeparator != null)
        {
            _context.ReportDiagnostic(Diagnostics.IgnoredAttributeForNonMultiValue(member, attributes.MultiValueSeparator));
        }

        if (!isDictionary && attributes.AllowDuplicateDictionaryKeys != null)
        {
            _context.ReportDiagnostic(Diagnostics.IgnoredAttributeForNonDictionary(member, attributes.AllowDuplicateDictionaryKeys));
        }

        if (argumentInfo.ShortName != '\0' && argumentInfo.ExplicitIsShort == false)
        {
            _context.ReportDiagnostic(Diagnostics.IsShortIgnored(member, attributes.CommandLineArgument));
        }

        if (attributes.ValidateEnumValue != null)
        { 
            if (elementType.TypeKind != TypeKind.Enum)
            {
                _context.ReportDiagnostic(Diagnostics.ValidateEnumInvalidType(member, elementType));
            }
            else if (attributes.Converter != null &&
                (attributes.ValidateEnumValue.GetNamedArgument("CaseSensitive") != null ||
                 attributes.ValidateEnumValue.GetNamedArgument("AllowCommaSeparatedValues") != null ||
                 attributes.ValidateEnumValue.GetNamedArgument("AllowNumericValues") != null))
            {
                _context.ReportDiagnostic(Diagnostics.ValidateEnumWithCustomConverter(member));
            }
        }

        return true;
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

    public string? DetermineConverter(ISymbol member, ITypeSymbol elementType, AttributeData? converterAttribute, bool isNullableValueType)
    {
        var converter = DetermineElementConverter(member, elementType, converterAttribute);
        if (converter != null && isNullableValueType)
        {
            converter = $"new Ookii.CommandLine.Conversion.NullableConverter({converter})";
        }

        return converter;
    }

    public string? DetermineElementConverter(ISymbol member, ITypeSymbol elementType, AttributeData? converterAttribute)
    {
        if (converterAttribute != null)
        {
            var argument = converterAttribute.ConstructorArguments[0];
            if (argument.Kind != TypedConstantKind.Type)
            {
                _context.ReportDiagnostic(Diagnostics.ArgumentConverterStringNotSupported(converterAttribute, member));
                return null;
            }

            var converterType = (INamedTypeSymbol)argument.Value!;
            return $"new {converterType.ToQualifiedName()}()";
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
            return $"new Ookii.CommandLine.Conversion.EnumConverter(typeof({elementType.ToQualifiedName()}))";
        }

        if (elementType.ImplementsInterface(_typeHelper.ISpanParsable?.Construct(elementType)))
        {
            return $"new Ookii.CommandLine.Conversion.SpanParsableConverter<{elementType.ToQualifiedName()}>()";
        }

        if (elementType.ImplementsInterface(_typeHelper.IParsable?.Construct(elementType)))
        {
            return $"new Ookii.CommandLine.Conversion.ParsableConverter<{elementType.ToQualifiedName()}>()";
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
        if (method.ReturnType.SymbolEquals(_typeHelper.CancelMode))
        {
            info.ReturnType = ReturnType.CancelMode;
        }
        else if (method.ReturnType.SpecialType == SpecialType.System_Boolean)
        {
            info.ReturnType = ReturnType.Boolean;
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

    private void AppendOptionalAttribute(AttributeData? attribute, string name)
    {
        if (attribute != null)
        {
            _builder.AppendArgument($"{name}: {attribute.CreateInstantiation()}");
        }
    }

    private void AppendOptionalAttribute(List<AttributeData>? attributes, string name, string typeName)
    {
        if (attributes != null)
        {
            _builder.AppendArgument($"{name}: new {typeName}[] {{ {string.Join(", ", attributes.Select(a => a.CreateInstantiation()))} }}");
        }
    }

    private bool VerifyPositionalArgumentRules()
    {
        if (_positionalArguments == null)
        {
            return true;
        }

        // This mirrors the logic in CommandLineParser.VerifyPositionalArgumentRules.
        _positionalArguments.Sort((x, y) => x.Position.CompareTo(y.Position));
        string? multiValueArgument = null;
        string? optionalArgument = null;
        var result = true;
        foreach (var argument in _positionalArguments)
        {
            if (multiValueArgument != null)
            {
                _context.ReportDiagnostic(Diagnostics.PositionalArgumentAfterMultiValue(argument.Member, multiValueArgument));
                result = false;
            }

            if (argument.IsRequired && optionalArgument != null)
            {
                _context.ReportDiagnostic(Diagnostics.PositionalRequiredArgumentAfterOptional(argument.Member, optionalArgument));
                result = false;
            }

            if (!argument.IsRequired)
            {
                optionalArgument = argument.Member.Name;
            }

            if (argument.IsMultiValue)
            {
                multiValueArgument = argument.Member.Name;
            }
        }

        return result;
    }

    private void CheckIgnoredDictionaryAttribute(ISymbol member, bool isDictionary, AttributeData? converter, AttributeData? attribute)
    {
        if (attribute == null)
        {
            return;
        }

        if (!isDictionary)
        {
            _context.ReportDiagnostic(Diagnostics.IgnoredAttributeForNonDictionary(member, attribute));
        }
        else if (converter != null)
        {
            _context.ReportDiagnostic(Diagnostics.IgnoredAttributeForDictionaryWithConverter(member, attribute));
        }
    }

    private string? GetInitializerValue(IPropertySymbol symbol)
    {
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(_context.CancellationToken) as PropertyDeclarationSyntax;
        if (syntax?.Initializer == null)
        {
            return null;
        }

        var expression = syntax.Initializer.Value;
        if (expression is PostfixUnaryExpressionSyntax postfixUnaryExpression)
        {
            if (postfixUnaryExpression.Kind() == SyntaxKind.SuppressNullableWarningExpression)
            {
                expression = postfixUnaryExpression.Operand;
            }
        }

        var expressionString = expression switch
        {
            // We have to include the type in a default expression because it's going to be
            // assigned to an object so just "default" would always be null.
            LiteralExpressionSyntax value => value.IsKind(SyntaxKind.DefaultLiteralExpression) ? $"default({symbol.Type.ToQualifiedName()})" : value?.Token.ToFullString(),
            MemberAccessExpressionSyntax memberAccessExpression => GetSymbolExpressionString(memberAccessExpression),
            IdentifierNameSyntax identifierName => GetSymbolExpressionString(identifierName),
            _ => null,
        };

        if (expressionString == null)
        {
            _context.ReportDiagnostic(Diagnostics.UnsupportedInitializerSyntax(symbol, syntax.Initializer.GetLocation()));
        }

        return expressionString;
    }

    private string? GetSymbolExpressionString(ExpressionSyntax syntax)
    {
        var model = _compilation.GetSemanticModel(syntax.SyntaxTree);
        var symbol = model.GetSymbolInfo(syntax);
        return symbol.Symbol?.ToQualifiedName();
    }
}
