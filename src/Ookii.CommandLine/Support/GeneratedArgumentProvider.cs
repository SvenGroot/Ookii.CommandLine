using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Ookii.CommandLine.Support;

/// <summary>
/// A base class for argument providers created by the <see cref="GeneratedParserAttribute"/>.
/// This type is for internal use only and should not be used by your code.
/// </summary>
public abstract class GeneratedArgumentProvider : ArgumentProvider
{
    private readonly ApplicationFriendlyNameAttribute? _friendlyNameAttribute;
    private readonly DescriptionAttribute? _descriptionAttribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedArgumentProvider"/> class.
    /// </summary>
    /// <param name="argumentsType">The type that will hold the argument values.</param>
    /// <param name="options">
    /// The <see cref="ParseOptionsAttribute"/> for the arguments type, or <see langword="null"/> if
    /// there is none.
    /// </param>
    /// <param name="validators">The class validators for the arguments type.</param>
    /// <param name="friendlyName">
    /// The <see cref="ApplicationFriendlyNameAttribute"/> for the arguments type, or
    /// <see langword="null"/> if there is none.
    /// </param>
    /// <param name="description">
    /// The <see cref="DescriptionAttribute"/> for the arguments type, or <see langword="null"/> if
    /// there is none.
    /// </param>
    protected GeneratedArgumentProvider(Type argumentsType,
                                        ParseOptionsAttribute? options = null,
                                        IEnumerable<ClassValidationAttribute>? validators = null,
                                        ApplicationFriendlyNameAttribute? friendlyName = null,
                                        DescriptionAttribute? description = null)
        : base(argumentsType, options, validators)
    {
        _friendlyNameAttribute = friendlyName;
        _descriptionAttribute = description;
    }

    /// <inheritdoc/>
    public override ProviderKind Kind => ProviderKind.Generated;

    /// <inheritdoc/>
    public override string ApplicationFriendlyName
        => _friendlyNameAttribute?.Name ?? ArgumentsType.Assembly.GetCustomAttribute<ApplicationFriendlyNameAttribute>()?.Name
            ?? ArgumentsType.Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? ArgumentsType.Assembly.GetName().Name
            ?? string.Empty;

    /// <inheritdoc/>
    public override string Description => _descriptionAttribute?.Description ?? string.Empty;
}
