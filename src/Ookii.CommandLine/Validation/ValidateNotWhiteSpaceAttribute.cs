using System;

namespace Ookii.CommandLine.Validation;


/// <summary>
/// Validates that the value of an argument is not an empty string, or a string containing only
/// white-space characters.
/// </summary>
/// <remarks>
/// <note>
///   This validator uses the raw string value provided by the user, before type conversion takes
///   place.
/// </note>
/// <para>
///   If the argument is optional, validation is only performed if the argument is specified,
///   so the value may still be an empty or white-space-only string if the argument is not supplied,
///   if that is the default value.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class ValidateNotWhiteSpaceAttribute : ArgumentValidationWithHelpAttribute
{
    /// <summary>
    /// Determines if the argument's value is not an empty string, or contains only white-space
    /// characters.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The raw string argument value.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValidPreConversion(CommandLineArgument argument, ReadOnlyMemory<char> value)
        => !value.Span.IsWhiteSpace();

    /// <summary>
    /// Gets the error message to display if validation failed.
    /// </summary>
    /// <param name="argument">The argument that was validated.</param>
    /// <param name="value">Not used.</param>
    /// <returns>The error message.</returns>
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ValidateNotWhiteSpaceFailed" qualifyHint="true"/>
    ///   method to customize this message.
    /// </para>
    /// </remarks>
    public override string GetErrorMessage(CommandLineArgument argument, object? value)
    {
        if (value == null)
        {
            return argument.Parser.StringProvider.NullArgumentValue(argument.ArgumentName);
        }
        else
        {
            return argument.Parser.StringProvider.ValidateNotWhiteSpaceFailed(argument.ArgumentName);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ValidateNotWhiteSpaceUsageHelp" qualifyHint="true"/> method
    ///   to customize this message.
    /// </para>
    /// </remarks>
    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => argument.Parser.StringProvider.ValidateNotWhiteSpaceUsageHelp();

}
