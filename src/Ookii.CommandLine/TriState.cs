namespace Ookii.CommandLine;

/// <summary>
/// Represents a value that can be either true, false or automatically determined.
/// </summary>
/// <remarks>
/// <value>
///   This enumeration is equivalent to a <see cref="bool"/> value, with an additional option that
///   indicates the value should be automatically determined based on some criteria.
/// </value>
/// </remarks>
public enum TriState
{
    /// <summary>
    /// The value should be automatically determined to be either <see langword="true"/> or
    /// <see langword="false"/>.
    /// </summary>
    Auto,
    /// <summary>
    /// Represents a <see langword="true"/> value.
    /// </summary>
    True,
    /// <summary>
    /// Represents a <see langword="false"/> value.
    /// </summary>
    False,
}
