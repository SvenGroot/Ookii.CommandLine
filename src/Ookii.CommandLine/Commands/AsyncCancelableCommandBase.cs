using System.Threading;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Base class for asynchronous commands with cancellation support that want the
/// <see cref="ICommand.Run" qualifyHint="true"/> method to invoke the
/// <see cref="IAsyncCancelableCommand.RunAsync" qualifyHint="true"/> method.
/// </summary>
/// <remarks>
/// <para>
///   This class is provided for convenience for creating asynchronous commands without having to
///   implement the <see cref="ICommand.Run" qualifyHint="true"/> method.
/// </para>
/// <para>
///   This class implements the <see cref="IAsyncCancelableCommand"/> interface, which can use the
///   cancellation token passed to the <see cref="CommandManager.RunCommandAsync(CancellationToken)" qualifyHint="true"/>
///   method.
/// </para>
/// <para>
///   If you do not need the cancellation token, you can implement the <see cref="IAsyncCommand"/>
///   interface or derive from the <see cref="AsyncCommandBase"/> class instead.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public abstract class AsyncCancelableCommandBase : IAsyncCancelableCommand
{
    /// <summary>
    /// Calls the <see cref="RunAsync"/> method and waits synchronously for it to complete.
    /// </summary>
    /// <returns>The exit code of the command.</returns>
    public virtual int Run() => Task.Run(() => RunAsync(default)).ConfigureAwait(false).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public abstract Task<int> RunAsync(CancellationToken token);
}
