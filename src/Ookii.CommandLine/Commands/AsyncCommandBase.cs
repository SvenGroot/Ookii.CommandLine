using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands
{
    /// <summary>
    /// Base class for asynchronous tasks that want the <see cref="ICommand.Run"/> method to
    /// invoke the <see cref="IAsyncCommand.RunAsync"/> method.
    /// </summary>
    public abstract class AsyncCommandBase : IAsyncCommand
    {
        /// <summary>
        /// Calls the <see cref="RunAsync"/> method and waits synchronously for it to complete.
        /// </summary>
        /// <returns>The exit code of the command.</returns>
        public virtual int Run()
        {
            return Task.Run(RunAsync).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public abstract Task<int> RunAsync();
    }
}
