using System.Collections;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Validates whether the number of items for a multi-value or dictionary argument is in the
/// specified range.
/// </summary>
/// <remarks>
/// <para>
///   If the argument is optional and has no value, this validator will not be used, so zero
///   values is valid regardless of the lower bound specified. If you want zero values to be
///   invalid, make it a required argument.
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
    /// Initializes a new instance of the <see cref="ValidateCountAttribute"/> class.
    /// </summary>
    /// <param name="minimum">The inclusive lower bound on the number of elements.</param>
    /// <param name="maximum">The inclusive upper bound on the number of elements.</param>
    public ValidateCountAttribute(int minimum, int maximum = int.MaxValue)
    {
        _minimum = minimum;
        _maximum = maximum;
    }

    /// <summary>
    /// Gets the inclusive lower bound on the number of elements.
    /// </summary>
    /// <value>
    /// The inclusive lower bound on the number of elements.
    /// </value>
    public int Minimum => _minimum;

    /// <summary>
    /// Get the inclusive upper bound on the number of elements.
    /// </summary>
    /// <value>
    /// The inclusive upper bound on the number of elements.
    /// </value>
    public int Maximum => _maximum;

    /// <summary>
    /// Gets or sets a value that indicates whether the minimum bound will be enforced if the
    /// argument was not specified at all.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if zero values will be considered valid regardless of the value of
    /// the <see cref="Minimum"/> properties; <see langword="false"/> if the minimum bound will be
    /// enforced even if the argument was not specified at all. The default value is
    /// <see langword="true"/>.
    /// </value>
    public bool AllowNoValue { get; set; } = true;

    /// <summary>
    /// Determines if the argument's item count is in the range.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValidPostParsing(CommandLineArgument argument)
    {
        if (argument.MultiValueInfo == null)
        {
            return false;
        }

        if (!argument.HasValue)
        {
            return AllowNoValue || Minimum <= 0;
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
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ValidateCountFailed" qualifyHint="true"/> method
    ///   to customize this message.
    /// </para>
    /// </remarks>
    public override string GetErrorMessage(CommandLineArgument argument, object? value)
        => argument.Parser.StringProvider.ValidateCountFailed(argument.ArgumentName, this);

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    ///   Use a custom <see cref="LocalizedStringProvider"/> class that overrides the
    ///   <see cref="LocalizedStringProvider.ValidateCountUsageHelp" qualifyHint="true"/> method
    ///   to customize this message.
    /// </para>
    /// </remarks>
    protected override string GetUsageHelpCore(CommandLineArgument argument)
        => argument.Parser.StringProvider.ValidateCountUsageHelp(this);
}
