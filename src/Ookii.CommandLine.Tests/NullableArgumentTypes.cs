#if NET6_0_OR_GREATER
#nullable enable

using Ookii.CommandLine.Conversion;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ookii.CommandLine.Tests;

// We deliberately have some properties and methods that cause warnings, so disable those.
#pragma warning disable OCL0021,OCL0033

class NullReturningStringConverter : ArgumentConverter
{
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        if (value.Span.Equals("(null)".AsSpan(), StringComparison.Ordinal))
        {
            return null;
        }
        else
        {
            return value.ToString();
        }
    }
}

class NullReturningIntConverter : ArgumentConverter
{
    public override object? Convert(ReadOnlyMemory<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        if (value.Span.Equals("(null)".AsSpan(), StringComparison.Ordinal))
        {
            return null;
        }
        else
        {
            return int.Parse(value.ToString());
        }
    }
}

[GeneratedParser]
partial class NullableArguments
{
    [CommandLineArgument("constructorNullable", Position = 0)]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public string? ConstructorNullable { get; set; }

    [CommandLineArgument("constructorNonNullable", Position = 1)]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public string ConstructorNonNullable { get; set; } = default!;

    [CommandLineArgument("constructorValueType", Position = 2)]
    [ArgumentConverter(typeof(NullReturningIntConverter))]
    public int ConstructorValueType { get; set; }

    [CommandLineArgument("constructorNullableValueType", Position = 3)]
    [ArgumentConverter(typeof(NullReturningIntConverter))]
    public int? ConstructorNullableValueType { get; set; }

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public string? Nullable { get; set; } = "NotNullDefaultValue";

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public string NonNullable { get; set; } = string.Empty;

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningIntConverter))]
    public int ValueType { get; set; }

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningIntConverter))]
    public int? NullableValueType { get; set; } = 42;

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public string[]? NonNullableArray { get; set; }

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningIntConverter))]
    public int[]? ValueArray { get; set; }

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public ICollection<string> NonNullableCollection { get; } = new List<string>();

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningIntConverter))]
    [MultiValueSeparator(";")]
    public ICollection<int> ValueCollection { get; } = new List<int>();

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public string?[]? NullableArray { get; set; }

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningIntConverter))]
    public string?[]? NullableValueArray { get; set; }

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public ICollection<string?> NullableCollection { get; } = new List<string?>();

    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public ICollection<int?> NullableValueCollection { get; } = new List<int?>();

    [CommandLineArgument]
    [KeyConverter(typeof(NullReturningStringConverter))]
    [ValueConverter(typeof(NullReturningStringConverter))]
    public Dictionary<string, string>? NonNullableDictionary { get; set; }

    [CommandLineArgument]
    [ValueConverter(typeof(NullReturningIntConverter))]
    public Dictionary<string, int>? ValueDictionary { get; set; }

    [CommandLineArgument]
    [ValueConverter(typeof(NullReturningStringConverter))]
    public IDictionary<string, string> NonNullableIDictionary { get; } = new Dictionary<string, string>();

    [CommandLineArgument]
    [KeyConverter(typeof(NullReturningStringConverter))]
    [ValueConverter(typeof(NullReturningIntConverter))]
    [MultiValueSeparator(";")]
    public IDictionary<string, int> ValueIDictionary { get; } = new Dictionary<string, int>();

    [CommandLineArgument]
    [KeyConverter(typeof(NullReturningStringConverter))]
    [ValueConverter(typeof(NullReturningStringConverter))]
    public Dictionary<string, string?>? NullableDictionary { get; set; }

    [CommandLineArgument]
    [KeyConverter(typeof(NullReturningStringConverter))]
    [ValueConverter(typeof(NullReturningIntConverter))]
    public Dictionary<string, int?>? NullableValueDictionary { get; set; }

    [CommandLineArgument]
    [KeyConverter(typeof(NullReturningStringConverter))]
    [ValueConverter(typeof(NullReturningStringConverter))]
    public IDictionary<string, string?> NullableIDictionary { get; } = new Dictionary<string, string?>();

    [CommandLineArgument]
    [KeyConverter(typeof(NullReturningStringConverter))]
    [ValueConverter(typeof(NullReturningIntConverter))]
    [MultiValueSeparator(";")]
    public IDictionary<string, int?> NullableValueIDictionary { get; } = new Dictionary<string, int?>();

    // This is an incorrect type converter (doesn't return KeyValuePair), but it doesn't
    // matter since it'll only be used to test null values.
    [CommandLineArgument]
    [ArgumentConverter(typeof(NullReturningStringConverter))]
    public Dictionary<string, string?>? InvalidDictionary { get; set; }
}

#endif

#if NET7_0_OR_GREATER

[GeneratedParser]
partial class RequiredPropertyArguments
{
    [CommandLineArgument]
    public required string Arg1 { get; set; }

    [CommandLineArgument]
    public string? Arg2 { get; set; }

    // IsRequired is ignored
    [CommandLineArgument(IsRequired = false)]
    public required string? Foo { get; init; }

    [CommandLineArgument]
    public required int[] Bar { get; set; }
}

#endif
