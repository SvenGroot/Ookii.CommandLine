using Microsoft.CodeAnalysis;

namespace Ookii.CommandLine.Generator;

internal class CommandLineArgumentAttributeInfo
{
    private readonly bool _isShort;
    private readonly bool _isPositional;

    public CommandLineArgumentAttributeInfo(AttributeData data)
    {
        if (data.ConstructorArguments.Length > 0)
        {
            ArgumentName = data.ConstructorArguments[0].Value as string;
        }

        foreach (var named in data.NamedArguments)
        {
            switch (named.Key)
            {
            case nameof(IsRequired):
                IsRequired = (bool)named.Value.Value!;
                HasIsRequired = true;
                break;

            case nameof(DefaultValue):
                DefaultValue = named.Value.Value;
                break;

            case nameof(Position):
                var position = (int)named.Value.Value!;
                if (position >= 0)
                {
                    Position = position;
                }

                break;

            case nameof(IsPositional):
                _isPositional = (bool)named.Value.Value!;
                break;

            case nameof(IsShort):
                _isShort = (bool)named.Value.Value!;
                ExplicitIsShort = _isShort;
                break;

            case nameof(ShortName):
                ShortName = (char)named.Value.Value!;
                break;

            case nameof(IsLong):
                IsLong = (bool)named.Value.Value!;
                break;

            case nameof(IsHidden):
                IsHidden = (bool)named.Value.Value!;
                break;
            }
        }
    }

    public string? ArgumentName { get; }

    public bool IsRequired { get; }

    public bool HasIsRequired { get; }

    public int? Position { get; }

    public bool IsPositional => _isPositional || Position != null;

    public object? DefaultValue { get; }

    public bool IsShort => _isShort || ShortName != '\0';

    public bool? ExplicitIsShort { get; }

    public char ShortName { get; }

    public bool IsLong { get; } = true;

    public bool IsHidden { get; }
}
