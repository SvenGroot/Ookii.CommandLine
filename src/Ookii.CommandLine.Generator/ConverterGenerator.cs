using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Ookii.CommandLine.Generator;

internal class ConverterGenerator
{
    #region Nested types

    private struct ConverterInfo
    {
        public string? Name { get; set; }
        public bool ParseMethod { get; set; }
        public bool HasCulture { get; set; }
        public bool UseSpan { get; set; }

        public bool IsBetter(ConverterInfo other)
        {
            // Prefer Parse over constructor.
            if (ParseMethod != other.ParseMethod)
            {
                return ParseMethod;
            }

            // Prefer culture over no culture.
            if (HasCulture != other.HasCulture)
            {
                return HasCulture;
            }

            // Prefer span over string.
            if (UseSpan != other.UseSpan)
            {
                return UseSpan;
            }

            return false;
        }
    }

    #endregion

    private const string DefaultGeneratedNamespace = "Ookii.CommandLine.Conversion.Generated";
    private const string ConverterSuffix = "Converter";
    private readonly INamedTypeSymbol? _readOnlySpanType;
    private readonly INamedTypeSymbol? _cultureType;
    private readonly Dictionary<ITypeSymbol, ConverterInfo> _converters = new(SymbolEqualityComparer.Default);

    public ConverterGenerator(TypeHelper typeHelper, SourceProductionContext context)
    {
        _cultureType = typeHelper.CultureInfo;
        _readOnlySpanType = typeHelper.ReadOnlySpanOfChar;
        GeneratedNamespace = GetGeneratedNamespace(typeHelper, context);
    }

    public string GeneratedNamespace { get; }

    public string? GetConverter(ITypeSymbol type)
    {
        if (!_converters.TryGetValue(type, out var converter))
        {
            var optionalInfo = FindParseMethod(type) ?? FindConstructor(type);
            if (optionalInfo is not ConverterInfo info)
            {
                return null;
            }

            info.Name = GenerateName(type);
            _converters.Add(type, info);
            converter = info;
        }

        return $"new {GeneratedNamespace}.{converter.Name}()";
    }

    public string? Generate()
    {
        if (_converters.Count == 0)
        {
            return null;
        }

        var builder = new SourceBuilder(GeneratedNamespace);
        foreach (var converter in _converters)
        {
            CreateConverter(builder, converter.Key, converter.Value);
        }

        return builder.GetSource();
    }

    private ConverterInfo? FindConstructor(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return null;
        }

        ConverterInfo? info = null;
        foreach (var ctor in namedType.Constructors)
        {
            if (ctor.IsStatic || ctor.DeclaredAccessibility != Accessibility.Public || ctor.Parameters.Length != 1)
            {
                continue;
            }

            var newInfo = new ConverterInfo();
            if (ctor.Parameters[0].Type.SymbolEquals(_readOnlySpanType))
            {
                newInfo.UseSpan = true;
                info = newInfo;
                // Won't find a better one
                break;
            }
            else if (ctor.Parameters[0].Type.SpecialType != SpecialType.System_String)
            {
                continue;
            }

            info = newInfo;
        }

        return info;
    }

    private ConverterInfo? FindParseMethod(ITypeSymbol type)
    {
        ConverterInfo? info = null;
        foreach (var member in type.GetMembers("Parse"))
        {
            if (!member.IsStatic || member.DeclaredAccessibility != Accessibility.Public || member is not IMethodSymbol method ||
                method.Parameters.Length < 1 || method.Parameters.Length > 2)
            {
                continue;
            }

            var newInfo = new ConverterInfo() { ParseMethod = true };
            if (method.Parameters[0].Type.SymbolEquals(_readOnlySpanType))
            {
                newInfo.UseSpan = true;
            }
            else if (method.Parameters[0].Type.SpecialType != SpecialType.System_String)
            {
                continue;
            }

            if (method.Parameters.Length == 2)
            {
                if (_cultureType != null && method.Parameters[1].Type.CanAssignFrom(_cultureType))
                {
                    newInfo.HasCulture = true;
                }
                else
                {
                    continue;
                }
            }

            if (info is not ConverterInfo i || newInfo.IsBetter(i))
            {
                info = newInfo;
                if (newInfo.HasCulture && newInfo.UseSpan)
                {
                    // Won't find a better one.
                    break;
                }
            }
        }

        return info;
    }

    private static string GenerateName(ITypeSymbol type)
    {
        // Use the full framework name even for types that have keywords, and don't include global
        // namespace.
        var format = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
            .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        var displayName = type.ToDisplayString(format);
        return displayName.ToIdentifier(ConverterSuffix);
    }

    private static void CreateConverter(SourceBuilder builder, ITypeSymbol type, ConverterInfo info)
    {
        builder.AppendGeneratedCodeAttribute();
        builder.AppendLine($"internal class {info.Name} : Ookii.CommandLine.Conversion.ArgumentConverter");
        builder.OpenBlock();
        string inputType = info.UseSpan ? "System.ReadOnlySpan<char>" : "string";
        string culture = info.HasCulture ? ", culture" : string.Empty;
        builder.AppendLine($"public override object? Convert({inputType} value, System.Globalization.CultureInfo culture, Ookii.CommandLine.CommandLineArgument argument)");
        if (info.ParseMethod)
        {
            builder.AppendLine($"    => {type.ToQualifiedName()}.Parse(value{culture});");
        }
        else
        {
            builder.AppendLine($"    => new {type.ToQualifiedName()}(value);");
        }

        if (info.UseSpan)
        {
            builder.AppendLine();
            builder.AppendLine("public override object? Convert(string value, System.Globalization.CultureInfo culture, Ookii.CommandLine.CommandLineArgument argument) => Convert(System.MemoryExtensions.AsSpan(value), culture, argument);");
        }

        builder.CloseBlock(); // class
        builder.AppendLine();
    }

    private static string GetGeneratedNamespace(TypeHelper typeHelper, SourceProductionContext context)
    {
        var attributeType = typeHelper.GeneratedConverterNamespaceAttribute;
        if (attributeType == null)
        {
            return DefaultGeneratedNamespace;
        }

        AttributeData? attribute = null;
        foreach (var attr in typeHelper.Compilation.Assembly.GetAttributes())
        {
            if (attributeType.SymbolEquals(attr.AttributeClass))
            {
                attribute = attr;
                break;
            }
        }

        if (attribute == null)
        {
            return DefaultGeneratedNamespace;
        }

        if (attribute.ConstructorArguments.FirstOrDefault().Value is not string ns)
        {
            return DefaultGeneratedNamespace;
        }

        var elements = ns.Split('.');
        foreach (var element in elements)
        {
            if (!SyntaxFacts.IsValidIdentifier(element))
            {
                context.ReportDiagnostic(Diagnostics.InvalidGeneratedConverterNamespace(ns, attribute));
                return DefaultGeneratedNamespace;
            }
        }

        return ns;
    }
}
