namespace Ookii.CommandLine;

/// <summary>
/// Indicates how the <see cref="LineWrappingTextWriter"/> class will wrap text at the maximum
/// line length.
/// </summary>
/// <seealso cref="LineWrappingTextWriter.Wrapping"/>
public enum WrappingMode
{
    /// <summary>
    /// The text will not be wrapped at the maximum line length.
    /// </summary>
    Disabled,
    /// <summary>
    /// The text will be white-space wrapped at the maximum line length, and if there is no
    /// suitable white-space location to wrap the text, it will be wrapped at the line length.
    /// </summary>
    Enabled,
    /// <summary>
    /// The text will be white-space wrapped at the maximum line length. If there is no suitable
    /// white-space location to wrap the text, the line will not be wrapped.
    /// </summary>
    EnabledNoForce
}
