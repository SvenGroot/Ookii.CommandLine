using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Support;

#if NET6_0_OR_GREATER
[RequiresUnreferencedCode("Trimming cannot be used when determining the default converter via reflection.")]
#endif
internal class ReflectionArgumentProvider : ArgumentProvider
{
    public ReflectionArgumentProvider(Type type)
        : base(type, type.GetCustomAttribute<ParseOptionsAttribute>(), type.GetCustomAttributes<ClassValidationAttribute>())
    {
    }

    public override ProviderKind Kind => ProviderKind.Reflection;

    public override string ApplicationFriendlyName
    {
        get
        {
            var attribute = ArgumentsType.GetCustomAttribute<ApplicationFriendlyNameAttribute>() ??
                ArgumentsType.Assembly.GetCustomAttribute<ApplicationFriendlyNameAttribute>();

            return attribute?.Name ?? ArgumentsType.Assembly.GetName().Name ?? string.Empty;
        }
    }

    public override string Description => ArgumentsType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;

    public override bool IsCommand => CommandInfo.IsCommand(ArgumentsType);

    public override object CreateInstance(CommandLineParser parser)
    {
        var inject = ArgumentsType.GetConstructor(new[] { typeof(CommandLineParser) }) != null;
        if (inject)
        {
            return Activator.CreateInstance(ArgumentsType, parser)!;
        }
        else
        {
            return Activator.CreateInstance(ArgumentsType)!;
        }
    }

    public override IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser)
    {
        var properties = ArgumentsType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(CommandLineArgumentAttribute)))
            .Select(p => ReflectionArgument.Create(parser, p));

        var methods = ArgumentsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => Attribute.IsDefined(m, typeof(CommandLineArgumentAttribute)))
            .Select(m => ReflectionArgument.Create(parser, m));

        return properties.Concat(methods);
    }
}
