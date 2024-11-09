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
public enum TriState : byte
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

/// <summary>
/// Provides extension methods for the <see cref="TriState"/> enumeration.
/// </summary>
/// <threadsafety static="true" instance="false"/>
public static class TriStateExtensions
{
    /// <summary>
    /// Converts the <see cref="TriState"/> value to a <see cref="bool"/> value.
    /// </summary>
    /// <param name="value">The <see cref="TriState"/> value to convert.</param>
    /// <param name="autoValue">
    /// The value to return if <paramref name="value"/> is <see cref="TriState.Auto" qualifyHint="true"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is <see cref="TriState.True" qualifyHint="true"/>;
    /// <see langword="false"/> if <paramref name="value"/> is <see cref="TriState.False" qualifyHint="true"/>;
    /// and <paramref name="autoValue"/> if <paramref name="value"/> is <see cref="TriState.Auto" qualifyHint="true"/>.
    /// </returns>
    public static bool ToBoolean(this TriState value, bool autoValue) => value switch
    {
        TriState.True => true,
        TriState.False => false,
        _ => autoValue,
    };
}
