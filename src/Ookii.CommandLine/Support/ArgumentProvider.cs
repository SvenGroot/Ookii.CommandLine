using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Ookii.CommandLine.Support;

/// <summary>
/// A source of arguments for the <see cref="CommandLineParser"/>.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public abstract class ArgumentProvider
{
    private readonly IEnumerable<ClassValidationAttribute> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentProvider"/> class.
    /// </summary>
    /// <param name="argumentsType">The type that will hold the argument values.</param>
    /// <param name="options">
    /// The <see cref="ParseOptionsAttribute"/> for the arguments type, or <see langword="null"/>
    /// if there is none.
    /// </param>
    /// <param name="validators">The class validators for the arguments type.</param>
    protected ArgumentProvider(
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)]
#endif
        Type argumentsType, ParseOptionsAttribute? options, IEnumerable<ClassValidationAttribute>? validators)
    {
        ArgumentsType = argumentsType ?? throw new ArgumentNullException(nameof(argumentsType));
        OptionsAttribute = options;
        _validators = validators ?? Enumerable.Empty<ClassValidationAttribute>();
    }

    /// <summary>
    /// Gets the kind of argument provider.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="ProviderKind"/> enumeration.
    /// </value>
    public virtual ProviderKind Kind => ProviderKind.Unknown;

    /// <summary>
    /// Gets the type that will hold the argument values.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the class that will hold the argument values.
    /// </value>
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)]
#endif
    public Type ArgumentsType { get; }

    /// <summary>
    /// Gets the friendly name of the application.
    /// </summary>
    /// <value>
    /// The friendly name of the application.
    /// </value>
    public abstract string ApplicationFriendlyName { get; }

    /// <summary>
    /// Gets a description that is used when generating usage information.
    /// </summary>
    /// <value>
    /// The description of the command line application.
    /// </value>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the <see cref="ParseOptionsAttribute"/> that was applied to the arguments type.
    /// </summary>
    /// <value>
    /// An instance of the <see cref="ParseOptionsAttribute"/> class, or <see langword="null"/> if
    /// the attribute was not present.
    /// </value>
    public ParseOptionsAttribute? OptionsAttribute { get; }

    /// <summary>
    /// Gets a value that indicates whether this arguments type is a subcommand.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the arguments type is a subcommand; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool IsCommand { get; }

    /// <summary>
    /// Gets the arguments defined by the arguments type.
    /// </summary>
    /// <param name="parser">The <see cref="CommandLineParser"/> that is parsing the arguments.</param>
    /// <returns>An enumeration of <see cref="CommandLineArgument"/> instances.</returns>
    public abstract IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser);

    /// <summary>
    /// Runs the class validators for the arguments type.
    /// </summary>
    /// <param name="parser">The <see cref="CommandLineParser"/> that is parsing the arguments.</param>
    /// <exception cref="CommandLineArgumentException">
    /// One of the validators failed.
    /// </exception>
    public void RunValidators(CommandLineParser parser)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        foreach (var validator in _validators)
        {
            validator.Validate(parser);
        }
    }

    /// <summary>
    /// Creates an instance of the arguments type.
    /// </summary>
    /// <param name="parser">The <see cref="CommandLineParser"/> that is parsing the arguments.</param>
    /// <param name="requiredPropertyValues">
    /// An array with the values of any arguments backed by required properties, or <see langword="null"/>
    /// if there are no required properties, or if the <see cref="Kind"/> property equals
    /// <see cref="ProviderKind.Reflection" qualifyHint="true"/>.
    /// </param>
    /// <returns>An instance of the type indicated by the <see cref="ArgumentsType"/> property.</returns>
    public abstract object CreateInstance(CommandLineParser parser, object?[]? requiredPropertyValues);
}
