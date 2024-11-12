namespace Ookii.CommandLine.Tests;

partial class CommandLineParserTest
{
    private const string _executableName = "test";

    private static readonly string _expectedDefaultUsage = @"Test arguments description.

Usage: test [/arg1] <String> [[/other] <Number>] [[/notSwitch] <Boolean>] [[/Arg5] <Single>] [[/other2] <Number>] [[/Arg8] <DayOfWeek>...] /Arg6 <String> [/Arg10...] [/Arg11] [/Arg12 <Int32>...] [/Arg13 <String=Int32>...] [/Arg14 <String=Int32>...] [/Arg15 <KeyValuePair<String, Int32>>] [/Arg3 <String>] [/Arg7] [/Arg9 <Int32>] [/Help] [/NotSwitch2 <Boolean>] [/Version]

    /arg1 <String>
        Arg1 description.

    /other <Number>
        Arg2 description. Default value: 42.

    /notSwitch <Boolean>
         Default value: False.

    /Arg5 <Single>
        Arg5 description.

    /other2 <Number>
        Arg4 description. Default value: 47.

    /Arg6 <String> (/Alias1, /Alias2)
        Arg6 description.

    /Arg12 <Int32>
         Default value: 42.

    /Arg7 [<Boolean>] (/Alias3)


    /Arg9 <Int32>
         Must be between 0 and 100.

    /Help [<Boolean>] (/?, /h)
        Displays this help message.

    /NotSwitch2 <Boolean>
        NotSwitch2 description.

    /Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedLongShortUsage = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [[--Arg2] <Int32>] [--Arg1 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

        --Arg1 <Int32>
            Arg1 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -k, --Switch2 [<Boolean>] (-x, --Switch2Alias)
            Switch2 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedLongShortUsageShortNameSyntax = @"Usage: test [[-f] <Int32>] [[--bar] <Int32>] [[-a] <Int32>] [--Arg1 <Int32>] [-?] [-S] [-k] [-u] [--Version]

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

        --Arg1 <Int32>
            Arg1 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -k, --Switch2 [<Boolean>] (-x, --Switch2Alias)
            Switch2 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedLongShortUsageAbbreviated = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [[--Arg2] <Int32>] [arguments]

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

        --Arg1 <Int32>
            Arg1 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -k, --Switch2 [<Boolean>] (-x, --Switch2Alias)
            Switch2 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageDescriptionOnly = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-NotSwitch2 <Boolean>] [-Version]

    -arg1 <String>
        Arg1 description.

    -other <Number>
        Arg2 description. Default value: 42.

    -Arg5 <Single>
        Arg5 description.

    -other2 <Number>
        Arg4 description. Default value: 47.

    -Arg6 <String> (-Alias1, -Alias2)
        Arg6 description.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -NotSwitch2 <Boolean>
        NotSwitch2 description.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageAll = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-NotSwitch2 <Boolean>] [-Version]

    -arg1 <String>
        Arg1 description.

    -other <Number>
        Arg2 description. Default value: 42.

    -notSwitch <Boolean>
         Default value: False.

    -Arg5 <Single>
        Arg5 description.

    -other2 <Number>
        Arg4 description. Default value: 47.

    -Arg8 <DayOfWeek>


    -Arg6 <String> (-Alias1, -Alias2)
        Arg6 description.

    -Arg10 [<Boolean>]


    -Arg11 [<Boolean>]


    -Arg12 <Int32>
         Default value: 42.

    -Arg13 <String=Int32>


    -Arg14 <String=Int32>


    -Arg15 <KeyValuePair<String, Int32>>


    -Arg3 <String>


    -Arg7 [<Boolean>] (-Alias3)


    -Arg9 <Int32>
         Must be between 0 and 100.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -NotSwitch2 <Boolean>
        NotSwitch2 description.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageNone = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-NotSwitch2 <Boolean>] [-Version]

".ReplaceLineEndings();

    // Raw strings would be nice here so including the escape character directly wouldn't be
    // necessary but that requires C# 11.
    private static readonly string _expectedUsageColor = @"Test arguments description.

[36mUsage:[0m test [/arg1] <String> [[/other] <Number>] [[/notSwitch] <Boolean>] [[/Arg5] <Single>] [[/other2] <Number>] [[/Arg8] <DayOfWeek>...] /Arg6 <String> [/Arg10...] [/Arg11] [/Arg12 <Int32>...] [/Arg13 <String=Int32>...] [/Arg14 <String=Int32>...] [/Arg15 <KeyValuePair<String, Int32>>] [/Arg3 <String>] [/Arg7] [/Arg9 <Int32>] [/Help] [/NotSwitch2 <Boolean>] [/Version]

    [32m/arg1 <String>[0m
        Arg1 description.

    [32m/other <Number>[0m
        Arg2 description. Default value: 42.

    [32m/notSwitch <Boolean>[0m
         Default value: False.

    [32m/Arg5 <Single>[0m
        Arg5 description.

    [32m/other2 <Number>[0m
        Arg4 description. Default value: 47.

    [32m/Arg6 <String> (/Alias1, /Alias2)[0m
        Arg6 description.

    [32m/Arg12 <Int32>[0m
         Default value: 42.

    [32m/Arg7 [<Boolean>] (/Alias3)[0m


    [32m/Arg9 <Int32>[0m
         Must be between 0 and 100.

    [32m/Help [<Boolean>] (/?, /h)[0m
        Displays this help message.

    [32m/NotSwitch2 <Boolean>[0m
        NotSwitch2 description.

    [32m/Version [<Boolean>][0m
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedLongShortUsageColor = @"[36mUsage:[0m test [[--foo] <Int32>] [[--bar] <Int32>] [[--Arg2] <Int32>] [--Arg1 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

    [32m-f, --foo <Int32>[0m
            Foo description. Default value: 0.

    [32m    --bar <Int32>[0m
            Bar description. Default value: 0.

    [32m-a, --Arg2 <Int32> (-b, --baz)[0m
            Arg2 description.

    [32m    --Arg1 <Int32>[0m
            Arg1 description.

    [32m-?, --Help [<Boolean>] (-h)[0m
            Displays this help message.

    [32m-S, --Switch1 [<Boolean>][0m
            Switch1 description.

    [32m-k, --Switch2 [<Boolean>] (-x, --Switch2Alias)[0m
            Switch2 description.

    [32m-u [<Boolean>][0m
            Switch3 description.

    [32m    --Version [<Boolean>][0m
            Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageHidden = @"Usage: test [-Foo <Int32>] [-Help] [-Version]

    -Foo <Int32>


    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageValidators = @"Usage: test [[-arg2] <String>] [-Arg1 <Int32>] [-Arg3 <Int32>] [-Arg4 <String>...] [-Day <DayOfWeek>] [-Day2 <DayOfWeek>] [-Help] [-NotNull <Int32>] [-Version]

    -arg2 <String>
        Arg2 description. Must not be empty.

    -Arg1 <Int32>
        Arg1 description. Must be between 1 and 5.

    -Arg3 <Int32>
        Arg3 description. Must be between 1000 and 7000.

    -Arg4 <String>
        Arg4 description. Must be between 1 and 3 characters. Must have between 2 and 4 items.

    -Day <DayOfWeek>
        Day description. Possible values: Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday.

    -Day2 <DayOfWeek>
        Day2 description. Possible values: Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -NotNull <Int32>
        NotNull description.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageDependencies = @"Usage: test [-Address <IPAddress>] [-Help] [-Path <FileInfo>] [-Port <Int16>] [-Protocol <Int32>] [-Throughput <Int32>] [-Version]

You must use at least one of: -Address, -Path.

    -Address <IPAddress>
        The address.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Path <FileInfo>
        The path. Cannot be used with: -Address.

    -Port <Int16>
        The port. Must be used with: -Address. Default value: 5000.

    -Protocol <Int32>
        The protocol. Must be used with: -Address, -Throughput.

    -Throughput <Int32>
        The throughput.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageDependenciesDisabled = @"Usage: test [-Address <IPAddress>] [-Help] [-Path <FileInfo>] [-Port <Int16>] [-Protocol <Int32>] [-Throughput <Int32>] [-Version]

    -Address <IPAddress>
        The address.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Path <FileInfo>
        The path.

    -Port <Int16>
        The port. Default value: 5000.

    -Protocol <Int32>
        The protocol.

    -Throughput <Int32>
        The throughput.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageAlphabeticalLongName = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [[--Arg2] <Int32>] [--Arg1 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

        --Arg1 <Int32>
            Arg1 description.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

        --bar <Int32>
            Bar description. Default value: 0.

    -f, --foo <Int32>
            Foo description. Default value: 0.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -k, --Switch2 [<Boolean>] (-x, --Switch2Alias)
            Switch2 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageAlphabeticalLongNameDescending = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [[--Arg2] <Int32>] [--Arg1 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

        --Version [<Boolean>]
            Displays version information.

    -u [<Boolean>]
            Switch3 description.

    -k, --Switch2 [<Boolean>] (-x, --Switch2Alias)
            Switch2 description.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

        --Arg1 <Int32>
            Arg1 description.

".ReplaceLineEndings();

    private static readonly string _expectedUsageAlphabeticalShortName = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [[--Arg2] <Int32>] [--Arg1 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

        --Arg1 <Int32>
            Arg1 description.

        --bar <Int32>
            Bar description. Default value: 0.

    -f, --foo <Int32>
            Foo description. Default value: 0.

    -k, --Switch2 [<Boolean>] (-x, --Switch2Alias)
            Switch2 description.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -u [<Boolean>]
            Switch3 description.

        --Version [<Boolean>]
            Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageAlphabeticalShortNameDescending = @"Usage: test [[--foo] <Int32>] [[--bar] <Int32>] [[--Arg2] <Int32>] [--Arg1 <Int32>] [--Help] [--Switch1] [--Switch2] [-u] [--Version]

        --Version [<Boolean>]
            Displays version information.

    -u [<Boolean>]
            Switch3 description.

    -S, --Switch1 [<Boolean>]
            Switch1 description.

    -k, --Switch2 [<Boolean>] (-x, --Switch2Alias)
            Switch2 description.

    -f, --foo <Int32>
            Foo description. Default value: 0.

        --bar <Int32>
            Bar description. Default value: 0.

        --Arg1 <Int32>
            Arg1 description.

    -a, --Arg2 <Int32> (-b, --baz)
            Arg2 description.

    -?, --Help [<Boolean>] (-h)
            Displays this help message.

".ReplaceLineEndings();

    private static readonly string _expectedUsageAlphabetical = @"Usage: test [[-foo] <Int32>] [[-bar] <Int32>] [[-Arg2] <Int32>] [-Arg1 <Int32>] [-Help] [-Switch1] [-Switch2] [-Switch3] [-Version]

    -Arg1 <Int32>
        Arg1 description.

    -Arg2 <Int32> (-baz)
        Arg2 description.

    -bar <Int32>
        Bar description. Default value: 0.

    -foo <Int32>
        Foo description. Default value: 0.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Switch1 [<Boolean>]
        Switch1 description.

    -Switch2 [<Boolean>] (-Switch2Alias)
        Switch2 description.

    -Switch3 [<Boolean>]
        Switch3 description.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedUsageAlphabeticalDescending = @"Usage: test [[-foo] <Int32>] [[-bar] <Int32>] [[-Arg2] <Int32>] [-Arg1 <Int32>] [-Help] [-Switch1] [-Switch2] [-Switch3] [-Version]

    -Version [<Boolean>]
        Displays version information.

    -Switch3 [<Boolean>]
        Switch3 description.

    -Switch2 [<Boolean>] (-Switch2Alias)
        Switch2 description.

    -Switch1 [<Boolean>]
        Switch1 description.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -foo <Int32>
        Foo description. Default value: 0.

    -bar <Int32>
        Bar description. Default value: 0.

    -Arg2 <Int32> (-baz)
        Arg2 description.

    -Arg1 <Int32>
        Arg1 description.

".ReplaceLineEndings();

    private static readonly string _expectedUsageSyntaxOnly = @"Usage: test [/arg1] <String> [[/other] <Number>] [[/notSwitch] <Boolean>] [[/Arg5] <Single>] [[/other2] <Number>] [[/Arg8] <DayOfWeek>...] /Arg6 <String> [/Arg10...] [/Arg11] [/Arg12 <Int32>...] [/Arg13 <String=Int32>...] [/Arg14 <String=Int32>...] [/Arg15 <KeyValuePair<String, Int32>>] [/Arg3 <String>] [/Arg7] [/Arg9 <Int32>] [/Help] [/NotSwitch2 <Boolean>] [/Version]

Run 'test /Help' for more information.
".ReplaceLineEndings();

    private static readonly string _expectedUsageMessageOnly = @"Run 'test /Help' for more information.
".ReplaceLineEndings();

    private static readonly string _expectedUsageSeparator = @"Test arguments description.

Usage: test [/arg1:]<String> [[/other:]<Number>] [[/notSwitch:]<Boolean>] [[/Arg5:]<Single>] [[/other2:]<Number>] [[/Arg8:]<DayOfWeek>...] /Arg6:<String> [/Arg10...] [/Arg11] [/Arg12:<Int32>...] [/Arg13:<String=Int32>...] [/Arg14:<String=Int32>...] [/Arg15:<KeyValuePair<String, Int32>>] [/Arg3:<String>] [/Arg7] [/Arg9:<Int32>] [/Help] [/NotSwitch2:<Boolean>] [/Version]

    /arg1 <String>
        Arg1 description.

    /other <Number>
        Arg2 description. Default value: 42.

    /notSwitch <Boolean>
         Default value: False.

    /Arg5 <Single>
        Arg5 description.

    /other2 <Number>
        Arg4 description. Default value: 47.

    /Arg6 <String> (/Alias1, /Alias2)
        Arg6 description.

    /Arg12 <Int32>
         Default value: 42.

    /Arg7 [<Boolean>] (/Alias3)


    /Arg9 <Int32>
         Must be between 0 and 100.

    /Help [<Boolean>] (/?, /h)
        Displays this help message.

    /NotSwitch2 <Boolean>
        NotSwitch2 description.

    /Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedCustomIndentUsage = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-NotSwitch2 <Boolean>] [-Version]

  -arg1 <String>
    Arg1 description.

  -other <Number>
    Arg2 description. Default value: 42.

  -notSwitch <Boolean>
     Default value: False.

  -Arg5 <Single>
    Arg5 description.

  -other2 <Number>
    Arg4 description. Default value: 47.

  -Arg6 <String> (-Alias1, -Alias2)
    Arg6 description.

  -Arg12 <Int32>
     Default value: 42.

  -Arg7 [<Boolean>] (-Alias3)


  -Arg9 <Int32>
     Must be between 0 and 100.

  -Help [<Boolean>] (-?, -h)
    Displays this help message.

  -NotSwitch2 <Boolean>
    NotSwitch2 description.

  -Version [<Boolean>]
    Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedEmptyLineDefaultUsage = @"Usage: test [-Argument <String>] [-Help] [-Version]

    -Argument <String>
        A description with

a blank line.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

Some usage footer.

".ReplaceLineEndings();

    private static readonly string _expectedEmptyLineIndentAfterBlankLineUsage = @"Usage: test [-Argument <String>] [-Help] [-Version]

    -Argument <String>
        A description with

        a blank line.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

Some usage footer.

".ReplaceLineEndings();

    private static readonly string _expectedDefaultValueFormatUsage = @"Usage: test [-Argument <Double>] [-Argument2 <Double>] [-Help] [-Version]

    -Argument <Double>
        An argument. Default value: (1.50).

    -Argument2 <Double>
        Another argument. Default value: 3.5.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedDefaultValueFormatCultureUsage = @"Usage: test [-Argument <Double>] [-Argument2 <Double>] [-Help] [-Version]

    -Argument <Double>
        An argument. Default value: (1,50).

    -Argument2 <Double>
        Another argument. Default value: 3,5.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedFooterUsage = @"Test arguments description.

Usage: test [-arg1] <String> [[-other] <Number>] [[-notSwitch] <Boolean>] [[-Arg5] <Single>] [[-other2] <Number>] [[-Arg8] <DayOfWeek>...] -Arg6 <String> [-Arg10...] [-Arg11] [-Arg12 <Int32>...] [-Arg13 <String=Int32>...] [-Arg14 <String=Int32>...] [-Arg15 <KeyValuePair<String, Int32>>] [-Arg3 <String>] [-Arg7] [-Arg9 <Int32>] [-Help] [-NotSwitch2 <Boolean>] [-Version]

    -arg1 <String>
        Arg1 description.

    -other <Number>
        Arg2 description. Default value: 42.

    -notSwitch <Boolean>
         Default value: False.

    -Arg5 <Single>
        Arg5 description.

    -other2 <Number>
        Arg4 description. Default value: 47.

    -Arg6 <String> (-Alias1, -Alias2)
        Arg6 description.

    -Arg12 <Int32>
         Default value: 42.

    -Arg7 [<Boolean>] (-Alias3)


    -Arg9 <Int32>
         Must be between 0 and 100.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -NotSwitch2 <Boolean>
        NotSwitch2 description.

    -Version [<Boolean>]
        Displays version information.

This is a custom footer.
".ReplaceLineEndings();

    private static readonly string _expectedCategoryUsage = @"[36mUsage:[0m test [-ArgWithoutCategory <String>] [-Bar <String>] [-Baz <String>] [-Category3Arg <String>] [-Foo <String>] [-Help] [-Version]

    [32m-Help [<Boolean>] (-?, -h)[0m
        Displays this help message.

    [32m-Version [<Boolean>][0m
        Displays version information.

[36mThe first category.[0m

    [32m-Bar <String>[0m
        Bar description.

    [32m-Foo <String>[0m
        Foo description.

[36mThe second category.[0m

    [32m-Baz <String>[0m
        Baz description.

[36mCategory3[0m

    [32m-Category3Arg <String>[0m
        Category3Arg description.

".ReplaceLineEndings();

    private static readonly string _expectedDefaultCategoryUsage = @"Usage: test [-Baz <String>] [-Foo <String>] [-Help] [-Version]

The first category.

    -Foo <String>
        Foo description.

The second category.

    -Baz <String>
        Baz description.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Version [<Boolean>]
        Displays version information.

".ReplaceLineEndings();

    private static readonly string _expectedAutoPrefixUsage = @"The prefix could refer to one of the following arguments:
  [32m-Port[0m
  [32m-Prefix[0m
  [32m-Protocol[0m

Run 'test -Help' for more information.
".ReplaceLineEndings();

    private static readonly string _expectedAutoPrefixUsageLongShort = @"The prefix could refer to one of the following arguments:
  [32m--port[0m
  [32m--protocol[0m

Run 'test --help' for more information.
".ReplaceLineEndings();
}
