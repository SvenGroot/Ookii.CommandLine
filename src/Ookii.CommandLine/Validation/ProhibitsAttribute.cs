using System;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Validates that an argument is not used together with other arguments.
/// </summary>
/// <remarks>
/// <para>
///   This attribute can be used to indicate that an argument can only be used when one or more
///   other arguments are not used. If one or more of the prohibited arguments has a value,
///   validation will fail.
/// </para>
/// <para>
///   This validator will not be checked until all arguments have been parsed.
/// </para>
/// <para>
///   If validation fails, a <see cref="CommandLineArgumentException"/> is thrown with the
///   error category set to <see cref="CommandLineArgumentErrorCategory.DependencyFailed" qualifyHint="true"/>.
/// </para>
/// <para>
///   The names of arguments that are dependencies are not validated when the attribute is created.
///   If one of the specified arguments does not exist, an exception will be thrown during
///   validation.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class ProhibitsAttribute : DependencyValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProhibitsAttribute"/> class.
    /// </summary>
    /// <param name="argument">The name of the argument that this argument prohibits.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="argument"/> is <see langword="null"/>.
    /// </exception>
    public ProhibitsAttribute(string argument)
        : base(false, argument)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProhibitsAttribute"/> class with multiple
    /// prohibited arguments.
    /// </summary>
    /// <param name="arguments">The names of the arguments that this argument prohibits.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="arguments"/> is <see langword="null"/>.
    /// </exception>
    public ProhibitsAttribute(params string[] arguments)
        : base(false, arguments)
    {
    }

    /// <summary>
    /// Gets the error message to display if validation failed.
    /// </summary>
    /// <param name="argument">The argument that was validated.</param>
    /// <param name="value">Not used.</param>
    /// <returns>The error message.</returns>
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ValidateProhibitsFailed" qualifyHint="true"/> method
    ///   to customize this message.
    /// </para>
    /// </remarks>
    public override string GetErrorMessage(CommandLineArgument argument, object? value)
        => argument.Parser.StringProvider.ValidateProhibitsFailed(argument.MemberName, GetArguments(argument.Parser));

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ProhibitsUsageHelp" qualifyHint="true"/> method
    ///   to customize this message.
    /// </para>
    /// </remarks>
    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => argument.Parser.StringProvider.ProhibitsUsageHelp(GetArguments(argument.Parser));
}
