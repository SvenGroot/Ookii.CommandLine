using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Base class for subcommands that have nested subcommands.
/// </summary>
/// <remarks>
/// <para>
///   The <see cref="ParentCommand"/> class, along with the <see cref="ParentCommandAttribute"/>
///   attribute, aid in easily creating applications that contain nested subcommands. This class
///   handles finding, creating and running any nested subcommands, and handling parsing errors and
///   printing usage help for those subcommands.
/// </para>
/// <para>
///   To utilize this class, derive a class from this class and apply the <see cref="CommandAttribute"/>
///   attribute to that class. Then, apply the <see cref="ParentCommandAttribute"/> attribute to any
///   child commands of this command.
/// </para>
/// <para>
///   Often, the derived class can be empty; however, you can override the members of this class
///   to customize the behavior.
/// </para>
/// <para>
///   The <see cref="ParentCommand"/> class is based on the <see cref="ICommandWithCustomParsing"/>
///   interface, so derived classes cannot define any arguments or use other functionality that
///   depends on the <see cref="CommandLineParser"/> class.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public abstract class ParentCommand : ICommandWithCustomParsing, IAsyncCommand
{
    private ICommand? _childCommand;

    /// <summary>
    /// Gets a value that indicates whether a child command was successfully created when parsing
    /// the command line arguments.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a child command was created; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsChildCommandCreated => _childCommand != null;

    /// <summary>
    /// Gets the exit code to return from the <see cref="Run"/> or <see cref="RunAsync"/> method
    /// if parsing command line arguments for a nested subcommand failed.
    /// </summary>
    /// <value>
    /// The exit code to use for parsing failure. The base class implementation returns 1.
    /// </value>
    protected virtual int FailureExitCode => 1;

    /// <summary>
    /// Parses the arguments for the command, locating and instantiating a child command.
    /// </summary>
    /// <param name="args">
    /// The arguments for the command, where the first argument is the name of the child command.
    /// </param>
    /// <param name="manager">
    /// The <see cref="CommandManager"/> instance that was used to create this command.
    /// </param>
    public void Parse(ReadOnlyMemory<string> args, CommandManager manager)
    {
        OnModifyOptions(manager.Options);
        var originalParentCommand = manager.Options.ParentCommand;
        manager.Options.ParentCommand = GetType();
        CommandInfo? info;
        try
        {
            var childCommandName = args.Length == 0 ? null : args.Span[0];
            (info, var possibleMatches) = childCommandName == null
                ? default
                : manager.GetCommandOrPossibleMatches(childCommandName);

            if (info == null)
            {
                OnChildCommandNotFound(manager, childCommandName, possibleMatches);
                return;
            }
        }
        finally
        {
            manager.Options.ParentCommand = originalParentCommand;
        }

        args = args.Slice(1);
        var originalCommandName = manager.Options.UsageWriter.CommandName;
        manager.Options.UsageWriter.CommandName = originalCommandName == null ? info.Name : originalCommandName + ' ' + info.Name;
        try
        {
            if (info.UseCustomArgumentParsing)
            {
                var command = info.CreateInstanceWithCustomParsing();
                command.Parse(args, manager);
                _childCommand = command;
                OnAfterParsing(null, command);
                return;
            }

            var parser = info.CreateParser();
            EventHandler<DuplicateArgumentEventArgs>? handler = null;
            if (parser.Options.DuplicateArgumentsOrDefault == ErrorMode.Warning)
            {
                handler = (sender, e) =>
                {
                    e.KeepOldValue = !OnDuplicateArgumentWarning(e.Argument, e.NewValue);
                };

                parser.DuplicateArgument += handler;
            }

            try
            {
                _childCommand = (ICommand?)parser.Parse(args);
            }
            catch (CommandLineArgumentException)
            {
                // Handled by OnAfterParsing.
            }

            OnAfterParsing(parser, _childCommand);
        }
        finally
        {
            manager.Options.UsageWriter.CommandName = originalCommandName;
        }
    }

    /// <summary>
    /// Runs the child command that was instantiated by the <see cref="Parse"/> method.
    /// </summary>
    /// <returns>
    /// The exit code of the child command, or the value of the <see cref="FailureExitCode"/>
    /// property if no child command was created.
    /// </returns>
    public virtual int Run()
    {
        if (_childCommand == null)
        {
            return FailureExitCode;
        }

        return _childCommand.Run();
    }

    /// <summary>
    /// Runs the child command that was instantiated by the <see cref="Parse"/> method asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous run operation. The result of the task is the exit
    /// code of the child command, or the value of the <see cref="FailureExitCode"/> property if no
    /// child command was created.
    /// </returns>
    public virtual async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        if (_childCommand == null)
        {
            return FailureExitCode;
        }

        if (_childCommand is IAsyncCommand asyncCommand)
        {
            return await asyncCommand.RunAsync(cancellationToken);
        }

        return _childCommand.Run();
    }

    /// <summary>
    /// Allows derived classes to customize the command and parse options used for the nested
    /// subcommands.
    /// </summary>
    /// <param name="options">The <see cref="CommandOptions"/>.</param>
    /// <remarks>
    /// <para>
    ///   The base class implementation does nothing.
    /// </para>
    /// </remarks>
    protected virtual void OnModifyOptions(CommandOptions options)
    {
        // Intentionally blank
    }

    /// <summary>
    /// Method called when no nested subcommand name was specified, or the nested subcommand
    /// could not be found.
    /// </summary>
    /// <param name="commandName">
    /// The name of the nested subcommand, or <see langword="null"/> if none was specified.
    /// </param>
    /// <param name="manager">The <see cref="CommandManager"/> used to create the subcommand.</param>
    /// <param name="possibleMatches">
    /// If <paramref name="commandName"/> was a prefix alias of more than one command, provides
    /// an array of the possible command names and aliases. Otherwise, this parameter is an empty
    /// array.
    /// </param>
    /// <remarks>
    /// <para>
    ///   The base class implementation writes an error message if <paramref name="commandName"/>
    ///   is not <see langword="null"/>, and usage help with a list of all nested subcommands. If
    ///   <paramref name="possibleMatches"/> is not empty, it will write a message listing the
    ///   matching commands instead.
    /// </para>
    /// </remarks>
    protected virtual void OnChildCommandNotFound(CommandManager manager, string? commandName, ImmutableArray<string> possibleMatches)
    {
        if (commandName != null)
        {
            if (!possibleMatches.IsDefaultOrEmpty)
            {
                CommandLineParser.WriteError(manager.Options,
                    manager.Options.StringProvider.AmbiguousCommandPrefixAlias(commandName), manager.Options.ErrorColor, true);

                manager.Options.UsageWriter.WriteCommandAmbiguousPrefixAliasUsage(manager, possibleMatches);
                return;
            }
            else
            {
                CommandLineParser.WriteError(manager.Options, manager.Options.StringProvider.UnknownCommand(commandName),
                    manager.Options.ErrorColor, true);
            }
        }

        manager.WriteUsage();
    }

    /// <summary>
    /// Method called when the <see cref="ParseOptions.DuplicateArguments" qualifyHint="true"/> property is set to
    /// <see cref="ErrorMode.Warning" qualifyHint="true"/> and a duplicate argument value was encountered.
    /// </summary>
    /// <param name="argument">The duplicate argument.</param>
    /// <param name="newValue">The new value for the argument.</param>
    /// <returns>
    /// <see langword="true"/> to use the new value for the argument; <see langword="false"/> to
    /// keep the old value. The base class implementation always returns <see langword="true"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   The base class implementation writes a warning to the <see cref="ParseOptions.Error" qualifyHint="true"/>
    ///   writer.
    /// </para>
    /// <para>
    ///   This method will not be called if the nested subcommand uses the <see cref="ICommandWithCustomParsing"/>
    ///   interface.
    /// </para>
    /// </remarks>
    protected virtual bool OnDuplicateArgumentWarning(CommandLineArgument argument, ReadOnlyMemory<char>? newValue)
    {
        var parser = argument.Parser;
        var warning = parser.StringProvider.DuplicateArgumentWarning(argument.ArgumentName);
        CommandLineParser.WriteError(parser.Options, warning, parser.Options.WarningColor);
        return true;
    }

    /// <summary>
    /// Function called after parsing, on success, cancellation, and failure.
    /// </summary>
    /// <param name="parser">
    /// The <see cref="CommandLineParser"/> instance for the nested subcommand, or <see langword="null"/>
    /// if the nested subcommand used the <see cref="ICommandWithCustomParsing"/> interface.
    /// </param>
    /// <param name="childCommand">
    /// The created subcommand class, or <see langword="null"/> if a failure or cancellation was
    /// encountered.
    /// </param>
    /// <remarks>
    /// <para>
    ///   The base class implementation writes any error message, and usage help for the nested
    ///   subcommand if applicable. On success, or for nested subcommands using the
    ///   <see cref="ICommandWithCustomParsing"/> interface, it does nothing.
    /// </para>
    /// </remarks>
    protected virtual void OnAfterParsing(CommandLineParser? parser, ICommand? childCommand)
    {
        if (parser == null)
        {
            return;
        }

        var helpMode = UsageHelpRequest.Full;
        if (parser.ParseResult.LastException != null)
        {
            CommandLineParser.WriteError(parser.Options, parser.ParseResult.LastException.Message, parser.Options.ErrorColor, true);
            helpMode = parser.Options.ShowUsageOnError;
        }

        if (parser.ParseResult.HelpRequested)
        {
            parser.Options.UsageWriter.WriteParserUsage(parser, helpMode);
        }
    }
}
