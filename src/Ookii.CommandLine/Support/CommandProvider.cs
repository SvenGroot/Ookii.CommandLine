using Ookii.CommandLine.Commands;
using System.Collections.Generic;

namespace Ookii.CommandLine.Support;

/// <summary>
/// A source of commands for the <see cref="CommandManager"/>.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedCommandManagerAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public abstract class CommandProvider
{
    /// <summary>
    /// Gets the kind of command provider.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="ProviderKind"/> enumeration.
    /// </value>
    public virtual ProviderKind Kind => ProviderKind.Unknown;

    /// <summary>
    /// Gets all the commands supported by this provider.
    /// </summary>
    /// <param name="manager">The <see cref="CommandManager"/> that the commands belong to.</param>
    /// <returns>
    /// A list of <see cref="CommandInfo"/> instances for the commands, in arbitrary order.
    /// </returns>
    public abstract IEnumerable<CommandInfo> GetCommandsUnsorted(CommandManager manager);

    /// <summary>
    /// Gets the application description.
    /// </summary>
    /// <returns>The application description, or <see langword="null"/> if there is none.</returns>
    public abstract string? GetApplicationDescription();
}
