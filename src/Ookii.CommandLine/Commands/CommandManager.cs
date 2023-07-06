using Ookii.CommandLine.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

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
///   the <see cref="ICommand.Run" qualifyHint="true"/> method to implement the command's functionality.
/// </para>
/// <para>
///   Subcommand classes are instantiated using the <see cref="CommandLineParser"/> class, and
///   follow the same rules as command line arguments classes, unless they implement the
///   <see cref="ICommandWithCustomParsing"/> interface.
/// </para>
/// <para>
///   Commands can be defined in a single assembly, or in multiple assemblies.
/// </para>
/// <note>
///   If you reuse the same <see cref="CommandManager"/> instance or <see cref="CommandOptions"/>
///   instance to create multiple commands, the <see cref="ParseOptionsAttribute"/> of one
///   command may affect the behavior of another.
/// </note>
/// </remarks>
/// <seealso cref="CommandLineParser"/>
/// <seealso href="https://www.github.com/SvenGroot/ookii.commandline">Usage documentation</seealso>
/// <threadsafety static="true" instance="false"/>
public class CommandManager
{
    private readonly CommandProvider _provider;
    private readonly CommandOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandManager"/> class for the assembly that
    /// is calling the constructor.
    /// </summary>
    /// <param name="options">
    ///   The options to use for parsing and usage help, or <see langword="null"/> to use
    ///   the default options.
    /// </param>
    /// <remarks>
    /// <para>
    ///   The <see cref="CommandManager"/> class will look in the calling assembly for any public
    ///   or internal classes that implement the <see cref="ICommand"/> interface, have the
    ///   <see cref="CommandAttribute"/> attribute, and are not <see langword="abstract"/>.
    /// </para>
    /// <para>
    ///   This constructor uses reflection to determine which commands are available at runtime. To
    ///   use source generation to locate commands at compile time, use the <see cref="GeneratedCommandManagerAttribute"/>
    ///   attribute.
    /// </para>
    /// <note>
    ///   Once a command is created, the <paramref name="options"/> instance may be modified
    ///   with the options of the <see cref="ParseOptionsAttribute"/> attribute applied to the
    ///   command class. Be aware of this if reusing the same <see cref="CommandManager"/> or
    ///   <see cref="CommandOptions"/> instance to create multiple commands.
    /// </note>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
    public CommandManager(CommandOptions? options = null)
        : this(new ReflectionCommandProvider(Assembly.GetCallingAssembly(), Assembly.GetCallingAssembly()), options)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CommandManager"/> class using the
    /// specified <see cref="CommandProvider"/>.
    /// </summary>
    /// <param name="provider">
    /// The <see cref="CommandProvider"/> that determines which commands are available.
    /// </param>
    /// <param name="options">
    ///   The options to use for parsing and usage help, or <see langword="null"/> to use
    ///   the default options.
    /// </param>
    /// <remarks>
    /// <para>
    ///   This constructor supports source generation, and should not typically be used directly
    ///   by application code.
    /// </para>
    /// <note>
    ///   Once a command is created, the <paramref name="options"/> instance may be modified
    ///   with the options of the <see cref="ParseOptionsAttribute"/> attribute applied to the
    ///   command class. Be aware of this if reusing the same <see cref="CommandManager"/> or
    ///   <see cref="CommandOptions"/> instance to create multiple commands.
    /// </note>
    /// </remarks>
    /// <seealso cref="GeneratedCommandManagerAttribute"/>
    protected CommandManager(CommandProvider provider, CommandOptions? options = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _options = options ?? new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandManager"/> class using the specified
    /// assembly.
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
    /// <para>
    ///   The <see cref="CommandManager"/> class will look in the specified assembly for any public
    ///   classes that implement the <see cref="ICommand"/> interface, have the
    ///   <see cref="CommandAttribute"/> attribute, and are not <see langword="abstract"/>.
    /// </para>
    /// <para>
    ///   If <paramref name="assembly"/> is the assembly that called this constructor, both public
    ///   and internal command classes will be used. Otherwise, only public command classes are
    ///   used.
    /// </para>
    /// <para>
    ///   This constructor uses reflection to determine which commands are available at runtime. To
    ///   use source generation to locate commands at compile time, use the <see cref="GeneratedCommandManagerAttribute"/>
    ///   attribute.
    /// </para>
    /// <note>
    ///   Once a command is created, the <paramref name="options"/> instance may be modified
    ///   with the options of the <see cref="ParseOptionsAttribute"/> attribute applied to the
    ///   command class. Be aware of this if reusing the same <see cref="CommandManager"/> or
    ///   <see cref="CommandOptions"/> instance to create multiple commands.
    /// </note>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
    public CommandManager(Assembly assembly, CommandOptions? options = null)
        : this(new ReflectionCommandProvider(assembly ?? throw new ArgumentNullException(nameof(assembly)), Assembly.GetCallingAssembly()), options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandManager"/> class using the specified
    /// assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies containing the commands.</param>
    /// <param name="options">
    ///   The options to use for parsing and usage help, or <see langword="null"/> to use
    ///   the default options.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="assemblies"/> or one of its elements is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   The <see cref="CommandManager"/> class will look in the specified assemblies for any
    ///   public classes that implement the <see cref="ICommand"/> interface, have the
    ///   <see cref="CommandAttribute"/> attribute, and are not <see langword="abstract"/>.
    /// </para>
    /// <para>
    ///   If an assembly in <paramref name="assemblies"/> is the assembly that called this
    ///   constructor, both public and internal command classes will be used. For other assemblies,
    ///   only public classes are used.
    /// </para>
    /// <para>
    ///   This constructor uses reflection to determine which commands are available at runtime. To
    ///   use source generation to locate commands at compile time, use the <see cref="GeneratedCommandManagerAttribute"/>
    ///   attribute.
    /// </para>
    /// <note>
    ///   Once a command is created, the <paramref name="options"/> instance may be modified
    ///   with the options of the <see cref="ParseOptionsAttribute"/> attribute applied to the
    ///   command class. Be aware of this if reusing the same <see cref="CommandManager"/> or
    ///   <see cref="CommandOptions"/> instance to create multiple commands.
    /// </note>
    /// </remarks>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
    public CommandManager(IEnumerable<Assembly> assemblies, CommandOptions? options = null)
        : this(new ReflectionCommandProvider(assemblies ?? throw new ArgumentNullException(nameof(assemblies)), Assembly.GetCallingAssembly()), options)
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
    /// The value of the <see cref="CommandLineParser.ParseResult" qualifyHint="true"/> property after the call to the
    /// <see cref="CommandLineParser.ParseWithErrorHandling()" qualifyHint="true"/> method made while creating
    /// the command.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the <see cref="CommandLineParser.ParseWithErrorHandling()" qualifyHint="true"/> method
    ///   was not invoked, for example because the <see cref="CreateCommand()"/> method has not been
    ///   called, no command name was specified, an unknown command name was specified, or the
    ///   command used the <see cref="ICommandWithCustomParsing"/> interface , the value of the
    ///   <see cref="ParseResult.Status" qualifyHint="true"/> property will be
    ///   <see cref="ParseStatus.None" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    public ParseResult ParseResult { get; private set; }

    /// <summary>
    /// Gets the kind of <see cref="CommandProvider"/> used to supply the commands.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="Support.ProviderKind"/> enumeration.
    /// </value>
    public ProviderKind ProviderKind => _provider.Kind;

    /// <summary>
    /// Gets information about all the commands managed by this instance.
    /// </summary>
    /// <returns>
    /// Information about every subcommand defined in the assemblies, ordered by command name.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not returned.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are returned. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are returned.
    /// </para>
    /// <para>
    ///   The automatic version command is returned if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is returned.
    /// </para>
    /// </remarks>
    public IEnumerable<CommandInfo> GetCommands()
    {
        var commands = GetCommandsUnsortedAndFiltered();
        if (_options.AutoVersionCommand &&
            _options.ParentCommand == null &&
            !commands.Any(c => c.MatchesName(_options.StringProvider.AutomaticVersionCommandName())))
        {
            var versionCommand = CommandInfo.GetAutomaticVersionCommand(this);
            if (Options.CommandFilter?.Invoke(versionCommand) ?? true)
            {
                commands = commands.Append(versionCommand);
            }
        }

        return commands.OrderBy(c => c.Name, _options.CommandNameComparison.GetComparer());
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
    ///   whose command name or alias matches the specified name. If there are multiple commands
    ///   with the same name, the first matching one will be returned.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.AutoCommandPrefixAliases" qualifyHint="true"/> property is <see langword="true"/>,
    ///   this function will also return a command whose name or alias starts with
    ///   <paramref name="commandName"/>. In this case, the command will only be returned if there
    ///   is exactly one matching command; if the prefix is ambiguous, <see langword="null"/> is
    ///   returned.
    /// </para>
    /// <para>
    ///   A command's name is taken from the <see cref="CommandAttribute.CommandName" qualifyHint="true"/> property. If
    ///   that property is <see langword="null"/>, the name is determined by taking the command
    ///   type's name, and applying the transformation specified by the <see cref="CommandOptions.CommandNameTransform" qualifyHint="true"/>
    ///   property. A command's aliases are specified using the <see cref="AliasAttribute"/>
    ///   attribute.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not returned.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are returned. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are returned.
    /// </para>
    /// <para>
    ///   The automatic version command is returned if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and <paramref name="commandName"/> matches the name of
    ///   the automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is returned.
    /// </para>
    /// </remarks>
    public CommandInfo? GetCommand(string commandName)
    {
        if (commandName == null)
        {
            throw new ArgumentNullException(nameof(commandName));
        }

        var commands = GetCommandsUnsortedAndFiltered();
        CommandInfo? versionCommand = null;
        if (_options.AutoVersionCommand && _options.ParentCommand == null)
        {
            // We can append this without checking for duplicates since it will not be checked if an
            // earlier command matches.
            versionCommand = CommandInfo.GetAutomaticVersionCommand(this);
            if (_options.CommandFilter?.Invoke(versionCommand) ?? true)
            {
                commands = commands.Append(versionCommand);
            }
        }

        CommandInfo? partialMatch = null;
        var ambiguousMatch = false;
        foreach (var command in commands)
        {
            // Check for an exact match.
            if (command.MatchesName(commandName))
            {
                return command;
            }

            // Check for a prefix match, if requested.
            if (Options.AutoCommandPrefixAliases && !ambiguousMatch && command.MatchesPrefix(commandName))
            {
                if (partialMatch == null)
                {
                    partialMatch = command;
                }
                else if (command != versionCommand || !partialMatch.MatchesName(Options.StringProvider.AutomaticVersionCommandName()))
                {
                    // The prefix is ambigious, so don't use it.
                    // N.B. This doesn't apply if this is the automatic version command and the
                    //      existing match would override the existence of that command.
                    partialMatch = null;
                    ambiguousMatch = true;
                }
            }
        }

        return partialMatch;
    }

    /// <summary>
    /// Finds and instantiates the subcommand with the specified name, or if that fails, writes
    /// error and usage information.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="args">The arguments to the command.</param>
    /// <returns>
    ///   An instance of a class implementing the <see cref="ICommand"/> interface, or
    ///   <see langword="null"/> if the command was not found or an error occurred parsing the
    ///   arguments.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If the command could not be found, a list of possible commands is written using the
    ///   <see cref="ParseOptions.UsageWriter" qualifyHint="true"/> property. If an error occurs
    ///   parsing the command's arguments, the error message is written to the stream indicated by
    ///   the <see cref="ParseOptions.Error" qualifyHint="true"/> property, and the command's usage
    ///   information is written using the <see cref="ParseOptions.UsageWriter" qualifyHint="true"/>
    ///   property.
    /// </para>
    /// <para>
    ///   If the <see cref="ParseOptions.Error" qualifyHint="true"/> property is <see langword="null"/>,
    ///   output is written to a <see cref="LineWrappingTextWriter"/> instance for the standard
    ///   error stream (<see cref="Console.Error" qualifyHint="true"/>, wrapping at the console's
    ///   window width. If the stream is redirected, output may still be wrapped, depending on the
    ///   value returned by <see cref="Console.WindowWidth" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not returned.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are returned. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are returned.
    /// </para>
    /// <para>
    ///   The automatic version command is returned if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is returned.
    /// </para>
    /// </remarks>
    public ICommand? CreateCommand(string? commandName, ReadOnlyMemory<string> args)
    {
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
            var (command, result) = info.CreateInstanceWithResult(args);
            ParseResult = result;
            return command;
        }
        finally
        {
            _options.UsageWriter.CommandName = null;
        }
    }

    /// <inheritdoc cref="CreateCommand(string?, ReadOnlyMemory{string})"/>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="args">The arguments to the command.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    public ICommand? CreateCommand(string? commandName, string[] args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        return CreateCommand(commandName, args.AsMemory());
    }

    /// <inheritdoc cref="CreateCommand(string?, string[])"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the name from the first argument, or if that
    /// fails, writes error and usage information.
    /// </summary>
    /// <param name="args">
    /// The command line arguments, where the first argument is the command name and the remaining
    /// ones are arguments for the command.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>.
    /// </exception>
    public ICommand? CreateCommand(string[] args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        return CreateCommand(args.AsMemory());
    }

    /// <inheritdoc cref="CreateCommand(string?, ReadOnlyMemory{string})"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the name from the first argument, or if that
    /// fails, writes error and usage information.
    /// </summary>
    /// <param name="args">
    /// The command line arguments, where the first argument is the command name and the remaining
    /// ones are arguments for the command.
    /// </param>
    public ICommand? CreateCommand(ReadOnlyMemory<string> args)
    {
        string? commandName = null;
        if (args.Length != 0)
        {
            commandName = args.Span[0];
            args = args.Slice(1);
        }

        return CreateCommand(commandName, args);
    }

    /// <summary>
    /// Finds and instantiates the subcommand using the arguments from the <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>
    /// method, using the first argument for the command name. If that fails, writes error and usage
    /// information.
    /// </summary>
    /// <returns>
    /// <inheritdoc cref="CreateCommand(string?, ReadOnlyMemory{string})"/>
    /// </returns>
    /// <remarks>
    /// <inheritdoc cref="CreateCommand(string?, ReadOnlyMemory{string})"/>
    /// </remarks>
    public ICommand? CreateCommand()
    {
        // Skip the first argument, it's the application name.
        return CreateCommand(Environment.GetCommandLineArgs().AsMemory(1));
    }


    /// <summary>
    /// Finds and instantiates the subcommand with the specified name, and if it succeeds,
    /// runs it. If it fails, writes error and usage information.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="args">The arguments to the command.</param>
    /// <returns>
    ///   The value returned by <see cref="ICommand.Run" qualifyHint="true"/>, or <see langword="null"/> if
    ///   the command could not be created.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="args"/> is <see langword="null"/>
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(string?, string[])"/>
    ///   method, and then invokes the <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> is <see langword="null"/>, only
    ///   commands without a <see cref="ParentCommandAttribute"/> attribute are included. If it is
    ///   not <see langword="null"/>, only commands where the type specified using the
    ///   <see cref="ParentCommandAttribute"/> attribute matches the value of the property are
    ///   included.
    /// </para>
    /// </remarks>
    public int? RunCommand(string? commandName, string[] args)
    {
        var command = CreateCommand(commandName, args);
        return command?.Run();
    }

    /// <summary>
    /// Finds and instantiates the subcommand with the specified name, and if it succeeds,
    /// runs it. If it fails, writes error and usage information.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <param name="args">The arguments to the command.</param>
    /// <returns>
    ///   The value returned by <see cref="ICommand.Run" qualifyHint="true"/>, or <see langword="null"/> if
    ///   the command could not be created.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(string?, ReadOnlyMemory{string})"/>
    ///   method, and then invokes the <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> is <see langword="null"/>, only
    ///   commands without a <see cref="ParentCommandAttribute"/> attribute are included. If it is
    ///   not <see langword="null"/>, only commands where the type specified using the
    ///   <see cref="ParentCommandAttribute"/> attribute matches the value of the property are
    ///   included.
    /// </para>
    /// </remarks>
    public int? RunCommand(string? commandName, ReadOnlyMemory<string> args)
    {
        var command = CreateCommand(commandName, args);
        return command?.Run();
    }

    /// <inheritdoc cref="RunCommand(string?, ReadOnlyMemory{string})"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the name from the first argument, and if it
    /// succeeds, runs it. If it fails, writes error and usage information.
    /// </summary>
    /// <param name="args">
    /// The command line arguments, where the first argument is the command name and the remaining
    /// ones are arguments for the command.
    /// </param>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(ReadOnlyMemory{string})"/>
    ///   method, and then invokes the <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> is <see langword="null"/>, only
    ///   commands without a <see cref="ParentCommandAttribute"/> attribute are included. If it is
    ///   not <see langword="null"/>, only commands where the type specified using the
    ///   <see cref="ParentCommandAttribute"/> attribute matches the value of the property are
    ///   included.
    /// </para>
    /// </remarks>
    public int? RunCommand(ReadOnlyMemory<string> args)
    {
        var command = CreateCommand(args);
        return command?.Run();
    }

    /// <inheritdoc cref="RunCommand(string?, string[])"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the name from the first argument, and if it
    /// succeeds, runs it. If it fails, writes error and usage information.
    /// </summary>
    /// <param name="args">
    /// The command line arguments, where the first argument is the command name and the remaining
    /// ones are arguments for the command.
    /// </param>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(string[])"/>
    ///   method, and then invokes the <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> is <see langword="null"/>, only
    ///   commands without a <see cref="ParentCommandAttribute"/> attribute are included. If it is
    ///   not <see langword="null"/>, only commands where the type specified using the
    ///   <see cref="ParentCommandAttribute"/> attribute matches the value of the property are
    ///   included.
    /// </para>
    /// </remarks>
    public int? RunCommand(string[] args)
    {
        var command = CreateCommand(args);
        return command?.Run();
    }

    /// <summary>
    /// Finds and instantiates the subcommand using the arguments from the <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>
    /// method, using the first argument as the command name. If it succeeds, runs the command.
    /// If it fails, writes error and usage information.
    /// </summary>
    /// <returns>
    /// <inheritdoc cref="RunCommand(string?, string[])"/>
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand()"/> method,
    ///   and then invokes the <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
    /// </para>
    /// </remarks>
    public int? RunCommand()
    {
        // Skip the first argument, it's the application name.
        return RunCommand(Environment.GetCommandLineArgs().AsMemory(1));
    }

    /// <inheritdoc cref="RunCommand(string?, ReadOnlyMemory{string})"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the specified name, and if it succeeds,
    /// runs it asynchronously. If it fails, writes error and usage information.
    /// </summary>
    /// <returns>
    ///   A task representing the asynchronous run operation. The result is the value returned
    ///   by <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/>, or <see langword="null"/> if the command
    ///   could not be created.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(string?, ReadOnlyMemory{string})"/>
    ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
    ///   invokes the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/> method; otherwise, it invokes the
    ///   <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
    /// </para>
    /// </remarks>
    public async Task<int?> RunCommandAsync(string? commandName, ReadOnlyMemory<string> args)
    {
        var command = CreateCommand(commandName, args);
        if (command is IAsyncCommand asyncCommand)
        {
            return await asyncCommand.RunAsync();
        }

        return command?.Run();
    }

    /// <inheritdoc cref="RunCommand(string?, string[])"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the specified name, and if it succeeds,
    /// runs it asynchronously. If it fails, writes error and usage information.
    /// </summary>
    /// <returns>
    ///   A task representing the asynchronous run operation. The result is the value returned
    ///   by <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/>, or <see langword="null"/> if the command
    ///   could not be created.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(string?, string[])"/>
    ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
    ///   invokes the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/> method; otherwise, it invokes the
    ///   <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
    /// </para>
    /// </remarks>
    public async Task<int?> RunCommandAsync(string? commandName, string[] args)
    {
        var command = CreateCommand(commandName, args);
        if (command is IAsyncCommand asyncCommand)
        {
            return await asyncCommand.RunAsync();
        }

        return command?.Run();
    }

    /// <inheritdoc cref="RunCommandAsync(string?, ReadOnlyMemory{string})"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the name from the first argument, and if it
    /// succeeds, runs it asynchronously. If it fails, writes error and usage information.
    /// </summary>
    /// <remarks>
    /// <param name="args">
    /// The command line arguments, where the first argument is the command name and the remaining
    /// ones are arguments for the command.
    /// </param>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(ReadOnlyMemory{string})"/>
    ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
    ///   invokes the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/> method; otherwise, it invokes the
    ///   <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
    /// </para>
    /// </remarks>
    public async Task<int?> RunCommandAsync(ReadOnlyMemory<string> args)
    {
        var command = CreateCommand(args);
        if (command is IAsyncCommand asyncCommand)
        {
            return await asyncCommand.RunAsync();
        }

        return command?.Run();
    }

    /// <inheritdoc cref="RunCommandAsync(string?, string[])"/>
    /// <summary>
    /// Finds and instantiates the subcommand with the name from the first argument, and if it
    /// succeeds, runs it asynchronously. If it fails, writes error and usage information.
    /// </summary>
    /// <param name="args">
    /// The command line arguments, where the first argument is the command name and the remaining
    /// ones are arguments for the command.
    /// </param>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand(string[])"/>
    ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
    ///   invokes the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/> method; otherwise, it invokes the
    ///   <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
    /// </para>
    /// </remarks>
    public async Task<int?> RunCommandAsync(string[] args)
    {
        var command = CreateCommand(args);
        if (command is IAsyncCommand asyncCommand)
        {
            return await asyncCommand.RunAsync();
        }

        return command?.Run();
    }

    /// <summary>
    /// Finds and instantiates the subcommand using the arguments from the <see cref="Environment.GetCommandLineArgs" qualifyHint="true"/>
    /// method, using the first argument as the command name. If it succeeds, runs the command
    /// asynchronously. If it fails, writes error and usage information.
    /// </summary>
    /// <returns>
    /// <inheritdoc cref="RunCommandAsync(string?, string[])"/>
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This function creates the command by invoking the <see cref="CreateCommand()"/>
    ///   method. If the command implements the <see cref="IAsyncCommand"/> interface, it
    ///   invokes the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/> method; otherwise, it invokes the
    ///   <see cref="ICommand.Run" qualifyHint="true"/> method on the command.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
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
    ///   This method writes usage help for the application, including a list of all
    ///   subcommand names and their descriptions using the <see cref="ParseOptions.UsageWriter" qualifyHint="true"/>
    ///   property.
    /// </para>
    /// <para>
    ///   A command's name is retrieved from its <see cref="CommandAttribute"/> attribute,
    ///   and the description is retrieved from its <see cref="DescriptionAttribute"/> attribute.
    /// </para>
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
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
    /// <para>
    ///   Commands that don't meet the criteria of the <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   predicate are not included.
    /// </para>
    /// <para>
    ///   If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is
    ///   <see langword="null"/>, only commands without a <see cref="ParentCommandAttribute"/>
    ///   attribute are included. If it is not <see langword="null"/>, only commands where the type
    ///   specified using the <see cref="ParentCommandAttribute"/> attribute matches the value of
    ///   the property are included.
    /// </para>
    /// <para>
    ///   The automatic version command is included if the <see cref="CommandOptions.AutoVersionCommand" qualifyHint="true"/>
    ///   property is <see langword="true"/> and the command name matches the name of the
    ///   automatic version command, and not any other command name. The <see cref="CommandOptions.CommandFilter" qualifyHint="true"/>
    ///   and <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property also affect
    ///   whether the version command is included.
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
    /// If the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property is not <see langword="null"/>,
    /// and the command type referenced has the <see cref="DescriptionAttribute"/> attribute, the
    /// description given in that attribute. Otherwise, the value of the
    /// <see cref="AssemblyDescriptionAttribute"/> for the first assembly used by this instance.
    /// </returns>
    /// <seealso cref="UsageWriter.IncludeApplicationDescriptionBeforeCommandList" qualifyHint="true"/>
    public string? GetApplicationDescription()
    {
        var attribute = _options.ParentCommand?.GetCustomAttribute<DescriptionAttribute>();
        if (attribute != null)
        {
            return attribute.Description;
        }

        return _provider.GetApplicationDescription();
    }

    private IEnumerable<CommandInfo> GetCommandsUnsortedAndFiltered()
    {
        var commands = _provider.GetCommandsUnsorted(this).Where(c => c.ParentCommandType == _options.ParentCommand);
        if (_options.CommandFilter != null)
        {
            commands = commands.Where(c => _options.CommandFilter(c));
        }

        return commands;
    }
}
