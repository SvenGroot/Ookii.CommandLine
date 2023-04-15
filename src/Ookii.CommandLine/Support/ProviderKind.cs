namespace Ookii.CommandLine.Support;

/// <summary>
/// Specifies the kind of provider that was the source of the arguments.
/// </summary>
public enum ProviderKind
{
    /// <summary>
    /// A custom provider that was not part of Ookii.CommandLine.
    /// </summary>
    Unknown,
    /// <summary>
    /// An argument provider that uses reflection.
    /// </summary>
    Reflection,
    /// <summary>
    /// An argument provider that uses code generation.
    /// </summary>
    Generated
}
