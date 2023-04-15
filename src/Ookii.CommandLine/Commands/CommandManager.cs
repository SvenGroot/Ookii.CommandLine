using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Provides functionality to find and instantiate subcommands.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Subcommands can be used to create applications that perform more than one operation,
    ///   where each operation has its own set of command line arguments. For example, think of
    ///   the <c>dotnet</c> executable, which has subcommands such as <c>dotnet build</c> and
    ///   <c>dotnet run</c>.
    /// </para>
    /// <para>
    ///   For a program using subcommands, typically the first command line argument will be the
    ///   name of the command, while the remaining arguments are arguments to the command. The
    ///   <see cref="CommandManager"/> class provides functionality that makes creating an
    ///   application like this easy.
    /// </para>
    /// <para>
    ///   A subcommand is created by creating a class that implements the <see cref="ICommand"/>
    ///   interface, and applying the <see cref="CommandAttribute"/> attribute to it. Implement
    ///   the <see cref="ICommand.Run"/> method to implement the command's functionality.
    /// </para>
    /// <para>
    ///   Subcommands classes are instantiated using the <see cref="CommandLineParser"/>, and follow
    ///   the same rules as command line arguments classes. They can define command line arguments
    ///   using the properties and constructor parameters, which will be the arguments for the
    ///   command.
    /// </para>
    /// <para>
    ///   Commands can be defined in a single assembly, or multiple assemblies.
    /// </para>
    /// <note>
    ///   If you reuse the same <see cref="CommandManager"/> instance or <see cref="CommandOptions"/>
    ///   instance to create multiple commands, the <see cref="ParseOptionsAttribute"/> of one
    ///   command may affect the behavior of another.
    /// </note>
    /// </remarks>
    /// <seealso cref="CommandLineParser"/>
    /// <seealso href="https://www.github.com/SvenGroot/ookii.commandline">Usage documentation</seealso>
    public class CommandManager
    {
        private readonly CommandProvider _provider;
        private readonly CommandOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandManager"/> class for the calling
        /// assembly.
        /// </summary>
        /// <param name="options">
        ///   The options to use for parsing and usage help, or <see langword="null"/> to use
        ///   the default options.
        /// </param>
        public CommandManager(CommandOptions? options = null)
            : this(Assembly.GetCallingAssembly(), options)
        {
        }

        public CommandManager(CommandProvider provider, CommandOptions? options = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _options = options ?? new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandManager"/> class.
        /// </summary>
        /// <param name="assembly">The assembly containing the commands.</param>
        /// <param name="options">
        ///   The options to use for parsing and usage help, or <see langword="null"/> to use
        ///   the default options.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="assembly"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <note>
        ///   Once a command is created, the <paramref name="options"/> instance may be modified
        ///   with the options of the <see cref="ParseOptionsAttribute"/> attribute applied to the
        ///   command class. Be aware of this if reusing the same <see cref="CommandManager"/> or
        ///   <see cref="CommandOptions"/> instance to create multiple commands.
        /// </note>
        /// </remarks>
        public CommandManager(Assembly assembly, CommandOptions? options = null)
            : this(new ReflectionCommandProvider(assembly ?? throw new ArgumentNullException(nameof(assembly))), options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandManager"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies containing the commands.</param>
        /// <param name="options">
        ///   The options to use for parsing and usage help, or <see langword="null"/> to use
        ///   the default options.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="assemblies"/> or one of its elements is <see langword="null"/>.
        /// </exception>
        public CommandManager(IEnumerable<Assembly> assemblies, CommandOptions? options = null)
            : this(new ReflectionCommandProvider(assemblies ?? throw new ArgumentNullException(nameof(assemblies))), options)
        {
        }

        /// <summary>
        /// Gets the options used by this instance.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="CommandOptions"/> class.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Modifying the options will change the way this instance behaves.
        /// </para>
        /// <note>
        ///   Once a command is created, the <see cref="ParseOptions"/> instance may be modified
        ///   with the options of the <see cref="ParseOptionsAttribute"/> attribute applied to the
        ///   command class. Be aware of this if reusing the same <see cref="CommandManager"/> or
        ///   <see cref="CommandOptions"/> instance to create multiple commands.
        /// </note>
        /// </remarks>
        public CommandOptions Options => _options;

        /// <summary>
        /// Gets the result of parsing the arguments for the last call to <see cref="CreateCommand()"/>.
        /// </summary>
        /// <value>
        /// The value of the <see cref="CommandLineParser.ParseResult"/> property after the call to the
        /// <see cref="CommandLineParser.ParseWithErrorHandling()"/> method made while creating
        /// the command.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If the <see cref="CommandLineParser.ParseWithErrorHandling()"/> was not invoked, for
        ///   example because the <see cref="CreateCommand()"/> method has not been called, no
        ///   command name was specified, an unknown command name was specified, or the command used
        ///   custom parsing, the value of the <see cref="ParseResult.Status"/> property will be
        ///   <see cref="ParseStatus.None"/>.
        /// </para>
        /// </remarks>
        public ParseResult ParseResult { get; private set; }

        /// <summary>
        /// Gets information about the commands.
        /// </summary>
        /// <returns>
        /// Information about every subcommand defined in the assemblies, ordered by command name.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter"/>
        ///   predicate are not returned.
        /// </para>
        /// <para>
        ///   The automatic version command is added if the <see cref="CommandOptions.AutoVersionCommand"/>
        ///   property is <see langword="true"/> and there is no command with a conflicting name.
        /// </para>
        /// </remarks>
        public IEnumerable<CommandInfo> GetCommands()
        {
            var commands = _provider.GetCommandsUnsorted(this);
            if (_options.AutoVersionCommand &&
                !commands.Any(c => _options.CommandNameComparer.Compare(c.Name, Properties.Resources.AutomaticVersionCommandName) == 0))
            {
                var versionCommand = CommandInfo.GetAutomaticVersionCommand(this);
                commands = commands.Append(versionCommand);
            }

            return commands.OrderBy(c => c.Name, _options.CommandNameComparer);
        }

        /// <summary>
        /// Gets the subcommand with the specified command name.
        /// </summary>
        /// <param name="commandName">The name of the subcommand.</param>
        /// <returns>
        ///   A <see cref="CommandInfo"/> instance for the specified subcommand, or <see langword="null"/>
        ///   if none could be found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="commandName"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   The command is located by searching all types in the assemblies for a command type
        ///   whose command name matches the specified name. If there are multiple commands with
        ///   the same name, the first matching one will be returned.
        /// </para>
        /// <para>
        ///   A command's name is taken from the <see cref="CommandAttribute.CommandName"/> property. If
        ///   that property is <see langword="null"/>, the name is determined by taking the command
        ///   type's name, and applying the transformation specified by the <see cref="CommandOptions.CommandNameTransform"/>
        ///   property.
        /// </para>
        /// <para>
        ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter"/>
        ///   predicate are not returned.
        /// </para>
        /// <para>
        ///   The automatic version command is returned if the <see cref="CommandOptions.AutoVersionCommand"/>
        ///   property is <see langword="true"/> and the <paramref name="commandName"/> matches the
        ///   name of the automatic version command, and not any other command name.
        /// </para>
        /// </remarks>
        public CommandInfo? GetCommand(string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException(nameof(commandName));
            }

            var command = _provider.GetCommand(commandName, this);
            if (command != null)
            {
                return command;
            }

            if (_options.AutoVersionCommand &&
                _options.CommandNameComparer.Compare(commandName, _options.AutoVersionCommandName()) == 0)
            {
                return CommandInfo.GetAutomaticVersionCommand(this);
            }

            return null;
        }

        /// <summary>
        /// Finds and instantiates the subcommand with the specified name, or if that fails, writes
        /// error and usage information.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <returns>
        ///   An instance a class implement the <see cref="ICommand"/> interface, or
        ///   <see langword="null"/> if the command was not found or an error occurred parsing the arguments.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   If the command could not be found, a list of possible commands is written using the
        ///   <see cref="ParseOptions.UsageWriter"/>. If an error occurs parsing the command's arguments,
        ///   the error message is written to <see cref="ParseOptions.Error"/>, and the
        ///   command's usage information is written to <see cref="ParseOptions.UsageWriter"/>.
        /// </para>
        /// <para>
        ///   If the <see cref="ParseOptions.Error"/> parameter is <see langword="null"/>, output is
        ///   written to a <see cref="LineWrappingTextWriter"/> for the standard error stream,
        ///   wrapping at the console's window width. If the stream is redirected, output may still
        ///   be wrapped, depending on the value returned by <see cref="Console.WindowWidth"/>.
        /// </para>
        /// <para>
        ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter"/>
        ///   predicate are not returned.
        /// </para>
        /// <para>
        ///   The automatic version command is returned if the <see cref="CommandOptions.AutoVersionCommand"/>
        ///   property is <see langword="true"/> and the command name matches the name of the
        ///   automatic version command, and not any other command name.
        /// </para>
        /// </remarks>
        public ICommand? CreateCommand(string? commandName, string[] args, int index)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (index < 0 || index > args.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            ParseResult = default;
            var commandInfo = commandName == null
                ? null
                : GetCommand(commandName);

            if (commandInfo is not CommandInfo info)
            {
                WriteUsage();
                return null;
            }

            _options.UsageWriter.CommandName = info.Name;
            try
            {
                var (command, result) = info.CreateInstanceWithResult(args, index);
                ParseResult = result;
                return command;
            }
            finally
            {
                _options.UsageWriter.CommandName = null;
            }
        }

        /// <inheritdoc cref="CreateCommand(string?, string[], int)"/>
        /// <summary>
        /// Finds and instantiates the subcommand with the name from the first argument, or if that
        /// fails, writes error and usage information.
        /// </summary>
        public ICommand? CreateCommand(string[] args, int index = 0)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (index < 0 || index > args.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            string? commandName = null;
            if (index < args.Length)
            {
                commandName = args[index];
                ++index;
            }

            return CreateCommand(commandName, args, index);
        }

        /// <summary>
        /// Finds and instantiates the subcommand using the arguments from <see cref="Environment.GetCommandLineArgs"/>,
        /// using the first argument for the command name. If that fails, writes error and usage information.
        /// </summary>
        /// <returns>
        /// <inheritdoc cref="CreateCommand(string?, string[], int)"/>
        /// </returns>
        /// <remarks>
        /// <inheritdoc cref="CreateCommand(string?, string[], int)"/>
        /// </remarks>
        public ICommand? CreateCommand()
        {
            // Skip the first argument, it's the application name.
            return CreateCommand(Environment.GetCommandLineArgs(), 1);
        }


        /// <summary>
        /// Finds and instantiates the subcommand with the specified name, and if it succeeds,
        /// runs it. If it fails, writes error and usage information.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <returns>
        ///   The value returned by <see cref="ICommand.Run"/>, or <see langword="null"/> if
        ///   the command could not be created.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        ///   This function creates the command by invoking the <see cref="CreateCommand(string?, string[], int)"/>,
        ///   method and then invokes the <see cref="ICommand.Run"/> method on the command.
        /// </para>
        /// </remarks>
        public int? RunCommand(string? commandName, string[] args, int index)
        {
            var command = CreateCommand(commandName, args, index);
            return command?.Run();
        }

        /// <inheritdoc cref="RunCommand(string?, string[], int)"/>
        /// <summary>
        /// Finds and instantiates the subcommand with the name from the first argument, and if it
        /// succeeds, runs it. If it fails, writes error and usage information.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This function creates the command by invoking the <see cref="CreateCommand(string[], int)"/>,
        ///   method and then invokes the <see cref="ICommand.Run"/> method on the command.
        /// </para>
        /// </remarks>
        public int? RunCommand(string[] args, int index = 0)
        {
            var command = CreateCommand(args, index);
            return command?.Run();
        }

        /// <summary>
        /// Finds and instantiates the subcommand using the arguments from the <see cref="Environment.GetCommandLineArgs"/>
        /// method, using the first argument as the command name. If it succeeds, runs the command.
        /// If it fails, writes error and usage information.
        /// </summary>
        /// <returns>
        /// <inheritdoc cref="RunCommand(string?, string[], int)"/>
        /// </returns>
        /// <remarks>
        /// <para>
        ///   This function creates the command by invoking the <see cref="CreateCommand()"/>,
        ///   method and then invokes the <see cref="ICommand.Run"/> method on the command.
        /// </para>
        /// </remarks>
        public int? RunCommand()
        {
            // Skip the first argument, it's the application name.
            return RunCommand(Environment.GetCommandLineArgs(), 1);
        }

        /// <inheritdoc cref="RunCommand(string?, string[], int)"/>
        /// <summary>
        /// Finds and instantiates the subcommand with the specified name, and if it succeeds,
        /// runs it asynchronously. If it fails, writes error and usage information.
        /// </summary>
        /// <returns>
        ///   A task representing the asynchronous run operation. The result is the value returned
        ///   by <see cref="IAsyncCommand.RunAsync"/>, or <see langword="null"/> if the command
        ///   could not be created.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   This function creates the command by invoking the <see cref="CreateCommand(string?, string[], int)"/>,
        ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
        ///   invokes the <see cref="IAsyncCommand.RunAsync"/> method; otherwise, it invokes the
        ///   <see cref="ICommand.Run"/> method on the command.
        /// </para>
        /// </remarks>
        public async Task<int?> RunCommandAsync(string? commandName, string[] args, int index)
        {
            var command = CreateCommand(commandName, args, index);
            if (command is IAsyncCommand asyncCommand)
            {
                return await asyncCommand.RunAsync();
            }

            return command?.Run();
        }

        /// <inheritdoc cref="RunCommandAsync(string?, string[], int)"/>
        /// <summary>
        /// Finds and instantiates the subcommand with the specified name, and if it succeeds,
        /// runs it asynchronously. If it fails, writes error and usage information.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This function creates the command by invoking the <see cref="CreateCommand(string[], int)"/>,
        ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
        ///   invokes the <see cref="IAsyncCommand.RunAsync"/> method; otherwise, it invokes the
        ///   <see cref="ICommand.Run"/> method on the command.
        /// </para>
        /// </remarks>
        public async Task<int?> RunCommandAsync(string[] args, int index = 0)
        {
            var command = CreateCommand(args, index);
            if (command is IAsyncCommand asyncCommand)
            {
                return await asyncCommand.RunAsync();
            }

            return command?.Run();
        }

        /// <inheritdoc cref="RunCommandAsync(string?, string[], int)"/>
        /// <summary>
        /// Finds and instantiates the subcommand using the arguments from the <see cref="Environment.GetCommandLineArgs"/>
        /// method, using the first argument as the command name. If it succeeds, runs the command
        /// asynchronously. If it fails, writes error and usage information.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This function creates the command by invoking the <see cref="CreateCommand()"/>,
        ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
        ///   invokes the <see cref="IAsyncCommand.RunAsync"/> method; otherwise, it invokes the
        ///   <see cref="ICommand.Run"/> method on the command.
        /// </para>
        /// </remarks>
        public async Task<int?> RunCommandAsync()
        {
            var command = CreateCommand();
            if (command is IAsyncCommand asyncCommand)
            {
                return await asyncCommand.RunAsync();
            }

            return command?.Run();
        }

        /// <summary>
        /// Writes usage help with a list of all the commands.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   This method writes usage help for the application, including a list of all shell
        ///   command names and their descriptions to <see cref="ParseOptions.UsageWriter"/>.
        /// </para>
        /// <para>
        ///   A command's name is retrieved from its <see cref="CommandAttribute"/> attribute,
        ///   and the description is retrieved from its <see cref="DescriptionAttribute"/> attribute.
        /// </para>
        /// </remarks>
        public void WriteUsage()
        {
            _options.UsageWriter.WriteCommandListUsage(this);
        }

        /// <summary>
        /// Gets a string with the usage help with a list of all the commands.
        /// </summary>
        /// <returns>A string containing the usage help.</returns>
        /// <remarks>
        /// <para>
        ///   A command's name is retrieved from its <see cref="CommandAttribute"/> attribute,
        ///   and the description is retrieved from its <see cref="DescriptionAttribute"/> attribute.
        /// </para>
        /// </remarks>
        public string GetUsage()
        {
            return _options.UsageWriter.GetCommandListUsage(this);
        }

        /// <summary>
        /// Gets the application description that will optionally be included in the usage help.
        /// </summary>
        /// <returns>
        /// The value of the <see cref="AssemblyDescriptionAttribute"/> for the first assembly
        /// used by this instance.
        /// </returns>
        public string? GetApplicationDescription() => _provider.GetApplicationDescription();
    }
}
