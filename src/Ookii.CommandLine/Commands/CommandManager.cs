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
    /// </remarks>
    /// <seealso cref="CommandLineParser"/>
    /// <seealso href="https://www.github.com/SvenGroot/ookii.commandline"/>
    public class CommandManager
    {
        private readonly Assembly? _assembly;
        private readonly IEnumerable<Assembly>? _assemblies;
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
        public CommandManager(Assembly assembly, CommandOptions? options = null)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _options = options ?? new();
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
        {
            _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            _options = options ?? new();

            if (_assemblies.Any(a => a == null))
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
        }

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
            var commands = GetCommandsUnsorted();
            if (_options.AutoVersionCommand &&
                !commands.Any(c => _options.CommandNameComparer.Compare(c.Name, Properties.Resources.AutomaticVersionCommandName) == 0))
            {
                var versionCommand = CommandInfo.GetAutomaticVersionCommand(_options);
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

            var commands = GetCommandsUnsorted()
                .Where(c => c.MatchesName(commandName, _options.CommandNameComparer));

            if (commands.Any())
            {
                return commands.First();
            }

            if (_options.AutoVersionCommand &&
                _options.CommandNameComparer.Compare(commandName, _options.AutoVersionCommandName()) == 0)
            {
                return CommandInfo.GetAutomaticVersionCommand(_options);
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
        ///   If the command could not be found, a list of possible commands is written to 
        ///   <see cref="ParseOptions.Out"/>. If an error occurs parsing the command's arguments,
        ///   the error message is written to <see cref="ParseOptions.Error"/>, and the shell
        ///   command's usage information is written to <see cref="ParseOptions.Out"/>.
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
        ///   <see cref="CommandOptions"/>, unless the <see cref="LineWrappingTextWriter.MaximumLineLength"/>
        ///   property is less than 30.
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

            using var restorer = _options.EnableOutputColor() ?? new OptionsRestorer(_options, null);
            using var output = DisposableWrapper.Create(_options.Out, LineWrappingTextWriter.ForConsoleOut);

            // Update the values because the options are passed to the command and the
            // ParseInternal method.
            if (_options.Out == null)
            {
                _options.Out = output.Inner;
                restorer.ResetOut = true;
            }

            restorer.ResetCommandName = true;

            var commandInfo = commandName == null
                ? null
                : GetCommand(commandName);

            if (commandInfo == null)
            {
                WriteUsage();
                return null;
            }

            _options.UsageOptions.CommandName = commandInfo.Value.Name;
            return commandInfo.Value.CreateInstance(args, index, _options);
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

        /// <inheritdoc cref="RunCommand(string?, string[], int)"/>
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
        ///   command names and their descriptions to <see cref="ParseOptions.Out"/>.
        /// </para>
        /// <para>
        ///   A command's name is retrieved from its <see cref="CommandAttribute"/> attribute,
        ///   and the description is retrieved from its <see cref="DescriptionAttribute"/> attribute.
        /// </para>
        /// </remarks>
        public void WriteUsage()
        {
            using var restorer = _options.EnableOutputColor();
            using var writer = DisposableWrapper.Create(_options.Out, LineWrappingTextWriter.ForConsoleOut);
            var lineWriter = writer.Inner as LineWrappingTextWriter;

            var useColor = _options.UsageOptions.UseColor ?? false;
            string usageColorStart = string.Empty;
            string colorEnd = string.Empty;
            if (useColor)
            {
                usageColorStart = _options.UsageOptions.UsagePrefixColor;
                colorEnd = _options.UsageOptions.ColorReset;
            }

            if (_options.IncludeApplicationDescriptionBeforeCommandList)
            {
                var description = GetDescription();
                if (description != null)
                {
                    if (lineWriter != null && CommandLineParser.ShouldIndent(lineWriter))
                    {
                        lineWriter.Indent = _options.UsageOptions.ApplicationDescriptionIndent;
                    }

                    writer.Inner.WriteLine(_options.StringProvider.CommandListApplicationDescription(description, useColor));
                    writer.Inner.WriteLine();
                }
            }

            var executableName = _options.UsageOptions.GetExecutableName();
            if (lineWriter != null && CommandLineParser.ShouldIndent(lineWriter))
            {
                lineWriter.Indent = _options.UsageOptions.SyntaxIndent;
            }

            writer.Inner.WriteLine(_options.StringProvider.RootCommandUsageSyntax(executableName, usageColorStart, colorEnd));
            writer.Inner.WriteLine();
            writer.Inner.WriteLine(_options.StringProvider.AvailableCommandsHeader(useColor));
            writer.Inner.WriteLine();
            if (lineWriter != null && CommandLineParser.ShouldIndent(lineWriter))
            {
                lineWriter.Indent = _options.CommandDescriptionIndent;
            }

            foreach (var command in GetCommands())
            {
                if (command.IsHidden)
                {
                    continue;
                }

                lineWriter?.ResetIndent();
                writer.Inner.WriteLine(_options.StringProvider.CommandDescription(command, _options));
            }

            if (_options.ShowCommandHelpInstruction)
            {
                if (lineWriter != null)
                {
                    lineWriter.Indent = 0;
                }

                var prefix = _options.Mode == ParsingMode.LongShort
                    ? (_options.LongArgumentNamePrefix ?? CommandLineParser.DefaultLongArgumentNamePrefix)
                    : (_options.ArgumentNamePrefixes?.FirstOrDefault() ?? CommandLineParser.GetDefaultArgumentNamePrefixes()[0]);

                var transform = _options.NameTransform ?? NameTransform.None;
                writer.Inner.WriteLine(transform.Apply(_options.StringProvider.CommandHelpInstruction(executableName, prefix, useColor)));
            }
        }

        private string? GetDescription() 
            => (_assembly ?? _assemblies?.FirstOrDefault())?.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

        // Return value does not include the automatic version command.
        private IEnumerable<CommandInfo> GetCommandsUnsorted()
        {
            IEnumerable<Type> types;
            if (_assembly != null)
            {
                types = _assembly.GetTypes();
            }
            else
            {
                Debug.Assert(_assemblies != null);
                types = _assemblies.SelectMany(a => a.GetTypes());
            }

            return from type in types
                   let info = CommandInfo.TryCreate(type, _options)
                   where info != null && (_options.CommandFilter?.Invoke(info.Value) ?? true)
                   select info.Value;
        }
    }
}
