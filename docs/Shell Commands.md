# Shell commands

Ookii.CommandLine provides the ability to create an application that has more than one function by using shell commands.

Shell commands can be used to create shell utilities that perform more than one operation, where each operation has its own set of command line arguments. For example, a utility might be used to modify or query different configuration parameters of a system. Depending on whether it's a query or a modification, and which configuration parameter is used, the arguments to such a utility might differ. Rather than provide different executables for each operation, it is often more convenient to combine related operations in a single utility.

Think for example of the Windows {{net}} command, which provides many different operations through the same command. Shell commands allow you to easily create such an application.

For a program using shell commands, typically the first command line argument will be the name of the operation and identifies which shell command to use, while the remaining arguments are arguments to the command. The {{ShellCommand}} class aids you in creating utilities that follow this pattern. 

## Defining shell commands

A shell command is created by deriving a type from the {{ShellCommand}} class, specifying the {{ShellCommandAttribute}} on that type to specify the name of the command, and implementing the {{ShellCommand.Run}} method for that type.

The class inheriting the {{ShellCommand}} class defines the arguments for the command. This class will be used with the {{CommandLineParser}} class to parse the arguments for the command.

## Using shell commands

To use a shell command, you must first determine the shell command the user wishes to invoke, typically by inspecting the first element of the array of arguments passed to the {{Main}} method of your application.

You can then get the {{Type}} instance of the shell command’s class by calling the {{ShellCommand.GetShellCommand}} method. This method searches the specified assembly for a type that inherits from the {{ShellCommand}} class, and has the {{ShellCommandAttribute}} attribute applied with the {{ShellCommandAttribute.Name}} property set to the specified name. You can also get a list of all shell commands in an assembly by using the {{ShellCommand.GetShellCommands}} method.

This {{Type}} instance can be passed to the constructor of the {{CommandLineParser}} class, after which you can parse arguments for the command as usual (make sure to pass an index so that the command name is not treated as an argument), and finally invoke its {{ShellCommand.Run}} method.

The {{ShellCommand}} class provides static utility methods that perform these tasks for you. The {{ShellCommand.CreateShellCommand}} method finds and creates a shell command, and writes error and usage information to the output if it failed. If no command name was specified, or the specified command name could not be found, it writes a list of all shell commands in the assembly and their descriptions to the output. If the command was found but parsing its arguments failed, it writes usage information for that command to the output.

The {{ShellCommand.RunShellCommand}} method works the same as the {{ShellCommand.CreateShellCommand}} method, but also invokes the {{ShellCommand.Run}} method if the command was successfully created.

It is recommended to return the value of the {{ShellCommand.ExitCode}} property to the operating system (by returning it from the {{Main}} method or by using the {{Environment.}}{{ExitCode}} property) after running the shell command. 

The source code of a full sample application that defines two commands is included with the Ookii.CommandLine library.
