﻿using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Ookii.CommandLine.Support;

/// <summary>
/// A base class for argument providers created by the <see cref="GeneratedParserAttribute"/>.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public abstract class GeneratedArgumentProvider : ArgumentProvider
{
    private readonly ApplicationFriendlyNameAttribute? _friendlyNameAttribute;
    private readonly DescriptionAttribute? _descriptionAttribute;
    private readonly UsageFooterAttribute? _usageFooterAttribute;

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
    /// <param name="usageFooter">
    /// The <see cref="UsageFooterAttribute"/> for the arguments type, or <see langword="null"/> if
    /// there is none.
    /// </param>
    protected GeneratedArgumentProvider(
#if NET6_0_OR_GREATER
                                        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)]
#endif
                                        Type argumentsType,
                                        ParseOptionsAttribute? options = null,
                                        IEnumerable<ClassValidationAttribute>? validators = null,
                                        ApplicationFriendlyNameAttribute? friendlyName = null,
                                        DescriptionAttribute? description = null,
                                        UsageFooterAttribute? usageFooter = null)
        : base(argumentsType, options, validators)
    {
        _friendlyNameAttribute = friendlyName;
        _descriptionAttribute = description;
        _usageFooterAttribute = usageFooter;
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

    /// <inheritdoc/>
    public override string UsageFooter => _usageFooterAttribute?.Footer ?? string.Empty;
}
