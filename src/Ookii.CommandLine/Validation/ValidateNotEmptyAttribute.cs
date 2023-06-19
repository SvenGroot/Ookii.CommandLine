using System;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Validates that the value of an argument is not an empty string.
/// </summary>
/// <remarks>
/// <note>
/// This validator uses the raw string value provided by the user, before type conversion takes
/// place.
/// </note>
/// <para>
///   If the argument is optional, validation is only performed if the argument is specified,
///   so the value may still be <see langword="null"/> if the argument is not supplied, if that
///   is the default value.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class ValidateNotEmptyAttribute : ArgumentValidationWithHelpAttribute
{
    /// <summary>
    /// Gets a value that indicates when validation will run.
    /// </summary>
    /// <value>
    /// <see cref="ValidationMode.BeforeConversion"/>.
    /// </value>
    public override ValidationMode Mode => ValidationMode.BeforeConversion;

    /// <summary>
    /// Determines if the argument is valid.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The raw string argument value provided by the user on the command line.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValid(CommandLineArgument argument, object? value)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    /// <inheritdoc cref="IsValid(CommandLineArgument, object?)"/>
    public override bool? IsSpanValid(CommandLineArgument argument, ReadOnlySpan<char> value)
        => !value.IsEmpty;

    /// <summary>
    /// Gets the error message to display if validation failed.
    /// </summary>
    /// <param name="argument">The argument that was validated.</param>
    /// <param name="value">Not used.</param>
    /// <returns>The error message.</returns>
    public override string GetErrorMessage(CommandLineArgument argument, object? value)
    {
        if (value == null)
        {
            return argument.Parser.StringProvider.NullArgumentValue(argument.ArgumentName);
        }
        else
        {
            return argument.Parser.StringProvider.ValidateNotEmptyFailed(argument.ArgumentName);
        }
    }

    /// <inheritdoc/>
    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => argument.Parser.StringProvider.ValidateNotEmptyUsageHelp();
}
