#if NET7_0_OR_GREATER

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Defines a mechanism for creating an instance of the <see cref="CommandManager"/> class for a
/// command provider.
/// </summary>
/// <remarks>
/// <note>
///   This type is only available when using .Net 7 or later.
/// </note>
/// <para>
///   This interface is automatically implemented on a class (on .Net 7 and later only) when the
///   <see cref="GeneratedCommandProviderAttribute"/> is used.
/// </para>
/// </remarks>
public interface ICommandProvider
{
    /// <summary>
    /// Creates a command manager using the class that implements this interface as a provider.
    /// </summary>
    /// <param name="options">
    /// The <see cref="CommandOptions"/> to use, or <see langword="null"/> to use the default options.
    /// </param>
    /// <returns>An instance of the <see cref="CommandManager"/> class.</returns>
    public abstract static CommandManager CreateCommandManager(CommandOptions? options = null);
}

#endif
