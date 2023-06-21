using System.Collections;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Validates whether the number of items for a multi-value or dictionary argument is in the
/// specified range.
/// </summary>
/// <remarks>
/// <para>
///   If the argument is optional and has no value, this validator will not be used, so no
///   values is valid regardless of the lower bound specified. If you want the argument to have
///   a value, make is a required argument.
/// </para>
/// <para>
///   This validator will not be checked until all arguments have been parsed.
/// </para>
/// <para>
///   If this validator is used on an argument that is not a multi-value or dictionary argument,
///   validation will always fail.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
public class ValidateCountAttribute : ArgumentValidationWithHelpAttribute
{
    private readonly int _minimum;
    private readonly int _maximum;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateStringLengthAttribute"/> class.
    /// </summary>
    /// <param name="minimum">The inclusive lower bound on the number of elements.</param>
    /// <param name="maximum">The inclusive upper bound on the number of elements.</param>
    public ValidateCountAttribute(int minimum, int maximum = int.MaxValue)
    {
        _minimum = minimum;
        _maximum = maximum;
    }

    /// <summary>
    /// Gets a value that indicates when validation will run.
    /// </summary>
    /// <value>
    /// <see cref="ValidationMode.AfterParsing" qualifyHint="true"/>.
    /// </value>
    public override ValidationMode Mode => ValidationMode.AfterParsing;

    /// <summary>
    /// Gets the inclusive lower bound on the string length.
    /// </summary>
    /// <value>
    /// The inclusive lower bound on the string length.
    /// </value>
    public int Minimum => _minimum;

    /// <summary>
    /// Get the inclusive upper bound on the string length.
    /// </summary>
    /// <value>
    /// The inclusive upper bound on the string length.
    /// </value>
    public int Maximum => _maximum;

    /// <summary>
    /// Determines if the argument's item count is in the range.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must be an instance of
    ///   <see cref="CommandLineArgument.ArgumentType" qualifyHint="true"/>.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValid(CommandLineArgument argument, object? value)
    {
        if (!argument.IsMultiValue)
        {
            return false;
        }

        var count = ((ICollection)argument.Value!).Count;
        return count >= _minimum && count <= _maximum;
    }

    /// <summary>
    /// Gets the error message to display if validation failed.
    /// </summary>
    /// <param name="argument">The argument that was validated.</param>
    /// <param name="value">Not used.</param>
    /// <returns>The error message.</returns>
    public override string GetErrorMessage(CommandLineArgument argument, object? value)
        => argument.Parser.StringProvider.ValidateCountFailed(argument.ArgumentName, this);

    /// <inheritdoc/>
    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => argument.Parser.StringProvider.ValidateCountUsageHelp(this);
}
