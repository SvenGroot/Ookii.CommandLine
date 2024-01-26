using System.Threading;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Represents a subcommand that executes asynchronously and supports cancellation.
/// </summary>
/// <remarks>
/// <para>
///   This interface adds a <see cref="CancellationToken"/> property to the
///   <see cref="IAsyncCommand"/> interface. The
///   <see cref="CommandManager.RunCommandAsync(CancellationToken)" qualifyHint="true"/> method
///   and its overloads will set this property prior to calling the
///   <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/> method.
/// </para>
/// <para>
///   Use the <see cref="AsyncCommandBase"/> class as a base class for your command to get a default
///   implementation of the <see cref="ICommand.Run" qualifyHint="true"/> method and the <see
///   cref="CancellationToken"/> property.
/// </para>
/// </remarks>
public interface IAsyncCancelableCommand : IAsyncCommand
{
    /// <summary>
    /// Gets or sets the cancellation token that can be used by the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/>
    /// method.
    /// </summary>
    /// <value>
    /// A <see cref="CancellationToken"/> instance, or <see cref="CancellationToken.None" qualifyHint="true"/>
    /// if none was set.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If a <see cref="CancellationToken"/> was passed to the
    ///   <see cref="CommandManager.RunCommandAsync(CancellationToken)" qualifyHint="true"/> method,
    ///   this property will be set to that token prior to the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/>
    ///   method being called.
    /// </para>
    /// </remarks>
    public CancellationToken CancellationToken { get; set; }
}
