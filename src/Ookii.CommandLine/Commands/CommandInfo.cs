using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Provides information about a subcommand.
    /// </summary>
    public struct CommandInfo
    {
        private readonly string _name;
        private readonly Type _commandType;
        private readonly CommandAttribute _attribute;
        private string? _description;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo"/> struct.
        /// </summary>
        /// <param name="commandType">The type that implements the subcommand.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="commandType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="commandType"/> is not a command type.
        /// </exception>
        public CommandInfo(Type commandType)
            : this(commandType, GetCommandAttributeOrThrow(commandType))
        {
        }

        private CommandInfo(string name, Type commandType, string description)
        {
            var attribute = GetCommandAttribute(commandType)!;
            _name = name;
            _commandType = commandType;
            _description = description;
            _attribute = attribute;
        }

        private CommandInfo(Type commandType, CommandAttribute attribute)
        {
            _name = GetName(attribute, commandType);
            _commandType = commandType;
            _description = null;
            _attribute = attribute;
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command, based on either the <see cref="CommandAttribute.CommandName"/>
        /// property or the <see cref="CommandType"/>'s name.
        /// </value>
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
        public string? Description => _description ??= GetCommandDescription();

        /// <summary>
        /// Gets a value that indicates if the command uses custom parsing.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the command type implements the <see cref="ICommandWithCustomParsing"/>
        /// interface; otherwise, <see langword="false"/>.
        /// </value>
        public bool UseCustomArgumentParsing => _commandType.ImplementsInterface(typeof(ICommandWithCustomParsing));

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
        ///   displayed.
        /// </para>
        /// </remarks>
        public bool IsHidden => _attribute.IsHidden;

        /// <summary>
        /// Creates an instance of the command type.
        /// </summary>
        /// <param name="args">The arguments to the command.</param>
        /// <param name="index">The index in <paramref name="args"/> at which to start parsing the arguments.</param>
        /// <param name="options">
        ///   The options that control parsing behavior. If <see langword="null" />, the default
        ///   options are used.
        /// </param>
        /// <returns>An instance of the <see cref="CommandType"/>.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="args"/>, or <paramref name="options"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not fall inside the bounds of <paramref name="args"/>.</exception>
        public ICommand CreateInstance(string[] args, int index, ParseOptions? options = null)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (index < 0 || index > args.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            options ??= new();
            if (UseCustomArgumentParsing)
            {
                var command = (ICommandWithCustomParsing)Activator.CreateInstance(CommandType)!;
                command.Parse(args, index, options);
                return command;
            }

            return (ICommand)CommandLineParser.ParseInternal(CommandType, args, index, options)!;
        }

        /// <summary>
        /// Creates an instance of the <see cref="CommandInfo"/> structure only if <paramref name="commandType"/>
        /// represents a command type.
        /// </summary>
        /// <param name="commandType">The type that implements the subcommand.</param>
        /// <returns>
        ///   A <see cref="CommandInfo"/> structure with information about the command, or
        ///   <see langword="null"/> if <paramref name="commandType"/> was not a command.
        /// </returns>
        public static CommandInfo? CreateIfCommand(Type commandType)
        {
            var attribute = GetCommandAttribute(commandType);
            if (attribute == null)
                return null;

            return new CommandInfo(commandType, attribute);
        }

        /// <summary>
        /// Returns a value indicating if the specified type is a subcommand.
        /// </summary>
        /// <param name="commandType">The type that implements the subcommand.</param>
        /// <returns>
        /// <see langword="true"/> if the type implements the <see cref="ICommand"/> interface and
        /// has the <see cref="CommandAttribute"/> applied; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsCommand(Type commandType)
        {
            return GetCommandAttribute(commandType) != null;
        }

        internal static CommandInfo GetAutomaticVersionCommand(LocalizedStringProvider stringProvider)
        {
            return new CommandInfo(stringProvider.AutomaticVersionCommandName(),
                typeof(AutomaticVersionCommand), stringProvider.AutomaticVersionCommandDescription());
        }

        private static CommandAttribute? GetCommandAttribute(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            if (commandType.IsAbstract || !commandType.ImplementsInterface(typeof(ICommand)))
                return null;

            return commandType.GetCustomAttribute<CommandAttribute>();
        }

        private static CommandAttribute GetCommandAttributeOrThrow(Type commandType)
        {
            return GetCommandAttribute(commandType) ?? 
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, 
                    Properties.Resources.TypeIsNotCommandFormat, commandType.FullName));
        }

        private static string GetName(CommandAttribute attribute, Type commandType)
        {
            return attribute.CommandName ?? commandType.Name;
        }

        private string? GetCommandDescription()
        {
            return _commandType.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
    }
}
