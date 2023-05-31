using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

public abstract class ParentCommand : ICommandWithCustomParsing, IAsyncCommand
{
    private ICommand? _childCommand;

    protected virtual int FailureExitCode { get; } = 1;

    public void Parse(ReadOnlyMemory<string> args, CommandManager manager)
    {
        var originalParentCommand = manager.Options.ParentCommand;
        manager.Options.ParentCommand = GetType();
        CommandInfo? info;
        try
        {
            var childCommandName = args.Length == 0 ? null : args.Span[0];
            info = childCommandName == null ? null : manager.GetCommand(childCommandName);
            if (info == null)
            {
                OnChildCommandNotFound(childCommandName, manager);
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
            if (parser.Options.DuplicateArguments == ErrorMode.Warning)
            {
                handler = (sender, e) =>
                {
                    OnDuplicateArgumentWarning(e.Argument);
                };

                parser.DuplicateArgument += handler;
            }

            try
            {
                _childCommand = (ICommand?)parser.Parse(args.Span);
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

    public virtual int Run()
    {
        if (_childCommand == null)
        {
            return FailureExitCode;
        }

        return _childCommand.Run();
    }

    public virtual async Task<int> RunAsync()
    {
        if (_childCommand == null)
        {
            return FailureExitCode;
        }

        if (_childCommand is IAsyncCommand asyncCommand)
        {
            return await asyncCommand.RunAsync();
        }

        return _childCommand.Run();
    }

    protected virtual void OnChildCommandNotFound(string? commandName, CommandManager manager)
    {
        manager.WriteUsage();
    }

    protected virtual void OnDuplicateArgumentWarning(CommandLineArgument argument)
    {
        var parser = argument.Parser;
        var warning = parser.StringProvider.DuplicateArgumentWarning(argument.ArgumentName);
        CommandLineParser.WriteError(parser.Options, warning, parser.Options.WarningColor);
    }

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

        if (parser.HelpRequested)
        {
            parser.Options.UsageWriter.WriteParserUsage(parser, helpMode);
        }
    }
}
