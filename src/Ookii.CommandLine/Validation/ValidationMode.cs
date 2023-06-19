namespace Ookii.CommandLine.Validation;

/// <summary>
/// Specifies when a derived class of the <see cref="ArgumentValidationAttribute"/> class
/// will run validation.
/// </summary>
public enum ValidationMode
{
    /// <summary>
    /// Validation will occur after the value is converted. The value passed to
    /// the <see cref="ArgumentValidationAttribute.IsValid"/> method is an instance of the
    /// argument's type.
    /// </summary>
    AfterConversion,
    /// <summary>
    /// Validation will occur before the value is converted. The value passed to
    /// the <see cref="ArgumentValidationAttribute.IsValid"/> method is the raw string provided
    /// by the user, and <see cref="CommandLineArgument.Value"/> is not yet set.
    /// </summary>
    BeforeConversion,
    /// <summary>
    /// Validation will occur after all arguments have been parsed. Validators will only be
    /// called on arguments with values, and the value passed to
    /// <see cref="ArgumentValidationAttribute.IsValid"/> is always <see langword="null"/>.
    /// </summary>
    AfterParsing,
}
