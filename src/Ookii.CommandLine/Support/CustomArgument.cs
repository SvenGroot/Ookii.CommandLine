using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Support;

// This is just a test placeholder.
public class CustomArgument : CommandLineArgument
{
    private readonly Action<object, object?> _setProperty;

    private CustomArgument(ArgumentInfo info, Action<object, object?> setProperty) : base(info)
    {
        _setProperty = setProperty;
    }

    public static CustomArgument Create(CommandLineParser parser, string name, Type type, Action<object, object?> setProperty)
    {
        var info = new ArgumentInfo()
        {
            Parser = parser,
            ArgumentName = name,
            Kind = ArgumentKind.SingleValue,
            ArgumentType = type,
            ElementTypeWithNullable = type,
            ElementType = type,
            Converter = new StringConverter(),
            Validators = Enumerable.Empty<ArgumentValidationAttribute>(),
        };

        return new CustomArgument(info, setProperty);
    }

    protected override bool CanSetProperty => true;

    protected override bool CallMethod(object? value) => throw new NotImplementedException();
    protected override object? GetProperty(object target) => throw new NotImplementedException();
    protected override void SetProperty(object target, object? value) => _setProperty(target, value);
}
