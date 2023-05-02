﻿using Microsoft.CodeAnalysis;

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
                break;

            case nameof(DefaultValue):
                DefaultValue = named.Value.Value;
                break;
            }
        }
    }

    public bool IsRequired { get; }

    public object? DefaultValue { get; }
}
