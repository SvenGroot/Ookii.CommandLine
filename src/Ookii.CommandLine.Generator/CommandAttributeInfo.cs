using Microsoft.CodeAnalysis;

namespace Ookii.CommandLine.Generator;

internal class CommandAttributeInfo
{
    public CommandAttributeInfo(AttributeData data)
    {
        foreach (var named in data.NamedArguments)
        {
            switch (named.Key)
            {
            case nameof(IsHidden):
                IsHidden = (bool)named.Value.Value!;
                break;
            }
        }
    }

    public bool IsHidden { get; }
}
