# Nested commands sample

While Ookii.CommandLine has no built-in way to nest subcommands, such functionality is easy to
implement using the [`CommandOptions.CommandFilter`][] property. All you need is a way to
distinguish top-level commands and child commands.

This sample demonstrates one way to do this. It defines a [`ParentCommandAttribute`](ParentCommandAttribute.cs)
that can be used to specify which command is the parent of a command, and commands without this
attribute are top-level commands.

Commands that have children use the [`ICommandWithCustomParsing`][] interface so they can do their
own parsing, rather than relying on the [`CommandLineParser`][]. This allows them to create a new
[`CommandManager`][] that filters only the children of that command, and passes the remaining
arguments to that. Check the [ParentCommand.cs](ParentCommand.cs) file to see how this works.

Child commands are just regular commands using the [`CommandLineParser`][], and don't need to do
anything special except to add the `ParentCommandAttribute` attribute to specify which command is
their parent. For an example, see [CourseCommands.cs](CourseCommands.cs).

This sample uses this framework to create a simple "database" application that lets your add and
remove students and courses to a json file. It has top-level commands `student` and `course`, which
both have child commands `add` and `remove` (and a few others).

All the leaf commands use a common base class, so they can specify the path to the json file. This
is the way you add common arguments to multiple commands in Ookii.CommandLine.

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

The 'student' command has the following subcommands:

    add
        Adds a student to the database.

    add-course
        Adds a course for a student.

    remove
        Removes a student from the database.

Run 'NestedCommands student <command> -Help' for more information about a command.
```

You can see the sample has customized the parent command to:

- Show the command description at the top, rather than the application description.
- Include the top-level command name in the usage syntax.
- Change the header above the commands to indicate these are nested subcommands.
- Remove the a `version` command (nested version commands would kind of redundant).

This was done by changing the [`CommandOptions`][] and using a simple custom
[`LocalizedStringProvider`][] derived class (see [CustomStringProvider.cs](CustomStringProvider.cs)).

If we run `./NestedCommand student -Help`, we get the same output. While the `student` command
doesn't have a help argument (since it uses custom parsing, and not the [`CommandLineParser`][]),
there is no command named `-Help` so it still just shows the command list.

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

We can see the usage syntax correctly shows both command names before the arguments.

[`CommandLineParser`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_CommandLineParser.htm
[`CommandManager`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_Commands_CommandManager.htm
[`CommandOptions.CommandFilter`]: https://www.ookii.org/docs/commandline-3.0-preview/html/P_Ookii_CommandLine_Commands_CommandOptions_CommandFilter.htm
[`CommandOptions`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_Commands_CommandOptions.htm
[`ICommandWithCustomParsing`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_Commands_ICommandWithCustomParsing.htm
[`LocalizedStringProvider`]: https://www.ookii.org/docs/commandline-3.0-preview/html/T_Ookii_CommandLine_LocalizedStringProvider.htm
