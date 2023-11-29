using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Base class for asynchronous commands that want the <see cref="ICommand.Run" qualifyHint="true"/> method to
/// invoke the <see cref="IAsyncCommand.RunAsync" qualifyHint="true"/> method.
/// </summary>
/// <remarks>
/// <para>
///   This class is provided for convenience for creating asynchronous commands without having to
///   implement the <see cref="ICommand.Run" qualifyHint="true"/> method.
/// </para>
/// <para>
///   If you want to use the cancellation token passed to the
///   <see cref="CommandManager.RunCommandAsync(System.Threading.CancellationToken)" qualifyHint="true"/>
///   method, you should instead implement the <see cref="IAsyncCancelableCommand"/> interface or
///   derive from the <see cref="AsyncCancelableCommandBase"/> class.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public abstract class AsyncCommandBase : IAsyncCommand
{
    /// <summary>
    /// Calls the <see cref="RunAsync"/> method and waits synchronously for it to complete.
    /// </summary>
    /// <returns>The exit code of the command.</returns>
    public virtual int Run() => Task.Run(RunAsync).ConfigureAwait(false).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public abstract Task<int> RunAsync();
}
