using Ookii.CommandLine.Commands;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Support;

internal class ReflectionArgumentProvider : IArgumentProvider
{
    private readonly Type _type;

    public ReflectionArgumentProvider(Type type)
    {
        _type = type;
    }

    public Type ArgumentsType => _type;

    public ParseOptionsAttribute? OptionsAttribute => _type.GetCustomAttribute<ParseOptionsAttribute>();

    public string ApplicationFriendlyName
    {
        get
        {
            var attribute = _type.GetCustomAttribute<ApplicationFriendlyNameAttribute>() ??
                _type.Assembly.GetCustomAttribute<ApplicationFriendlyNameAttribute>();

            return attribute?.Name ?? _type.Assembly.GetName().Name ?? string.Empty;
        }
    }

    public string Description => _type.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;

    public bool IsCommand => CommandInfo.IsCommand(_type);

    public object CreateInstance(CommandLineParser parser)
    {
        var inject = _type.GetConstructor(new[] { typeof(CommandLineParser) }) != null;
        try
        {
            if (inject)
            {
                return Activator.CreateInstance(_type, parser)!;
            }
            else
            {
                return Activator.CreateInstance(_type)!;
            }
        }
        catch (TargetInvocationException ex)
        {
            throw parser.StringProvider.CreateException(CommandLineArgumentErrorCategory.CreateArgumentsTypeError, ex.InnerException);
        }
    }

    public IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser)
    {
        var properties = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => Attribute.IsDefined(p, typeof(CommandLineArgumentAttribute)))
            .Select(p => ReflectionArgument.Create(parser, p));

        var methods = _type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => Attribute.IsDefined(m, typeof(CommandLineArgumentAttribute)))
            .Select(m => ReflectionArgument.Create(parser, m));

        return properties.Concat(methods);
    }

    public void RunValidators(CommandLineParser parser)
    {
        foreach (var validator in _type.GetCustomAttributes<ClassValidationAttribute>())
        {
            validator.Validate(parser);
        }
    }
}
