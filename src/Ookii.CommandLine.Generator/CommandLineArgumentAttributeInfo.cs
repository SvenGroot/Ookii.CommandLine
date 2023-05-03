﻿using Microsoft.CodeAnalysis;

namespace Ookii.CommandLine.Generator;

internal class CommandLineArgumentAttributeInfo
{
    private readonly bool _isShort;

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

            case nameof(IsShort):
                _isShort = (bool)named.Value.Value!;
                break;

            case nameof(ShortName):
                ShortName = (char)named.Value.Value!;
                break;

            case nameof(IsLong):
                IsLong = (bool)named.Value.Value!;
                break;
            }
        }
    }

    public bool IsRequired { get; }

    public bool HasIsRequired { get; }

    public int? Position { get; }

    public object? DefaultValue { get; }

    public bool IsShort => _isShort || ShortName != '\0';

    public char ShortName { get; }

    public bool IsLong { get; } = true;
}