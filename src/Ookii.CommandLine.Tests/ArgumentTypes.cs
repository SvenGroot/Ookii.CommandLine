using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;

namespace Ookii.CommandLine.Tests
{
    class EmptyArguments
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

        [CommandLineArgument("other", Position = 2, DefaultValue = 42, ValueDescription = "Number")]
        [Description("Arg2 description.")]
        public int Arg2 { get; set; }

        [CommandLineArgument("notSwitch", Position = 3, DefaultValue = false)]
        public bool NotSwitch { get; set; }

        [CommandLineArgument()]
        public string Arg3 { get; set; }

        // Default value is intentionally a string to test default value conversion.
        [CommandLineArgument("other2", DefaultValue = "47", ValueDescription = "Number", Position = 5), Description("Arg4 description.")]
        [ValidateRange(0, 1000, IncludeInUsageHelp = false)]
        public int Arg4 { get; set; }

        // Short/long name stuff should be ignored if not using LongShort mode.
        [CommandLineArgument(Position = 4, ShortName = 'a', IsLong = false), Description("Arg5 description.")]
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

    class ThrowingArguments
    {
        private int _throwingArgument;

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

    class ThrowingConstructor
    {
        public ThrowingConstructor()
        {
            throw new ArgumentException();
        }

        [CommandLineArgument]
        public int Arg { get; set; }
    }

    class DictionaryArguments
    {
        [CommandLineArgument]
        public Dictionary<string, int> NoDuplicateKeys { get; set; }
        [CommandLineArgument, AllowDuplicateDictionaryKeys]
        public Dictionary<string, int> DuplicateKeys { get; set; }
    }

    class MultiValueSeparatorArguments
    {
        [CommandLineArgument]
        public string[] NoSeparator { get; set; }
        [CommandLineArgument, MultiValueSeparator(",")]
        public string[] Separator { get; set; }
    }

    class SimpleArguments
    {
        [CommandLineArgument]
        public string Argument1 { get; set; }
        [CommandLineArgument]
        public string Argument2 { get; set; }
    }

    class KeyValueSeparatorArguments
    {
        [CommandLineArgument]
        public Dictionary<string, int> DefaultSeparator { get; set; }

        [CommandLineArgument]
        [KeyValueSeparator("<=>")]
        public Dictionary<string, string> CustomSeparator { get; set; }
    }

    class CancelArguments
    {
        [CommandLineArgument]
        public string Argument1 { get; set; }

        [CommandLineArgument]
        public string Argument2 { get; set; }

        [CommandLineArgument]
        public bool DoesNotCancel { get; set; }

        [CommandLineArgument(CancelParsing = true)]
        public bool DoesCancel { get; set; }
    }

    [ParseOptions(
        Mode = ParsingMode.LongShort,
        DuplicateArguments = ErrorMode.Allow,
        AllowWhiteSpaceValueSeparator = false,
        ArgumentNamePrefixes = new[] { "--", "-" },
        LongArgumentNamePrefix = "---",
        CaseSensitive = true,
        NameValueSeparator = '=',
        AutoHelpArgument = false)]
    class ParseOptionsArguments
    {
        [CommandLineArgument]
        public string Argument { get; set; }
    }

    class CultureArguments
    {
        [CommandLineArgument]
        public float Argument { get; set; }
    }

    [ParseOptions(Mode = ParsingMode.LongShort)]
    class LongShortArguments
    {
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
        [Description("Switch2 description.")]
        public bool Switch2 { get; set; }

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

    class MethodArguments
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
        public void NotStatic()
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

    class AutomaticConflictingNameArguments
    {
        [CommandLineArgument]
        public int Help { get; set; }

        [CommandLineArgument]
        public int Version { get; set; }
    }

    [ParseOptions(Mode = ParsingMode.LongShort)]
    class AutomaticConflictingShortNameArguments
    {
        [CommandLineArgument(ShortName = '?')]
        public int Foo { get; set; }
    }

    class HiddenArguments
    {
        [CommandLineArgument]
        public int Foo { get; set; }

        [CommandLineArgument(IsHidden = true)]
        public int Hidden { get; set; }
    }

    class NameTransformArguments
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

    class ValueDescriptionTransformArguments
    {
        [CommandLineArgument]
        public FileInfo Arg1 { get; set; }

        [CommandLineArgument]
        public int Arg2 { get; set; }
    }

    class ValidationArguments
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
        [ValidateEnumValue]
        public DayOfWeek? Day2 { get; set; }

        [CommandLineArgument]
        [Description("NotNull description.")]
        [ValidateNotNull]
        public int? NotNull { get; set; }
    }

    // N.B. nameof is only safe if the argument name matches the property name.
    [RequiresAny(nameof(Address), nameof(Path))]
    class DependencyArguments
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

    class MultiValueWhiteSpaceArguments
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

    class InjectionArguments
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

    // TODO: Test with new ctor argument style.
    //class InjectionMixedArguments
    //{
    //    private readonly CommandLineParser _parser;
    //    private readonly int _arg1;
    //    private readonly int _arg2;

    //    public InjectionMixedArguments(int arg1, CommandLineParser parser, int arg2)
    //    {
    //        _arg1 = arg1;
    //        _parser = parser;
    //        _arg2 = arg2;
    //    }

    //    public CommandLineParser Parser => _parser;

    //    public int Arg1 => _arg1;

    //    public int Arg2 => _arg2;

    //    [CommandLineArgument]
    //    public int Arg3 { get; set; }
    //}

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

    class ConversionArguments
    {
        [CommandLineArgument]
        public StructWithParseCulture ParseCulture { get; set; }

        [CommandLineArgument]
        public StructWithParse Parse { get; set; }

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
}
