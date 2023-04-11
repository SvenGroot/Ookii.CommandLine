using Microsoft.CodeAnalysis;
using System.Text;

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

        public string ConstructorCall => $"new {GeneratedNamespace}.{Name}()";

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

    // TODO: Customizable or random namespace?
    private const string GeneratedNamespace = "Ookii.CommandLine.Conversion.Generated";
    private const string ConverterSuffix = "Converter";
    private readonly INamedTypeSymbol? _readOnlySpanType;
    private readonly INamedTypeSymbol? _stringType;
    private readonly INamedTypeSymbol? _cultureType;
    private readonly Dictionary<ITypeSymbol, ConverterInfo> _converters = new(SymbolEqualityComparer.Default);

    public ConverterGenerator(Compilation compilation)
    {
        _stringType = compilation.GetTypeByMetadataName("System.String");
        _cultureType = compilation.GetTypeByMetadataName("System.Globalization.CultureInfo");
        var charType = compilation.GetTypeByMetadataName("System.Char");
        if (charType != null)
        {
            _readOnlySpanType = compilation.GetTypeByMetadataName("System.ReadOnlySpan`1")?.Construct(charType);
        }
    }

    public string? GetConverter(ITypeSymbol type)
    {
        if (_converters.TryGetValue(type, out var converter))
        {
            return converter.ConstructorCall;
        }

        var optionalInfo = FindParseMethod(type) ?? FindConstructor(type);
        if (optionalInfo is not ConverterInfo info)
        {
            return null;
        }

        info.Name = GenerateName(type.ToDisplayString());
        _converters.Add(type, info);
        return info.ConstructorCall;
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
            if (SymbolEqualityComparer.Default.Equals(_readOnlySpanType, ctor.Parameters[0].Type))
            {
                newInfo.UseSpan = true;
                info = newInfo;
                // Won't find a better one
                break;
            }
            else if (!SymbolEqualityComparer.Default.Equals(_stringType, ctor.Parameters[0].Type))
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
            if (SymbolEqualityComparer.Default.Equals(_readOnlySpanType, method.Parameters[0].Type))
            {
                newInfo.UseSpan = true;
            }
            else if (!SymbolEqualityComparer.Default.Equals(_stringType, method.Parameters[0].Type))
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

    private static string GenerateName(string displayName)
    {
        return displayName.ToIdentifier(ConverterSuffix);
    }

    private static void CreateConverter(SourceBuilder builder, ITypeSymbol type, ConverterInfo info)
    {
        // TODO: Handle exceptions similar to reflection versions.
        builder.AppendLine($"internal class {info.Name} : Ookii.CommandLine.Conversion.ArgumentConverter");
        builder.OpenBlock();
        string inputType = info.UseSpan ? "System.ReadOnlySpan<char>" : "string";
        string culture = info.HasCulture ? ", culture" : string.Empty;
        if (info.ParseMethod)
        {
            builder.AppendLine($"public override object? Convert({inputType} value, System.Globalization.CultureInfo culture, Ookii.CommandLine.CommandLineArgument argument) => {type.ToDisplayString()}.Parse(value{culture});");
        }
        else
        {
            builder.AppendLine($"public override object? Convert({inputType} value, System.Globalization.CultureInfo culture, Ookii.CommandLine.CommandLineArgument argument) => new {type.ToDisplayString()}(value);");
        }

        if (info.UseSpan)
        {
            builder.AppendLine("public override object? Convert(string value, System.Globalization.CultureInfo culture, Ookii.CommandLine.CommandLineArgument argument) => Convert(System.MemoryExtensions.AsSpan(value), culture, argument);");
        }

        builder.CloseBlock(); // class
        builder.AppendLine();
    }
}
