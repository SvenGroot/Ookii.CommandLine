using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Tests
{
    class EmptyArguments
    {
    }

    [System.ComponentModel.Description("Test arguments description.")]
    class TestArguments
    {
        private readonly Collection<int> _arg12 = new Collection<int>();
        private readonly Dictionary<string, int> _arg14 = new Dictionary<string, int>();

        private TestArguments(string notAnArg)
        {
        }

        public TestArguments([System.ComponentModel.Description("Arg1 description.")] string arg1, [System.ComponentModel.Description("Arg2 description."), ArgumentName("other"), ValueDescription("Number")] int arg2 = 42, bool notSwitch = false)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            NotSwitch = notSwitch;
        }

        public string Arg1 { get; private set; }

        public int Arg2 { get; private set; }

        public bool NotSwitch { get; private set; }

        [CommandLineArgument()]
        public string Arg3 { get; set; }

        // Default value is intentionally a string to test default value conversion.
        [CommandLineArgument("other2", DefaultValue = "47", ValueDescription = "Number", Position = 1), System.ComponentModel.Description("Arg4 description.")]
        public int Arg4 { get; set; }

        // Short/long name stuff should be ignored if not using LongShort mode.
        [CommandLineArgument(Position = 0, ShortName = 'a', Long = false), System.ComponentModel.Description("Arg5 description.")]
        public float Arg5 { get; set; }

        [Alias("Alias1")]
        [Alias("Alias2")]
        [CommandLineArgument(IsRequired = true), System.ComponentModel.Description("Arg6 description.")]
        public string Arg6 { get; set; }

        [Alias("Alias3")]
        [CommandLineArgument()]
        public bool Arg7 { get; set; }

        [CommandLineArgument(Position = 2)]
        public DayOfWeek[] Arg8 { get; set; }

        [CommandLineArgument()]
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

        [CommandLineArgument, TypeConverter(typeof(KeyValuePairConverter<string, int>))]
        public KeyValuePair<string, int> Arg15 { get; set; }

        public string NotAnArg { get; set; }

        [CommandLineArgument()]
        private string NotAnArg2 { get; set; }

        [CommandLineArgument()]
        public static string NotAnArg3 { get; set; }
    }

    class MultipleConstructorsArguments
    {
        private int _throwingArgument;

        public MultipleConstructorsArguments() { }
        public MultipleConstructorsArguments(string notArg1, int notArg2) { }
        [CommandLineConstructor]
        public MultipleConstructorsArguments(string arg1)
        {
            if (arg1 == "invalid")
                throw new ArgumentException("Invalid argument value.", nameof(arg1));
        }

        [CommandLineArgument]
        public int ThrowingArgument
        {
            get { return _throwingArgument; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _throwingArgument = value;
            }
        }

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
        AllowDuplicateArguments = true,
        AllowWhiteSpaceValueSeparator = false,
        ArgumentNamePrefixes = new[] { "--", "-" },
        LongArgumentNamePrefix = "---",
        CaseSensitive = true,
        NameValueSeparator = '=')]
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
        public LongShortArguments([ArgumentName(Short = true)] int foo = 0, int bar = 0)
        {
            Foo = foo;
            Bar = bar;
        }

        [CommandLineArgument]
        public int Arg1 { get; set; }

        [CommandLineArgument(ShortName = 'a')]
        public int Arg2 { get; set; }

        public int Foo { get; set; }

        public int Bar { get; set; }

        [CommandLineArgument(Short = true)]
        public bool Switch1 { get; set; }

        [CommandLineArgument(ShortName = 't')]
        public bool Switch2 { get; set; }

        [CommandLineArgument(ShortName = 'u', Long = false)]
        public bool Switch3 { get; set; }
    }

}
