using System;

namespace Ookii.CommandLine;

/// <summary>
/// Helper type that allows the creation of a <see cref="Boolean"/> argument that is not a switch.
/// </summary>
/// <remarks>
/// <para>
///   A command line argument using this type will be a regular named argument that requires an
///   explicit value of either "true" or "false". This is contrary to switch arguments, which do
///   not require a value.
/// </para>
/// <para>
///   There is no need to use the <see cref="NonSwitchBoolean"/> structure with positional
///   arguments, as a positional argument cannot be a switch even if its type is
///   <see cref="Boolean"/>.
/// </para>
/// </remarks>
[ValueDescription(nameof(Boolean), ApplyTransform = true)]
public struct NonSwitchBoolean
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonSwitchBoolean"/> structure.
    /// </summary>
    /// <param name="value">The boolean value for the structure.</param>
    public NonSwitchBoolean(bool value) => Value = value;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// Either <see langword="true"/> or <see langword="false"/>.
    /// </value>
    public bool Value { get; set; }

    // TODO: Use custom converter so we can use ReadOnlyMemory<char> instead of string
    public static NonSwitchBoolean Parse(string value) => new NonSwitchBoolean(bool.Parse(value));

    /// <summary>
    /// Converts a <see cref="NonSwitchBoolean"/> structure to a <see cref="Boolean"/>.
    /// </summary>
    /// <param name="value">The <see cref="NonSwitchBoolean"/> to convert.</param>
    /// <returns>The value of the <see cref="Value"/> property.</returns>
    public static implicit operator bool(NonSwitchBoolean value) => value.Value;

    /// <summary>
    /// Converts a <see cref="Boolean"/> to a <see cref="NonSwitchBoolean"/>.
    /// </summary>
    /// <param name="value">The <see cref="Boolean"/> value to convert.</param>
    /// <returns>
    /// A new instance of the <see cref="NonSwitchBoolean"/> structure with the specified value.
    /// </returns>
    public static implicit operator NonSwitchBoolean(bool value) => new(value);
}
