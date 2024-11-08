namespace Ookii.CommandLine.Tests;

partial class SubCommandTest
{
    private const string _executableName = "test";

    public static readonly string _expectedUsage = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

".ReplaceLineEndings();

    public static readonly string _expectedUsageNoVersion = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

".ReplaceLineEndings();

    public static readonly string _expectedUsageColor = @"[36mUsage:[0m test <command> [arguments]

The following commands are available:

    [32mAnotherSimpleCommand, alias[0m

    [32mcustom[0m
        Custom parsing command.

    [32mtest[0m
        Test command description.

    [32mversion[0m
        Displays version information.

".ReplaceLineEndings();

    public static readonly string _expectedUsageInstruction = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

Run 'test <command> -Help' for more information about a command.
".ReplaceLineEndings();

    public static readonly string _expectedUsageAutoInstruction = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    test
        Test command description.

    version
        Displays version information.

Run 'test <command> -Help' for more information about a command.
".ReplaceLineEndings();


    public static readonly string _expectedUsageWithDescription = @"Tests for Ookii.CommandLine.

Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

".ReplaceLineEndings();

    public static readonly string _expectedCommandUsage = @"Async command description.

Usage: test AsyncCommand [[-Value] <Int32>] [-Help]

    -Value <Int32>
        Argument description.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

".ReplaceLineEndings();

    public static readonly string _expectedParentCommandUsage = @"Parent command description.

Usage: test TestParentCommand <command> [arguments]

The following commands are available:

    NestedParentCommand
        Other parent command description.

    OtherTestChildCommand, TestChild2

    TestChildCommand

Run 'test TestParentCommand <command> -Help' for more information about a command.
".ReplaceLineEndings();

    public static readonly string _expectedNestedParentCommandUsage = @"Other parent command description.

Usage: test TestParentCommand NestedParentCommand <command> [arguments]

The following commands are available:

    NestedParentChildCommand

Run 'test TestParentCommand NestedParentCommand <command> -Help' for more information about a command.
".ReplaceLineEndings();

    public static readonly string _expectedNestedChildCommandUsage = @"Unknown argument name 'Foo'.

Usage: test TestParentCommand NestedParentCommand NestedParentChildCommand [-Help]

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

".ReplaceLineEndings();

    public static readonly string _expectedUsageFooter = @"Usage: test <command> [arguments]

The following commands are available:

    AnotherSimpleCommand, alias

    custom
        Custom parsing command.

    test
        Test command description.

    version
        Displays version information.

This is the command list footer.
".ReplaceLineEndings();

    public static readonly string _expectedUsageAmbiguousPrefix = @"The prefix could refer to one of the following commands:
  [32mtest[0m
  [32mTestAlias[0m
  [32mTestParentCommand[0m

Run 'test' without arguments for more information about available commands.
".ReplaceLineEndings();

    public static readonly string _expectedUsageAmbiguousPrefixNested = @"The prefix could refer to one of the following commands:
  [32mTestChildCommand[0m
  [32mTestChild2[0m

Run 'test TestParentCommand' without arguments for more information about available commands.
".ReplaceLineEndings();
}
