using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;


#if NET6_0_OR_GREATER
[RequiresUnreferencedCode("Trimming is not possible when determining commands using reflection. Use the GeneratedCommandProviderAttribute instead.")]
#endif
internal class ReflectionCommandInfo : CommandInfo
{
    private string? _description;

    public ReflectionCommandInfo(Type commandType, CommandAttribute? attribute, CommandManager manager)
        : base(commandType, attribute ?? GetCommandAttributeOrThrow(commandType), manager)
    {
    }

    public override string? Description => _description ??= GetCommandDescription();

    public override bool UseCustomArgumentParsing => CommandType.ImplementsInterface(typeof(ICommandWithCustomParsing));

    public override IEnumerable<string> Aliases => CommandType.GetCustomAttributes<AliasAttribute>().Select(a => a.Alias);

    public static new CommandInfo? TryCreate(Type commandType, CommandManager manager)
    {
        var attribute = GetCommandAttribute(commandType);
        if (attribute == null)
        {
            return null;
        }

        return new ReflectionCommandInfo(commandType, attribute, manager);
    }

    public override CommandLineParser CreateParser()
    {
        if (UseCustomArgumentParsing)
        {
            throw new InvalidOperationException(Properties.Resources.NoParserForCustomParsingCommand);
        }

        return new CommandLineParser(CommandType, Manager.Options);
    }

    public override ICommandWithCustomParsing CreateInstanceWithCustomParsing()
    {
        if (!UseCustomArgumentParsing)
        {
            throw new InvalidOperationException(Properties.Resources.NoCustomParsing);
        }

        return (ICommandWithCustomParsing)Activator.CreateInstance(CommandType)!;
    }

    internal static CommandAttribute? GetCommandAttribute(Type commandType)
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

    private static CommandAttribute GetCommandAttributeOrThrow(Type commandType)
    {
        return GetCommandAttribute(commandType) ??
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                Properties.Resources.TypeIsNotCommandFormat, commandType.FullName));
    }

    private string? GetCommandDescription()
    {
        return CommandType.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }
}
