# Nested commands sample

This sample demonstrates how to use the [`ParentCommandAttribute`][] attribute and the [`ParentCommand`][]
class to build an application that has commands with nested subcommands. Commands with the
[`ParentCommandAttribute`][] are nested under the specified command, and commands without this attribute
are top-level commands.

Commands that have children derive from the [`ParentCommand`][]  class. This class will use your
[`CommandManager`][], but sets the [`CommandOptions.ParentCommand`][] property to filter only the
children of that command. The remaining arguments are passed to the nested subcommand.

Child commands are just regular commands using the [`CommandLineParser`][], and don't need to do
anything special except to add the [`ParentCommandAttribute`][] attribute to specify which command is
their parent. For an example, see [CourseCommands.cs](CourseCommands.cs).

This sample creates a simple "database" application that lets you add and remove students and
courses to a json file. It has top-level commands `student` and `course`, which both have child
commands `add` and `remove` (and a few others).

All the leaf commands use a common base class, so they can specify the path to the json file. This
is the primary way you add common arguments to multiple commands in Ookii.CommandLine (for an
alternative, see the [top-level arguments sample](../TopLevelArguments)).

When invoked without arguments, we see only the top-level commands:

```text
Nested subcommands sample for Ookii.CommandLine.

Usage: NestedCommands <command> [arguments]

The following commands are available:

    course
        Add or remove a course.

    list
        Lists all students and courses.

    student
        Add or remove a student.

    version
        Displays version information.

Run 'NestedCommands <command> -Help' for more information about a command.
```

This is completely ordinary help for any application with subcommands.

Now, if we run `./NestedCommands student`, we see the following:

```text
Add or remove a student.

Usage: NestedCommands student <command> [arguments]

The following commands are available:

    add
        Adds a student to the database.

    add-course
        Adds a course for a student.

    remove
        Removes a student from the database.

Run 'NestedCommands student <command> -Help' for more information about a command.
```

You can see the parent command will:

- Show the command description at the top, rather than the application description.
- Include the top-level command name in the usage syntax.
- Show only its child commands (which also excludes the `version` command).

If we run `./NestedCommand student -Help`, we get the same output. While the `student` command
doesn't have a help argument (since the [`ParentCommand`][] uses [`ICommandWithCustomParsing`][],
and not the [`CommandLineParser`][]), there is no command named `-Help` so it still just shows the
command list.

If we run `./NestedCommand student add -Help`, we get the help for the command's arguments as
usual:

```text
Adds a student to the database.

Usage: NestedCommands student add [-FirstName] <String> [-LastName] <String> [[-Major] <String>]
   [-Help] [-Path <String>]

    -FirstName <String>
        The first name of the student. Must not be blank.

    -LastName <String>
        The last name of the student. Must not be blank.

    -Major <String>
        The student's major.

    -Help [<Boolean>] (-?, -h)
        Displays this help message.

    -Path <String>
        The json file holding the data. Default value: data.json.
```

The usage syntax shows both command names before the arguments.

[`CommandLineParser`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`CommandOptions.ParentCommand`]: https://www.ookii.org/docs/commandline-4.1/html/P_Ookii_CommandLine_Commands_CommandOptions_ParentCommand.htm
[`ICommandWithCustomParsing`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Commands_ICommandWithCustomParsing.htm
[`ParentCommand`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Commands_ParentCommand.htm
[`ParentCommandAttribute`]: https://www.ookii.org/docs/commandline-4.1/html/T_Ookii_CommandLine_Commands_ParentCommandAttribute.htm
