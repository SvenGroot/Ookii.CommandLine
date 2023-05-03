using Microsoft.CodeAnalysis;

namespace Ookii.CommandLine.Generator;

internal class CommandLineArgumentAttributeInfo
{
    public CommandLineArgumentAttributeInfo(AttributeData data)
    {
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
            }
        }
    }

    public bool IsRequired { get; }

    public bool HasIsRequired { get; }

    public int? Position { get; }

    public object? DefaultValue { get; }
}
