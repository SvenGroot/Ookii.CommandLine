using Ookii.CommandLine.Conversion;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ookii.CommandLine.Support;

/// <summary>
/// Represents information about an argument determined by the source generator.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public abstract class GeneratedArgumentBase : CommandLineArgument
{
    private readonly Action<object, object?>? _setProperty;
    private readonly Func<object, object?>? _getProperty;
    private readonly Func<object?, CommandLineParser, CancelMode>? _callMethod;
    private readonly string _defaultValueDescription;
    private readonly string? _defaultKeyDescription;
    private MemberInfo? _member;

    private protected GeneratedArgumentBase(ArgumentCreationInfo info) : base(new ArgumentInfo(info))
    {
        _setProperty = info.SetProperty;
        _getProperty = info.GetProperty;
        _callMethod = info.CallMethod;
        _defaultValueDescription = info.DefaultValueDescription;
        _defaultKeyDescription = info.DefaultKeyDescription;
    }

    /// <inheritdoc/>
    public override MemberInfo? Member => _member ??= (MemberInfo?)Parser.ArgumentsType.GetProperty(MemberName)
        ?? Parser.ArgumentsType.GetMethod(MemberName, BindingFlags.Public | BindingFlags.Static);

    /// <inheritdoc/>
    protected override bool CanSetProperty => _setProperty != null;

    /// <inheritdoc/>
    protected override CancelMode CallMethod(object? value)
    {
        if (_callMethod == null)
        {
            throw new InvalidOperationException(Properties.Resources.InvalidMethodAccess);
        }

        return _callMethod(value, this.Parser);
    }

    /// <inheritdoc/>
    protected override object? GetProperty(object target)
    {
        if (_getProperty == null)
        {
            throw new InvalidOperationException(Properties.Resources.InvalidPropertyAccess);
        }

        return _getProperty(target);
    }

    /// <inheritdoc/>
    protected override void SetProperty(object target, object? value)
    {
        if (_setProperty == null)
        {
            throw new InvalidOperationException(Properties.Resources.InvalidPropertyAccess);
        }

        _setProperty(target, value);
    }

    /// <inheritdoc/>
    protected override string DetermineValueDescriptionForType(Type type)
    {
        Debug.Assert(DictionaryInfo == null ? type == ElementType : (type == DictionaryInfo.KeyType || type == DictionaryInfo.ValueType));
        if (DictionaryInfo != null && type == DictionaryInfo.KeyType)
        {
            return _defaultKeyDescription!;
        }

        return _defaultValueDescription;
    }
}
