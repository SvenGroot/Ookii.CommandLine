// Copyright (c) Sven Groot (Ookii.org)
using Ookii.CommandLine.Terminal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Represents a command that can be invoked through a command line application that supports more than one operation.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Shell commands can be used to create shell utilities that perform more than one operation, where each operation has its own
    ///   set of command line arguments. For example, a utility might be used to modify or query different configuration parameters
    ///   of a system. Depending on whether it's a query or a modification, and which configuration parameter is used, the arguments
    ///   to such a utility might differ. Rather than provide different executables for each operation, it is often more convenient
    ///   to combine related operations in a single utility.
    /// </para>
    /// <para>
    ///   For a program using shell commands, typically the first command line argument will be the name of the operation and identifies which
    ///   shell command to use, while the remaining arguments are arguments to the command. The <see cref="ShellCommand"/> class aids
    ///   you in creating utilities that follow this pattern.
    /// </para>
    /// <para>
    ///   A shell command is created by deriving a type from the <see cref="ShellCommand"/> class, specifying the <see cref="ShellCommandAttribute"/>
    ///   on that type to specify the name of the command, and implementing the <see cref="ShellCommand.Run"/> method for that type.
    /// </para>
    /// <para>
    ///   An application can get a list of all shell commands defined in an assembly by using the <see cref="GetShellCommands"/> method, or
    ///   get a specific shell command using the <see cref="GetShellCommand"/> method. The <see cref="GetShellCommand"/> method searches
    ///   for a type that inherits from <see cref="ShellCommand"/> and defines the <see cref="ShellCommandAttribute"/> attribute, and
    ///   where the value of the <see cref="ShellCommandAttribute.CommandName"/> property matches the specified command name.
    ///   If a matching type is found, it returns the <see cref="Type"/> instance for that type.
    /// </para>
    /// <para>
    ///   Shell commands behave like regular command line arguments classes for the <see cref="CommandLineParser"/> class. Once
    ///   a shell command has been found using the <see cref="GetShellCommand"/> method, you can instantiate it by creating an
    ///   instance of the <see cref="CommandLineParser"/> class, passing the shell command's <see cref="Type"/> to the <see cref="CommandLineParser.CommandLineParser(Type, IEnumerable{string}?, IComparer{string}?)"/>
    ///   constructor. Then invoke the <see cref="CommandLineParser.Parse(string[],int)"/> method to parse the shell command's arguments (make sure to
    ///   set index so that the command does not try to parse the command name), and cast the result to a <see cref="ShellCommand"/> instance.
    ///   Then invoke the <see cref="ShellCommand.Run"/> method to invoke the command.
    /// </para>
    /// <para>
    ///   It is recommended to return the value of the <see cref="ShellCommand.ExitCode"/> property to the operating system
    ///   (by returning it from the Main method or by using the <see cref="Environment.ExitCode"/> property) after running the shell command.
    /// </para>
    /// <para>
    ///   Various utility methods to find, create and run shell commands are provided as <see langword="static"/> members of
    ///   the <see cref="ShellCommand"/> class.
    /// </para>
    /// </remarks>
    public abstract class ShellCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommand"/> class.
        /// </summary>
        protected ShellCommand()
        {
        }

        /// <summary>
        /// Gets or sets the exit code for the command.
        /// </summary>
        /// <value>
        /// The exit code for the command.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If your application doesn't process the exit code, it is recommended to return the value of
        ///   the <see cref="ExitCode"/> property to the operating system by returning it from the Main method
        ///   or setting the <see cref="Environment.ExitCode"/> property.
        /// </para>
        /// </remarks>
        public int ExitCode { get; protected set; }

        /// <summary>
        /// When implemented in a derived class, executes the command.
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Gets the <see cref="Type"/> instance for shell commands defined in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly whose types to search.</param>
        /// <param name="options">The options, or <see langword="null"/> to use the default options.</param>
        /// <returns>A list of types that inherit from <see cref="ShellCommand"/> and specify the <see cref="ShellCommandAttribute"/> attribute.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<ShellCommandInfo> GetShellCommands(Assembly assembly, CreateShellCommandOptions? options = null)
        {
            if( assembly == null )
                throw new ArgumentNullException(nameof(assembly));

            options ??= new CreateShellCommandOptions();
            var commands = GetShellCommandsUnsorted(assembly);
            if (options.AutoVersionCommand &&
                !commands.Any(c => options.CommandNameComparer.Compare(c.Name, Properties.Resources.AutomaticVersionCommandName) == 0))
            {
                var versionCommand = ShellCommandInfo.GetAutomaticVersionCommand(options.StringProvider);
                commands = commands.Append(versionCommand);
            }

            return commands.OrderBy(c => c.Name, options.CommandNameComparer);
        }

        /// <summary>
        /// Writes usage help with a list of all the shell commands in the specified assembly using
        /// the specified options.
        /// </summary>
        /// <param name="assembly">The assembly that contains the shell commands.</param>
        /// <param name="options">
        ///   The options used to write the command list, or <see langword="null"/> to use the
        ///   default options.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/>  is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   This method writes usage help for the application, including a list of all shell
        ///   command names and their descriptions to <see cref="ParseOptions.Out"/>.
        /// </para>
        /// <para>
        ///   A command's name is retrieved from its <see cref="ShellCommandAttribute"/> attribute,
        ///   and the description is retrieved from its <see cref="DescriptionAttribute"/> attribute.
        /// </para>
        /// </remarks>
        public static void WriteUsage(Assembly assembly, CreateShellCommandOptions? options = null)
        {
            if( assembly == null )
                throw new ArgumentNullException(nameof(assembly));

            options ??= new();

            using var writer = DisposableWrapper.Create(options.Out, LineWrappingTextWriter.ForConsoleOut);
            var lineWriter = writer.Inner as LineWrappingTextWriter;

            bool useColor = options.UsageOptions.UseColor ?? false;
            string usageColorStart = string.Empty;
            string colorEnd = string.Empty;
            if (useColor)
            {
                usageColorStart = options.UsageOptions.UsagePrefixColor;
                colorEnd = options.UsageOptions.ColorReset;
            }

            var executableName = options.UsageOptions.ExecutableName ?? 
                CommandLineParser.GetExecutableName(options.UsageOptions.IncludeExecutableExtension);

            writer.Inner.WriteLine(options.StringProvider.RootCommandUsageSyntax(executableName, usageColorStart, colorEnd));
            writer.Inner.WriteLine();
            writer.Inner.WriteLine(options.StringProvider.AvailableCommandsHeader(useColor));
            writer.Inner.WriteLine();
            if (lineWriter != null)
                lineWriter.Indent = (lineWriter.MaximumLineLength > 0 && lineWriter.MaximumLineLength < CommandLineParser.MaximumLineWidthForIndent) ? 0 : options.CommandDescriptionIndent;

            foreach (var command in GetShellCommands(assembly, options))
            {
                if (command.IsHidden)
                    continue;

                lineWriter?.ResetIndent();
                writer.Inner.WriteLine(options.StringProvider.CommandDescription(command, options));
            }
        }

        /// <summary>
        /// Determines whether the specified type is a shell command type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        ///   <see langword="true"/> if the specified type is a shell command type; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   A type is a shell command type if it is not an abstract type, inherits from the <see cref="ShellCommand"/> class, and has the
        ///   <see cref="ShellCommandAttribute"/> applied to it.
        /// </para>
        /// </remarks>
        public static bool IsShellCommand(Type type)
        {
            return GetShellCommandAttribute(type) != null;
        }

        /// <summary>
        /// Gets the name of the specified shell command.
        /// </summary>
        /// <param name="commandType">The <see cref="Type"/> of the shell command.</param>
        /// <returns>The shell command's name.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="commandType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="commandType"/> is not a shell command type.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The shell command's name is determined by the name specified in its <see cref="ShellCommandAttribute"/> attribute.
        /// </para>
        /// </remarks>
        public static string GetShellCommandName(Type commandType)
        {
            if( commandType == null )
                throw new ArgumentNullException(nameof(commandType));
            if( !IsShellCommand(commandType) )
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.TypeIsNotShellCommandFormat, commandType.FullName));

            return GetShellCommandName(commandType.GetCustomAttribute<ShellCommandAttribute>()!, commandType);
        }

        /// <summary>
        /// Gets the description of the specified shell command.
        /// </summary>
        /// <param name="commandType">The <see cref="Type"/> of the shell command.</param>
        /// <returns>The shell command's description, or <see langword="null"/> if it doesn't specify one.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="commandType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="commandType"/> is not a shell command type.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   A shell command's description if specified using the <see cref="DescriptionAttribute"/> attribute, or
        ///   <see langword="null"/> if none is specified.
        /// </para>
        /// </remarks>
        public static string? GetShellCommandDescription(Type commandType)
        {
            if( commandType == null )
                throw new ArgumentNullException(nameof(commandType));
            if( !IsShellCommand(commandType) )
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.TypeIsNotShellCommandFormat, commandType.FullName));

            return commandType.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        /// <summary>
        /// Gets the shell command with the specified command name, using the specified <see cref="IEqualityComparer{T}"/> to compare command names.
        /// </summary>
        /// <param name="assembly">The assembly whose types to search.</param>
        /// <param name="commandName">The command name of the shell command.</param>
        /// <param name="options">The options, or <see langword="null"/> to use the default options.</param>
        /// <returns>The <see cref="Type"/> of the specified shell command, or <see langword="null"/> if none could be found.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/> or <paramref name="commandName"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// </remarks>
        public static ShellCommandInfo? GetShellCommand(Assembly assembly, string commandName, CreateShellCommandOptions? options = null)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (commandName == null)
                throw new ArgumentNullException(nameof(commandName));

            options ??= new CreateShellCommandOptions();
            var command = GetShellCommandsUnsorted(assembly)
                .Where(c => options.CommandNameComparer.Compare(c.Name, commandName) == 0)
                .Cast<ShellCommandInfo?>()
                .FirstOrDefault();

            if (command == null && options.AutoVersionCommand &&
                options.CommandNameComparer.Compare(commandName, Properties.Resources.AutomaticVersionCommandName) == 0)
            {
                command = ShellCommandInfo.GetAutomaticVersionCommand(options.StringProvider);
            }

            return command;
        }

        /// <summary>
        /// Finds and instantiates the shell command with the specified name, or if that fails, writes error and usage information to the specified writers.
        /// </summary>
        /// <param name="assembly">The assembly to search for the shell command.</param>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="args">The arguments to the shell command.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>An instance a class deriving from <see cref="ShellCommand"/>, or <see langword="null"/> if the command was not found or an error occurred parsing the arguments.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/>, <paramref name="args"/>, or <paramref name="options"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.</exception>
        /// <remarks>
        /// <para>
        ///   If the command could not be found, a list of possible commands is written to <see cref="ParseOptions.Out"/>. If an error occurs parsing the command's arguments, the error
        ///   message is written to <see cref="ParseOptions.Error"/>, and the shell command's usage information is written to <see cref="ParseOptions.Out"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property or <see cref="ParseOptions.Error"/>
        ///   property is <see langword="null"/>, output is written to a <see cref="LineWrappingTextWriter"/>
        ///   for the standard output and error streams respectively, wrapping at the console's
        ///   window width. When the console output is redirected to a file, Microsoft .Net will
        ///   still report the console's actual window width, but on Mono the value of the
        ///   <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage
        ///   information will not be wrapped.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property is instance of the
        ///   <see cref="LineWrappingTextWriter"/> class, this method indents additional lines for
        ///   the usage syntax and argument descriptions according to the values specified by the
        ///   <see cref="CreateShellCommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
        /// </para>
        /// </remarks>
        public static ShellCommand? CreateShellCommand(Assembly assembly, string? commandName, string[] args, int index, CreateShellCommandOptions? options = null)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (index < 0 || index > args.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            options ??= new();
            using var vtSupport = options.EnableOutputColor();
            using var output = DisposableWrapper.Create(options.Out, LineWrappingTextWriter.ForConsoleOut);

            // Update the values because the options are passed to the shell command and the ParseInternal method.
            var originalOut = options.Out;
            options.Out = output.Inner;

            try
            {
                var commandInfo = commandName == null 
                    ? null 
                    : GetShellCommand(assembly, commandName, options);

                if (commandInfo == null)
                {
                    WriteUsage(assembly, options);
                    return null;
                }

                options.UsageOptions.CommandName = commandInfo.Value.Name;
                return commandInfo.Value.CreateInstance(args, index, options);
            }
            finally
            {
                options.Out = originalOut;
                options.UsageOptions.CommandName = null;
            }
        }

        /// <summary>
        /// Finds and instantiates the shell command from the specified arguments, or if that fails, writes error and usage information to the specified writers.
        /// </summary>
        /// <param name="assembly">The assembly to search for the shell command.</param>
        /// <param name="args">The arguments to the shell command, with the shell command name at the position specified by <paramref name="index"/>.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>An instance a class deriving from <see cref="ShellCommand"/>, or <see langword="null"/> if the command was not found or an error occurred parsing the arguments.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/>, <paramref name="args"/>, or <paramref name="options"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.</exception>
        /// <remarks>
        /// <para>
        ///   If the command could not be found, a list of possible commands is written to <see cref="ParseOptions.Out"/>. If an error occurs parsing the command's arguments, the error
        ///   message is written to <see cref="ParseOptions.Error"/>, and the shell command's usage information is written to <see cref="ParseOptions.Out"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property or <see cref="ParseOptions.Error"/>
        ///   property is <see langword="null"/>, output is written to a <see cref="LineWrappingTextWriter"/>
        ///   for the standard output and error streams respectively, wrapping at the console's
        ///   window width. When the console output is redirected to a file, Microsoft .Net will
        ///   still report the console's actual window width, but on Mono the value of the
        ///   <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage
        ///   information will not be wrapped.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property is instance of the
        ///   <see cref="LineWrappingTextWriter"/> class, this method indents additional lines for
        ///   the usage syntax and argument descriptions according to the values specified by the
        ///   <see cref="CreateShellCommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
        /// </para>
        /// </remarks>
        public static ShellCommand? CreateShellCommand(Assembly assembly, string[] args, int index, CreateShellCommandOptions? options = null)
        {
            if( assembly == null )
                throw new ArgumentNullException(nameof(assembly));
            if( args == null )
                throw new ArgumentNullException(nameof(args));
            if( index < 0 || index > args.Length )
                throw new ArgumentOutOfRangeException(nameof(index));

            return CreateShellCommand(assembly, index >= args.Length ? null : args[index], args, index == args.Length ? index : index + 1, options);
        }

        /// <summary>
        /// Runs a shell command with the specified arguments; if the command name or arguments are invalid, prints error and usage information.
        /// </summary>
        /// <param name="assembly">The assembly to search for the shell command.</param>
        /// <param name="args">The arguments to the shell command, with the shell command name at the position specified by <paramref name="index"/>.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>The value of the <see cref="ShellCommand.ExitCode"/> property after the command finishes running, or -1 if the command could not be created.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/> or <paramref name="args"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.</exception>
        /// <remarks>
        /// <para>
        ///   If the command could not be found, a list of possible commands is written to <see cref="ParseOptions.Out"/>. If an error occurs parsing the command's arguments, the error
        ///   message is written to <see cref="ParseOptions.Error"/>, and the shell command's usage information is written to <see cref="ParseOptions.Out"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property or <see cref="ParseOptions.Error"/>
        ///   property is <see langword="null"/>, output is written to a <see cref="LineWrappingTextWriter"/>
        ///   for the standard output and error streams respectively, wrapping at the console's
        ///   window width. When the console output is redirected to a file, Microsoft .Net will
        ///   still report the console's actual window width, but on Mono the value of the
        ///   <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage
        ///   information will not be wrapped.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property is instance of the
        ///   <see cref="LineWrappingTextWriter"/> class, this method indents additional lines for
        ///   the usage syntax and argument descriptions according to the values specified by the
        ///   <see cref="CreateShellCommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
        /// </para>
        /// </remarks>
        public static int RunShellCommand(Assembly assembly, string[] args, int index, CreateShellCommandOptions? options = null)
        {
            var command = CreateShellCommand(assembly, args, index, options);
            if( command != null )
            {
                command.Run();
                return command.ExitCode;
            }
            else
                return -1;
        }

        /// <summary>
        /// Runs the specified shell command with the specified arguments; if the command name or arguments are invalid, prints error and usage information.
        /// </summary>
        /// <param name="assembly">The assembly to search for the shell command.</param>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="args">The arguments to the shell command.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>The value of the <see cref="ShellCommand.ExitCode"/> property after the command finishes running, or -1 if the command could not be created.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="assembly"/> or <paramref name="args"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.</exception>
        /// <remarks>
        /// <para>
        ///   If the command could not be found, a list of possible commands is written to <see cref="ParseOptions.Out"/>. If an error occurs parsing the command's arguments, the error
        ///   message is written to <see cref="ParseOptions.Error"/>, and the shell command's usage information is written to <see cref="ParseOptions.Out"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property or <see cref="ParseOptions.Error"/>
        ///   property is <see langword="null"/>, output is written to a <see cref="LineWrappingTextWriter"/>
        ///   for the standard output and error streams respectively, wrapping at the console's
        ///   window width. When the console output is redirected to a file, Microsoft .Net will
        ///   still report the console's actual window width, but on Mono the value of the
        ///   <see cref="Console.WindowWidth"/> property will be 0. In that case, the usage
        ///   information will not be wrapped.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Out"/> property is instance of the
        ///   <see cref="LineWrappingTextWriter"/> class, this method indents additional lines for
        ///   the usage syntax and argument descriptions according to the values specified by the
        ///   <see cref="CreateShellCommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
        /// </para>
        /// </remarks>
        public static int RunShellCommand(Assembly assembly, string? commandName, string[] args, int index, CreateShellCommandOptions? options = null)
        {
            var command = CreateShellCommand(assembly, commandName, args, index, options);
            if( command != null )
            {
                command.Run();
                return command.ExitCode;
            }
            else
                return -1;
        }

        internal static string GetShellCommandName(ShellCommandAttribute attribute, Type commandType)
        {
            return attribute.CommandName ?? commandType.Name;
        }

        internal static ShellCommandAttribute? GetShellCommandAttribute(Type type)
        {
            if( type == null )
                throw new ArgumentNullException(nameof(type));

            if (type.IsAbstract || !type.IsSubclassOf(typeof(ShellCommand)))
                return null;

            return type.GetCustomAttribute<ShellCommandAttribute>();
        }

        private static IEnumerable<ShellCommandInfo> GetShellCommandsUnsorted(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var commands = assembly.GetTypes()
                .Where(t => IsShellCommand(t))
                .Select(t => new ShellCommandInfo(t));

            return commands;
        }
    }
}
