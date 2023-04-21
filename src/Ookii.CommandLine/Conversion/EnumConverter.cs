﻿using System;
using System.Globalization;

namespace Ookii.CommandLine.Conversion;

/// <summary>
/// A converter for arguments with enumeration values.
/// </summary>
/// <threadsafety static="true" instance="true"/>
public class EnumConverter : ArgumentConverter
{
    private readonly Type _enumType;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumConverter"/> for the specified enumeration
    /// type.
    /// </summary>
    /// <param name="enumType">The enumeration type.</param>
    public EnumConverter(Type enumType)
    {
        _enumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
    }

    /// <inheritdoc/>
    public override object? Convert(string value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return Enum.Parse(_enumType, value, true);
        }
        catch (ArgumentException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
        catch (OverflowException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
    }

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    public override object? Convert(ReadOnlySpan<char> value, CultureInfo culture, CommandLineArgument argument)
    {
        try
        {
            return Enum.Parse(_enumType, value, true);
        }
        catch (ArgumentException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
        catch (OverflowException ex)
        {
            throw new FormatException(ex.Message, ex);
        }
    }
#endif
}