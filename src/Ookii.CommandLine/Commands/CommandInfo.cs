using Ookii.CommandLine.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Provides information about a subcommand.
/// </summary>
/// <seealso cref="CommandManager"/>
/// <seealso cref="ICommand"/>
/// <seealso cref="CommandAttribute"/>
/// <threadsafety static="true" instance="true"/>
public abstract class CommandInfo
{
    private readonly CommandManager _manager;
    private readonly string _name;
    private readonly Type _commandType;
    private readonly CommandAttribute _attribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandInfo"/> class.
    /// </summary>
    /// <param name="commandType">The type that implements the subcommand.</param>
    /// <param name="attribute">The <see cref="CommandAttribute"/> for the subcommand type.</param>
    /// <param name="parentCommandType">The <see cref="Type"/> of a command that is the parent of this command.</param>
    /// <param name="manager">
    ///   The <see cref="CommandManager"/> that is managing this command.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="commandType"/> or <paramref name="manager"/> is <see langword="null"/>.
    /// </exception>
    protected CommandInfo(Type commandType, CommandAttribute attribute, CommandManager manager, Type? parentCommandType)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _name = GetName(attribute, commandType, manager.Options);
        _commandType = commandType;
        _attribute = attribute;
        ParentCommandType = parentCommandType;
    }

    internal CommandInfo(Type commandType, string name, CommandManager manager)
    {
        _manager = manager;
        _name = name;
        _commandType = commandType;
        _attribute = new();
    }

    /// <summary>
    /// Gets the <see cref="CommandManager"/> that this instance belongs to.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="CommandManager"/> class.
    /// </value>
    public CommandManager Manager => _manager;

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    /// <value>
    /// The name of the command.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The name is taken from the <see cref="CommandAttribute.CommandName" qualifyHint="true"/> property. If
    ///   that property is <see langword="null"/>, the name is determined by taking the command
    ///   type's name, and applying the transformation specified by the <see cref="CommandOptions.CommandNameTransform" qualifyHint="true"/>
    ///   property.
    /// </para>
    /// </remarks>
    public string Name => _name;

    /// <summary>
    /// Gets the type that implements the command.
    /// </summary>
    /// <value>
    /// The type that implements the command.
    /// </value>
    public Type CommandType => _commandType;

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    /// <value>
    /// The description of the command, determined using the <see cref="DescriptionAttribute"/>
    /// attribute.
    /// </value>
    public abstract string? Description { get; }

    /// <summary>
    /// Gets a value that indicates if the command uses custom parsing.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the command type implements the <see cref="ICommandWithCustomParsing"/>
    /// interface; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool UseCustomArgumentParsing { get; }

    /// <summary>
    /// Gets or sets a value that indicates whether the command is hidden from the usage help.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the command is hidden from the usage help; otherwise,
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A hidden command will not be included in the command list when usage help is
    ///   displayed, but can still be invoked from the command line.
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandAttribute.IsHidden" qualifyHint="true"/>
    public bool IsHidden => _attribute.IsHidden;

    /// <summary>
    /// Gets the alternative names of this command.
    /// </summary>
    /// <value>
    /// A list of aliases.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Aliases for a command are specified by using the <see cref="AliasAttribute"/> attribute
    ///   on a class implementing the <see cref="ICommand"/> interface.
    /// </para>
    /// </remarks>
    public abstract IEnumerable<string> Aliases { get; }

    /// <summary>
    /// Gets the type of the command that is the parent of this command.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the parent command, or <see langword="null"/> if this command
    /// does not have a parent.
    /// </value>
    /// <remarks>
    /// <para>
    ///   Subcommands can specify their parent using the <see cref="ParentCommandAttribute"/>
    ///   attribute.
    /// </para>
    /// <para>
    ///   The <see cref="CommandManager"/> class will only use commands whose parent command
    ///   type matches the value of the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/> property.
    /// </para>
    /// </remarks>
    public Type? ParentCommandType { get; }

    /// <summary>
    /// Creates an instance of the command type parsing the specified arguments.
    /// </summary>
    /// <param name="args">The arguments to the command.</param>
    /// <returns>
    /// An instance of the <see cref="CommandType"/>, or <see langword="null"/> if an error
    /// occurred or parsing was canceled.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If the type indicated by the <see cref="CommandType"/> property implements the
    ///   <see cref="ICommandWithCustomParsing"/> parsing interface, an instance of the type is
    ///   created and the <see cref="ICommandWithCustomParsing.Parse" qualifyHint="true"/> method
    ///   invoked. Otherwise, an instance of the type is created using the <see cref="CommandLineParser{T}"/>
    ///   class.
    /// </para>
    /// </remarks>
    public ICommand? CreateInstance(ReadOnlyMemory<string> args)
    {
        var (command, _) = CreateInstanceWithResult(args);
        return command;
    }

    /// <summary>
    /// Creates an instance of the command type by parsing the specified arguments, and returns it
    /// in addition to the result of the parsing operation.
    /// </summary>
    /// <param name="args">The arguments to the command.</param>
    /// <returns>
    /// A tuple containing an instance of the <see cref="CommandType"/>, or <see langword="null"/> if an error
    /// occurred or parsing was canceled, and the <see cref="ParseResult"/> of the operation.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If the type indicated by the <see cref="CommandType"/> property implements the
    ///   <see cref="ICommandWithCustomParsing"/> parsing interface, an instance of the type is
    ///   created and the <see cref="ICommandWithCustomParsing.Parse" qualifyHint="true"/> method
    ///   invoked. Otherwise, an instance of the type is created using the <see cref="CommandLineParser"/>
    ///   class.
    /// </para>
    /// <para>
    ///   The <see cref="ParseResult.Status" qualifyHint="true"/> property of the returned <see cref="ParseResult"/>
    ///   will be <see cref="ParseStatus.None" qualifyHint="true"/> if the command used custom parsing.
    /// </para>
    /// </remarks>
    public (ICommand?, ParseResult) CreateInstanceWithResult(ReadOnlyMemory<string> args)
    {
        if (UseCustomArgumentParsing)
        {
            var command = CreateInstanceWithCustomParsing();
            command.Parse(args, _manager);
            return (command, default);
        }
        else
        {
            var parser = CreateParser();
            var command = (ICommand?)parser.ParseWithErrorHandling(args);
            return (command, parser.ParseResult);
        }
    }

    /// <summary>
    /// Creates a <see cref="CommandLineParser"/> instance for the type indicated by the
    /// <see cref="CommandType"/> property.
    /// </summary>
    /// <returns>
    /// A <see cref="CommandLineParser"/> instance for the <see cref="CommandType"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///   The command uses the <see cref="ICommandWithCustomParsing"/> interface.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   If the <see cref="UseCustomArgumentParsing"/> property is <see langword="true"/>, the
    ///   command cannot be created using the <see cref="CommandLineParser"/> class, and you
    ///   must use the <see cref="CreateInstanceWithCustomParsing"/> method or
    ///   <see cref="CreateInstanceWithResult(ReadOnlyMemory{string})"/> method instead.
    /// </para>
    /// </remarks>
    public abstract CommandLineParser CreateParser();

    /// <summary>
    /// Creates an instance of a command that uses the <see cref="ICommandWithCustomParsing"/>
    /// interface.
    /// </summary>
    /// <returns>An instance of the command type.</returns>
    /// <exception cref="InvalidOperationException">
    ///   The command does not use the <see cref="ICommandWithCustomParsing"/> interface.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   It is the responsibility of the caller to invoke the <see cref="ICommandWithCustomParsing.Parse" qualifyHint="true"/>
    ///   method after the instance is created.
    /// </para>
    /// </remarks>
    public abstract ICommandWithCustomParsing CreateInstanceWithCustomParsing();

    /// <summary>
    /// Checks whether the command's name or aliases match the specified name.
    /// </summary>
    /// <param name="name">The name to check for.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="name"/> matches the <see cref="Name"/>
    /// property or any of the items in the <see cref="Aliases"/> property.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   Automatic prefix aliases are not considered by this method, regardless of the value of
    ///   the <see cref="CommandOptions.AutoCommandPrefixAliases"/> property. To check for a prefix,
    ///   use the <see cref="MatchingPrefix"/> method.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    public bool MatchesName(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.Equals(name, _name, Manager.Options.CommandNameComparison))
        {
            return true;
        }

        return Aliases.Any(alias => string.Equals(name, alias, Manager.Options.CommandNameComparison));
    }

    /// <summary>
    /// Checks whether the command's name or one of its aliases start with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to check for.</param>
    /// <returns>
    /// The name or alias that matched the prefix, or <see langword="null"/> if no match was found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="prefix"/> is <see langword="null"/>.
    /// </exception>
    public string? MatchingPrefix(string prefix)
    {
        if (prefix == null)
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        if (Name.StartsWith(prefix, Manager.Options.CommandNameComparison))
        {
            return Name;
        }

        return Aliases.FirstOrDefault(alias => alias.StartsWith(prefix, Manager.Options.CommandNameComparison));
    }

    /// <summary>
    /// Creates an instance of the <see cref="CommandInfo"/> class only if <paramref name="commandType"/>
    /// represents a command type.
    /// </summary>
    /// <param name="commandType">The type that implements the subcommand.</param>
    /// <param name="manager">
    ///   The <see cref="CommandManager"/> that is managing this command.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="commandType"/> or <paramref name="manager"/> is <see langword="null"/>.
    /// </exception>
    /// <returns>
    ///   If the type specified by <paramref name="commandType"/> implements the <see cref="ICommand"/>
    ///   interface, has the <see cref="CommandAttribute"/> attribute, and is not <see langword="abstract"/>,
    ///   a <see cref="CommandInfo"/> class with information about the command; otherwise,
    ///   <see langword="null"/>.
    /// </returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
#endif
    public static CommandInfo? TryCreate(Type commandType, CommandManager manager)
        => ReflectionCommandInfo.TryCreate(commandType, manager);

    /// <summary>
    /// Creates an instance of the <see cref="CommandInfo"/> class for the specified command
    /// type.
    /// </summary>
    /// <param name="commandType">The type that implements the subcommand.</param>
    /// <param name="manager">
    ///   The <see cref="CommandManager"/> that is managing this command.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="commandType"/> or <paramref name="manager"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="commandType"/> is does not implement the <see cref="ICommand"/> interface,
    ///   does not have the <see cref="CommandAttribute"/> attribute, or is <see langword="abstract"/>.
    /// </exception>
    /// <returns>
    ///   A <see cref="CommandInfo"/> class with information about the command.
    /// </returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
#endif
    public static CommandInfo Create(Type commandType, CommandManager manager)
        => new ReflectionCommandInfo(commandType, null, manager);

    /// <summary>
    /// Returns a value indicating if the specified type is a subcommand.
    /// </summary>
    /// <param name="commandType">The type that implements the subcommand.</param>
    /// <returns>
    /// <see langword="true"/> if the type implements the <see cref="ICommand"/> interface, has the
    /// <see cref="CommandAttribute"/> attribute applied, and is not <see langword="abstract"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="commandType"/> is <see langword="null"/>.
    /// </exception>
    public static bool IsCommand(
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
        Type commandType
        ) => GetCommandAttribute(commandType) != null;

    internal static CommandInfo GetAutomaticVersionCommand(CommandManager manager)
        => new AutomaticVersionCommandInfo(manager);

    internal static CommandAttribute? GetCommandAttribute(
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
        Type commandType)
    {
        if (commandType == null)
        {
            throw new ArgumentNullException(nameof(commandType));
        }

        if (commandType.IsAbstract || !commandType.ImplementsInterface(typeof(ICommand)))
        {
            return null;
        }

        return commandType.GetCustomAttribute<CommandAttribute>();
    }

    private static string GetName(CommandAttribute attribute, Type commandType, CommandOptions? options)
    {
        return attribute.CommandName ??
            options?.CommandNameTransform.Apply(commandType.Name, options.StripCommandNameSuffix) ??
            commandType.Name;
    }
}
