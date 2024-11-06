
namespace Ookii.CommandLine;

/// <summary>
/// Specifies the kind of error that occurred while parsing arguments.
/// </summary>
public enum CommandLineArgumentErrorCategory
{
    /// <summary>
    /// The category was not specified.
    /// </summary>
    Unspecified,
    /// <summary>
    /// The argument value supplied could not be converted to the type of the argument.
    /// </summary>
    ArgumentValueConversion,
    /// <summary>
    /// The argument name supplied does not name a known argument.
    /// </summary>
    UnknownArgument,
    /// <summary>
    /// An argument name was supplied, but without an accompanying value.
    /// </summary>
    MissingNamedArgumentValue,
    /// <summary>
    /// An argument was supplied more than once.
    /// </summary>
    DuplicateArgument,
    /// <summary>
    /// Too many positional arguments were supplied.
    /// </summary>
    TooManyArguments,
    /// <summary>
    /// Not all required arguments were supplied.
    /// </summary>
    MissingRequiredArgument,
    /// <summary>
    /// Invalid value for a dictionary argument; typically the result of a duplicate key.
    /// </summary>
    InvalidDictionaryValue,
    /// <summary>
    /// An error occurred creating an instance of the arguments type (e.g. the constructor threw an exception).
    /// </summary>
    CreateArgumentsTypeError,
    /// <summary>
    /// An error occurred applying the value of the argument (e.g. the property set accessor threw an exception).
    /// </summary>
    ApplyValueError,
    /// <summary>
    /// An argument value was <see langword="null"/> after conversion from a string, and the argument type is a value
    /// type or (in .Net 6.0 and later) a non-nullable reference type.
    /// </summary>
    NullArgumentValue,
    /// <summary>
    /// A combined short argument contains an argument that is not a switch.
    /// </summary>
    CombinedShortNameNonSwitch,
    /// <summary>
    /// An instance of a class derived from the <see cref="Validation.ArgumentValidationAttribute"/>
    /// class failed to validate the argument.
    /// </summary>
    ValidationFailed,
    /// <summary>
    /// An argument failed a dependency check performed by the <see cref="Validation.RequiresAttribute"/>
    /// or the <see cref="Validation.ProhibitsAttribute"/> class.
    /// </summary>
    DependencyFailed,
    /// <summary>
    /// The provided argument name was a prefix of more than one argument name or alias.
    /// </summary>
    AmbiguousPrefixAlias
}
