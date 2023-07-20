namespace Ookii.CommandLine;

/// <summary>
/// Specifies what kind of argument an instance of the <see cref="CommandLineArgument"/> class
/// represents.
/// </summary>
public enum ArgumentKind
{
    /// <summary>
    /// A regular argument that can have only a single value.
    /// </summary>
    SingleValue,
    /// <summary>
    /// A multi-value argument.
    /// </summary>
    MultiValue,
    /// <summary>
    /// A dictionary argument, which is a multi-value argument where the values are key/value
    /// pairs with unique keys.
    /// </summary>
    Dictionary,
    /// <summary>
    /// An argument that invokes a method when specified.
    /// </summary>
    Method
}
