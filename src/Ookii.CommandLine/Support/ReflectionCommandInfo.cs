using Ookii.CommandLine.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine.Support;


#if NET6_0_OR_GREATER
[RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
#if NET7_0_OR_GREATER
[RequiresDynamicCode("Consider using the GeneratedParserAttribute.")]
#endif
internal class ReflectionCommandInfo : CommandInfo
{
    private string? _description;

    public ReflectionCommandInfo(Type commandType, CommandAttribute? attribute, CommandManager manager)
        : base(commandType, attribute ?? GetCommandAttributeOrThrow(commandType), manager, GetParentCommand(commandType))
    {
    }

    public override string? Description => _description ??= GetCommandDescription();

    public override bool UseCustomArgumentParsing => CommandType.ImplementsInterface(typeof(ICommandWithCustomParsing));

    public override IEnumerable<AliasAttribute> Aliases => CommandType.GetCustomAttributes<AliasAttribute>();

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

    private static Type? GetParentCommand(Type commandType)
    {
        if (commandType == null)
        {
            throw new ArgumentNullException(nameof(commandType));
        }

        var attribute = commandType.GetCustomAttribute<ParentCommandAttribute>();
        return attribute?.GetParentCommandType();
    }
}
