using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;

// Nullability is disabled for this file because there are some differences for both reflection and
// source generation in how nullable and non-nullable contexts are handled and both need to be
// tested.
#nullable disable

// We deliberately have some properties and methods that cause warnings, so disable those.
#pragma warning disable OCL0017,OCL0018,OCL0020,OCL0023,OCL0029,OCL0033,OCL0038,OCL0039

namespace Ookii.CommandLine.Tests;

[GeneratedParser]
partial class EmptyArguments
{
}

[GeneratedParser]
[ApplicationFriendlyName("Friendly name")]
[Description("Test arguments description.")]
partial class TestArguments
{
    private readonly Collection<int> _arg12 = new Collection<int>();
    private readonly Dictionary<string, int> _arg14 = new Dictionary<string, int>();

    [CommandLineArgument("arg1", Position = 1, IsRequired = true)]
    [Description("Arg1 description.")]
    public string Arg1 { get; set; }

    [CommandLineArgument("other", Position = 2, DefaultValue = 42)]
    [ValueDescription("Number")]
    [Description("Arg2 description.")]
    public int Arg2 { get; set; }

    [CommandLineArgument("notSwitch", Position = 3, DefaultValue = false)]
    public bool NotSwitch { get; set; }

    [CommandLineArgument()]
    public string Arg3 { get; set; }

    // Default value is intentionally a string to test default value conversion.
    [CommandLineArgument("other2", DefaultValue = "47", Position = 5), Description("Arg4 description.")]
    [ValueDescription("Number")]
    [ValidateRange(0, 1000, IncludeInUsageHelp = false)]
    [ArgumentConverter(typeof(WrappedDefaultTypeConverter<int>))]
    public int Arg4 { get; set; }

    // Short/long name stuff should be ignored if not using LongShort mode.
    [CommandLineArgument(Position = 4, ShortName = 'a', IsLong = false, DefaultValue = 1.0f, IncludeDefaultInUsageHelp = false)]
    [Description("Arg5 description.")]
    public float Arg5 { get; set; }

    [Alias("Alias1")]
    [Alias("Alias2")]
    [CommandLineArgument(IsRequired = true), Description("Arg6 description.")]
    public string Arg6 { get; set; }

    [Alias("Alias3")]
    [CommandLineArgument()]
    public bool Arg7 { get; set; }

    [CommandLineArgument(Position = 6)]
    public DayOfWeek[] Arg8 { get; set; }

    [CommandLineArgument()]
    [ValidateRange(0, 100)]
    public int? Arg9 { get; set; }

    [CommandLineArgument]
    public bool[] Arg10 { get; set; }

    [CommandLineArgument]
    public bool? Arg11 { get; set; }

    [CommandLineArgument(DefaultValue = 42)] // Default value is ignored for collection types.
    public Collection<int> Arg12
    {
        get { return _arg12; }
    }

    [CommandLineArgument]
    public Dictionary<string, int> Arg13 { get; set; }

    [CommandLineArgument]
    public IDictionary<string, int> Arg14
    {
        get { return _arg14; }
    }

    [CommandLineArgument, ArgumentConverter(typeof(KeyValuePairConverter<string, int>))]
    public KeyValuePair<string, int> Arg15 { get; set; }

    public string NotAnArg { get; set; }

    [CommandLineArgument()]
    private string NotAnArg2 { get; set; }

    [CommandLineArgument()]
    public static string NotAnArg3 { get; set; }
}

[GeneratedParser]
partial class ThrowingArguments
{
    private int _throwingArgument;

    [CommandLineArgument(Position = 0)]
    public string Arg { get; set; }

    [CommandLineArgument]
    public int ThrowingArgument
    {
        get { return _throwingArgument; }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _throwingArgument = value;
        }
    }
}

[GeneratedParser]
partial class ThrowingConstructor
{
    public ThrowingConstructor()
    {
        throw new ArgumentException();
    }

    [CommandLineArgument]
    public int Arg { get; set; }
}

[GeneratedParser]
partial class DictionaryArguments
{
    [CommandLineArgument]
    public Dictionary<string, int> NoDuplicateKeys { get; set; }
    [CommandLineArgument, AllowDuplicateDictionaryKeys]
    public Dictionary<string, int> DuplicateKeys { get; set; }
}

[GeneratedParser]
partial class MultiValueSeparatorArguments
{
    [CommandLineArgument]
    public string[] NoSeparator { get; set; }
    [CommandLineArgument, MultiValueSeparator(",")]
    public string[] Separator { get; set; }
}

[GeneratedParser]
partial class SimpleArguments
{
    [CommandLineArgument]
    public string Argument1 { get; set; }
    [CommandLineArgument]
    public string Argument2 { get; set; }
}

[GeneratedParser]
partial class KeyValueSeparatorArguments
{
    [CommandLineArgument]
    public Dictionary<string, int> DefaultSeparator { get; set; }

    [CommandLineArgument]
    [KeyValueSeparator("<=>")]
    public Dictionary<string, string> CustomSeparator { get; set; }
}

[GeneratedParser]
partial class CancelArguments
{
    [CommandLineArgument]
    public string Argument1 { get; set; }

    [CommandLineArgument]
    public string Argument2 { get; set; }

    [CommandLineArgument]
    public bool DoesNotCancel { get; set; }

    [CommandLineArgument(CancelParsing = CancelMode.Abort)]
    public bool DoesCancel { get; set; }

    [CommandLineArgument(CancelParsing = CancelMode.Success)]
    public bool DoesCancelWithSuccess { get; set; }
}

[GeneratedParser]
[ParseOptions(
    Mode = ParsingMode.LongShort,
    DuplicateArguments = ErrorMode.Allow,
    AllowWhiteSpaceValueSeparator = false,
    ArgumentNamePrefixes = new[] { "--", "-" },
    LongArgumentNamePrefix = "---",
    CaseSensitive = true,
    NameValueSeparators = new[] { '=' },
    AutoHelpArgument = false)]
partial class ParseOptionsArguments
{
    [CommandLineArgument]
    public string Argument { get; set; }
}

[GeneratedParser]
partial class CultureArguments
{
    [CommandLineArgument]
    public float Argument { get; set; }
}

[GeneratedParser]
[ParseOptions(Mode = ParsingMode.LongShort)]
partial class LongShortArguments
{
    public static bool Switch2Value { get; set; }

    [CommandLineArgument, ShortAlias('c')]
    [Description("Arg1 description.")]
    public int Arg1 { get; set; }

    [CommandLineArgument(ShortName = 'a', Position = 2), ShortAlias('b'), Alias("baz")]
    [Description("Arg2 description.")]
    public int Arg2 { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Description("Switch1 description.")]
    public bool Switch1 { get; set; }

    [CommandLineArgument(ShortName = 'k')]
    [Alias("Switch2Alias")]
    [ShortAlias('x')]
    [Description("Switch2 description.")]
    public static void Switch2(bool value)
    {
        Switch2Value = value;
    }

    [CommandLineArgument(ShortName = 'u', IsLong = false)]
    [Description("Switch3 description.")]
    public bool Switch3 { get; set; }

    [CommandLineArgument("foo", Position = 0, IsShort = true, DefaultValue = 0)]
    [Description("Foo description.")]
    public int Foo { get; set; }

    [CommandLineArgument("bar", DefaultValue = 0, Position = 1)]
    [Description("Bar description.")]
    public int Bar { get; set; }
}

[GeneratedParser]
partial class MethodArguments
{
    // Using method arguments to store stuff in static fields isn't really recommended. It's
    // done here for testing purposes only.
    public static string CalledMethodName;
    public static int Value;

    [CommandLineArgument]
    public static bool NoCancel()
    {
        CalledMethodName = nameof(NoCancel);
        return true;
    }

    [CommandLineArgument]
    public static bool Cancel()
    {
        CalledMethodName = nameof(Cancel);
        return false;
    }

    [CommandLineArgument]
    public static CancelMode CancelModeAbort()
    {
        CalledMethodName = nameof(CancelModeAbort);
        return CancelMode.Abort;
    }

    [CommandLineArgument]
    public static CancelMode CancelModeSuccess()
    {
        CalledMethodName = nameof(CancelModeSuccess);
        return CancelMode.Success;
    }

    [CommandLineArgument]
    public static CancelMode CancelModeNone()
    {
        CalledMethodName = nameof(CancelModeNone);
        return CancelMode.None;
    }

    [CommandLineArgument]
    public static bool CancelWithHelp(CommandLineParser parser)
    {
        CalledMethodName = nameof(CancelWithHelp);
        parser.HelpRequested = true;
        return false;
    }

    [CommandLineArgument]
    public static bool CancelWithValue(int value)
    {
        CalledMethodName = nameof(CancelWithValue);
        Value = value;
        return value > 0;
    }

    [CommandLineArgument]
    public static bool CancelWithValueAndHelp(int value, CommandLineParser parser)
    {
        CalledMethodName = nameof(CancelWithValueAndHelp);
        Value = value;
        // This should be reset to false if parsing continues.
        parser.HelpRequested = true;
        return value > 0;
    }

    [CommandLineArgument]
    public static void NoReturn()
    {
        CalledMethodName = nameof(NoReturn);
    }

    [CommandLineArgument(Position = 0)]
    public static void Positional(int value)
    {
        CalledMethodName = nameof(Positional);
        Value = value;
    }

    [CommandLineArgument]
    public void NoStatic()
    {
    }

    [CommandLineArgument]
    private static void NotPublic()
    {
    }

    public static void NotAnArgument()
    {
    }
}

[GeneratedParser]
partial class AutomaticConflictingNameArguments
{
    [CommandLineArgument]
    public int Help { get; set; }

    [CommandLineArgument]
    public int Version { get; set; }
}

[GeneratedParser]
[ParseOptions(Mode = ParsingMode.LongShort)]
partial class AutomaticConflictingShortNameArguments
{
    [CommandLineArgument(ShortName = '?')]
    public int Foo { get; set; }
}

[GeneratedParser]
partial class HiddenArguments
{
    [CommandLineArgument]
    public int Foo { get; set; }

    [CommandLineArgument(IsHidden = true)]
    public int Hidden { get; set; }
}

[GeneratedParser]
partial class NameTransformArguments
{
    [CommandLineArgument(Position = 0, IsRequired = true)]
    public string testArg { get; set; }

    [CommandLineArgument]
    public int TestArg2 { get; set; }

    [CommandLineArgument]
    public int __test__arg3__ { get; set; }

    [CommandLineArgument("ExplicitName")]
    public int Explicit { get; set; }
}

[GeneratedParser]
partial class ValueDescriptionTransformArguments
{
    [CommandLineArgument]
    public FileInfo Arg1 { get; set; }

    [CommandLineArgument]
    public int Arg2 { get; set; }
}

[GeneratedParser]
partial class ValidationArguments
{
    public static int Arg3Value { get; set; }

    [CommandLineArgument]
    [Description("Arg1 description.")]
    [ValidateRange(1, 5)]
    public int? Arg1 { get; set; }

    [CommandLineArgument("arg2", Position = 0)]
    [ValidateNotEmpty, Description("Arg2 description.")]
    public string Arg2 { get; set; }

    [CommandLineArgument]
    [Description("Arg3 description.")]
    [ValidatePattern("^[0-7]{4}$")]
    [ValidateRange(1000, 7000)]
    public static void Arg3(int value)
    {
        Arg3Value = value;
    }

    [CommandLineArgument]
    [Description("Arg4 description.")]
    [MultiValueSeparator(";")]
    [ValidateStringLength(1, 3)]
    [ValidateCount(2, 4)]
    public string[] Arg4 { get; set; }

    [CommandLineArgument]
    [Description("Day description.")]
    [ValidateEnumValue]
    public DayOfWeek Day { get; set; }

    [CommandLineArgument]
    [Description("Day2 description.")]
    [ValidateEnumValue(CaseSensitive = true, AllowNonDefinedValues = true, AllowCommaSeparatedValues = false)]
    public DayOfWeek? Day2 { get; set; }

    [CommandLineArgument(IsHidden = true)]
    [Description("Day3 description.")]
    [ValidateEnumValue(AllowNumericValues = false)]
    public DayOfWeek Day3 { get; set; }

    [CommandLineArgument]
    [Description("NotNull description.")]
    [ValidateNotNull]
    public int? NotNull { get; set; }
}

[GeneratedParser]
// N.B. nameof is only safe if the argument name matches the property name.
[RequiresAny(nameof(Address), nameof(Path))]
partial class DependencyArguments
{
    [CommandLineArgument]
    [Description("The address.")]
    public IPAddress Address { get; set; }

    [CommandLineArgument(DefaultValue = (short)5000)]
    [Description("The port.")]
    [Requires(nameof(Address))]
    public short Port { get; set; }

    [CommandLineArgument]
    [Description("The throughput.")]
    public int Throughput { get; set; }

    [CommandLineArgument]
    [Description("The protocol.")]
    [Requires(nameof(Address), nameof(Throughput))]
    public int Protocol { get; set; }

    [CommandLineArgument]
    [Description("The path.")]
    [Prohibits("Address")]
    public FileInfo Path { get; set; }
}

[GeneratedParser]
partial class MultiValueWhiteSpaceArguments
{

    [CommandLineArgument(Position = 0)]
    public int Arg1 { get; set; }

    [CommandLineArgument(Position = 1)]
    public int Arg2 { get; set; }

    [CommandLineArgument]
    [MultiValueSeparator]
    public int[] Multi { get; set; }

    [CommandLineArgument]
    [MultiValueSeparator]
    public int Other { get; set; }


    [CommandLineArgument]
    [MultiValueSeparator]
    public bool[] MultiSwitch { get; set; }
}

[GeneratedParser]
partial class InjectionArguments
{
    private readonly CommandLineParser _parser;

    public InjectionArguments(CommandLineParser parser)
    {
        _parser = parser;
    }

    public CommandLineParser Parser => _parser;

    [CommandLineArgument]
    public int Arg { get; set; }
}

struct StructWithParseCulture
{
    public int Value { get; set; }

    public static StructWithParseCulture Parse(string value, IFormatProvider provider)
    {
        return new StructWithParseCulture()
        {
            Value = int.Parse(value, provider)
        };
    }
}

struct StructWithParse
{
    public int Value { get; set; }

    public static StructWithParse Parse(string value)
    {
        return new StructWithParse()
        {
            Value = int.Parse(value, CultureInfo.InvariantCulture)
        };
    }
}

struct StructWithCtor
{
    public StructWithCtor(string value)
    {
        Value = int.Parse(value);
    }

    public int Value { get; set; }
}

[GeneratedParser]
partial class ConversionArguments
{
    [CommandLineArgument]
    public StructWithParseCulture ParseCulture { get; set; }

    [CommandLineArgument]
    public StructWithParse ParseStruct { get; set; }

    [CommandLineArgument]
    public StructWithCtor Ctor { get; set; }

    [CommandLineArgument]
    public StructWithParse? ParseNullable { get; set; }

    [CommandLineArgument]
    [MultiValueSeparator]
    public StructWithParse[] ParseMulti { get; set; }

    [CommandLineArgument]
    [MultiValueSeparator]
    public StructWithParse?[] ParseNullableMulti { get; set; }

    [CommandLineArgument]
    [MultiValueSeparator]
    public int?[] NullableMulti { get; set; }

    [CommandLineArgument]
    public int? Nullable { get; set; }
}

[Description("Base class attribute.")]
class BaseArguments
{
    [CommandLineArgument]
    public string BaseArg { get; set; }
}

[GeneratedParser]
partial class DerivedArguments : BaseArguments
{
    [CommandLineArgument]
    public int DerivedArg { get; set; }
}

[GeneratedParser]
partial class InitializerDefaultValueArguments
{
    [CommandLineArgument]
    public string Arg1 { get; set; } = "foo\tbar\"";

    [CommandLineArgument]
    public float Arg2 { get; set; } = 5.5f;

    [CommandLineArgument]
    public int Arg3 { get; set; } = int.MaxValue;

    [CommandLineArgument]
    public DayOfWeek Arg4 { get; set; } = DayOfWeek.Tuesday;

    [CommandLineArgument]
    public int Arg5 { get; set; } = Value;

    [CommandLineArgument]
    public int Arg6 { get; set; } = GetValue();

    [CommandLineArgument]
    public int Arg7 { get; set; } = default;

#nullable enable
    [CommandLineArgument]
    public string? Arg8 { get; set; } = default!;

    [CommandLineArgument]
    public string? Arg9 { get; set; } = null!;
#nullable disable

    [CommandLineArgument(IncludeDefaultInUsageHelp = false)]
    public int Arg10 { get; set; } = 10;

    private const int Value = 47;

    public static int GetValue() => 42;

}

[GeneratedParser]
partial class AutoPrefixAliasesArguments
{
    [CommandLineArgument(IsShort = true)]
    public string Protocol { get; set; }

    [CommandLineArgument]
    public int Port { get; set; }

    [CommandLineArgument(IsShort = true)]
    [Alias("Prefix")]
    public bool EnablePrefix { get; set; }
}

class AutoPositionArgumentsBase
{
    [CommandLineArgument(IsPositional = true, IsRequired = true)]
    public string BaseArg1 { get; set; }

    [CommandLineArgument(IsPositional = true)]
    public int BaseArg2 { get; set; }

    [CommandLineArgument]
    public int BaseArg3 { get; set; }
}

[GeneratedParser]
partial class AutoPositionArguments : AutoPositionArgumentsBase
{
    [CommandLineArgument(IsPositional = true)]
    public string Arg1 { get; set; }

    [CommandLineArgument(IsPositional = true)]
    public int Arg2 { get; set; }

    [CommandLineArgument]
    public int Arg3 { get; set; }
}

[GeneratedParser]
partial class EmptyLineDescriptionArguments
{
    [CommandLineArgument]
    [Description("A description with\n\na blank line.")]
    public string Argument { get; set; }
}

[GeneratedParser]
partial class DefaultValueFormatArguments
{
    [CommandLineArgument(DefaultValue = 1.5, DefaultValueFormat = "({0:0.00})")]
    [Description("An argument.")]
    public double Argument { get; set; }

    [CommandLineArgument(DefaultValue = 3.5)]
    [Description("Another argument.")]
    public double Argument2 { get; set; }
}
