namespace Ookii.CommandLine.Support;

/// <summary>
/// Specifies the kind of provider that was the source of the arguments or subcommands.
/// </summary>
public enum ProviderKind
{
    /// <summary>
    /// A custom provider that was not part of Ookii.CommandLine.
    /// </summary>
    Unknown,
    /// <summary>
    /// An provider that uses reflection.
    /// </summary>
    Reflection,
    /// <summary>
    /// An provider that uses source generation. These are typically created using the
    /// <see cref="GeneratedParserAttribute"/> and <see cref="Commands.GeneratedCommandManagerAttribute"/>
    /// attributes.
    /// </summary>
    Generated
}
