using Ookii.CommandLine.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Provides information about a subcommand.
    /// </summary>
    /// <seealso cref="CommandManager"/>
    /// <seealso cref="ICommand"/>
    /// <seealso cref="CommandAttribute"/>
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
        /// <exception cref="ArgumentException">
        ///   <paramref name="commandType"/> is not a command type.
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
        ///   The name is taken from the <see cref="CommandAttribute.CommandName"/> property. If
        ///   that property is <see langword="null"/>, the name is determined by taking the command
        ///   type's name, and applying the transformation specified by the <see cref="CommandOptions.CommandNameTransform"/>
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
        /// <seealso cref="CommandAttribute.IsHidden"/>
        public bool IsHidden => _attribute.IsHidden;

        /// <summary>
        /// Gets the alternative names of this command.
        /// </summary>
        /// <value>
        /// A list of aliases.
        /// </value>
        /// <remarks>
        /// <para>
        ///   Aliases for a command are specified by using the <see cref="AliasAttribute"/> on a
        ///   class implementing the <see cref="ICommand"/> interface.
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
        ///   type matches the value of the <see cref="CommandOptions.ParentCommand"/> property.
        /// </para>
        /// </remarks>
        public Type? ParentCommandType { get; }

        /// <summary>
        /// Creates an instance of the command type.
        /// </summary>
        /// <param name="args">The arguments to the command.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <returns>
        /// An instance of the <see cref="CommandType"/>, or <see langword="null"/> if an error
        /// occurred or parsing was canceled.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.</exception>
        public ICommand? CreateInstance(string[] args, int index)
        {
            var (command, _) = CreateInstanceWithResult(args, index);
            return command;
        }

        /// <summary>
        /// Creates an instance of the command type.
        /// </summary>
        /// <param name="args">The arguments to the command.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <returns>
        /// A tuple containing an instance of the <see cref="CommandType"/>, or <see langword="null"/> if an error
        /// occurred or parsing was canceled, and the <see cref="ParseResult"/> of the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   The <see cref="ParseResult.Status"/> property of the returned <see cref="ParseResult"/>
        ///   will be <see cref="ParseStatus.None"/> if the command used custom parsing.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.</exception>
        public (ICommand?, ParseResult) CreateInstanceWithResult(string[] args, int index)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            if (index < 0 || index > args.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return CreateInstanceWithResult(args.AsMemory(index));
        }

        /// <summary>
        /// Creates an instance of the command type.
        /// </summary>
        /// <param name="args">The arguments to the command.</param>
        /// <returns>
        /// A tuple containing an instance of the <see cref="CommandType"/>, or <see langword="null"/> if an error
        /// occurred or parsing was canceled, and the <see cref="ParseResult"/> of the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   The <see cref="ParseResult.Status"/> property of the returned <see cref="ParseResult"/>
        ///   will be <see cref="ParseStatus.None"/> if the command used custom parsing.
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
        /// Creates a <see cref="CommandLineParser"/> instance that can be used to instantiate
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
        ///   command cannot be created suing the <see cref="CommandLineParser"/> class, and you
        ///   must use the <see cref="CreateInstance"/> method.
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
        public abstract ICommandWithCustomParsing CreateInstanceWithCustomParsing();

        /// <summary>
        /// Checks whether the command's name or aliases match the specified name.
        /// </summary>
        /// <param name="name">The name to check for.</param>
        /// <param name="comparer">
        /// The <see cref="IComparer{T}"/> to use for the comparisons, or <see langword="null"/>
        /// to use the default comparison, which is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="name"/> matches the <see cref="Name"/>
        /// property or any of the items in the <see cref="Aliases"/> property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public bool MatchesName(string name, IComparer<string>? comparer = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            comparer ??= StringComparer.OrdinalIgnoreCase;
            if (comparer.Compare(name, _name) == 0)
            {
                return true;
            }

            return Aliases.Any(alias => comparer.Compare(name, alias) == 0);
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
        ///   A <see cref="CommandInfo"/> class with information about the command, or
        ///   <see langword="null"/> if <paramref name="commandType"/> was not a command.
        /// </returns>
#if NET6_0_OR_GREATER
        [RequiresUnreferencedCode("Trimming cannot be used when determining commands via reflection. Use the GeneratedCommandManagerAttribute instead.")]
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
        ///   <paramref name="commandType"/> is not a command.
        /// </exception>
        /// <returns>
        ///   A <see cref="CommandInfo"/> class with information about the command.
        /// </returns>
#if NET6_0_OR_GREATER
        [RequiresUnreferencedCode("Trimming cannot be used when determining commands via reflection. Use the GeneratedCommandManagerAttribute instead.")]
#endif
        public static CommandInfo Create(Type commandType, CommandManager manager)
            => new ReflectionCommandInfo(commandType, null, manager);

        /// <summary>
        /// Returns a value indicating if the specified type is a subcommand.
        /// </summary>
        /// <param name="commandType">The type that implements the subcommand.</param>
        /// <returns>
        /// <see langword="true"/> if the type implements the <see cref="ICommand"/> interface and
        /// has the <see cref="CommandAttribute"/> applied; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandType"/> is <see langword="null"/>.
        /// </exception>
#if NET6_0_OR_GREATER
        [RequiresUnreferencedCode("Trimming cannot be used when determining commands via reflection. Use the GeneratedCommandManagerAttribute instead.")]
#endif
        public static bool IsCommand(Type commandType) => ReflectionCommandInfo.GetCommandAttribute(commandType) != null;

        internal static CommandInfo GetAutomaticVersionCommand(CommandManager manager)
            => new AutomaticVersionCommandInfo(manager);

        private static string GetName(CommandAttribute attribute, Type commandType, CommandOptions? options)
        {
            return attribute.CommandName ??
                options?.CommandNameTransform.Apply(commandType.Name, options.StripCommandNameSuffix) ??
                commandType.Name;
        }
    }
}
