﻿using System.Threading;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Represents a subcommand that executes asynchronously.
/// </summary>
/// <remarks>
/// <para>
///   This interface adds a <see cref="RunAsync"/> method to the <see cref="ICommand"/>
///   interface, that will be invoked by the
///   <see cref="CommandManager.RunCommandAsync(System.Threading.CancellationToken)" qualifyHint="true"/>
///   method and its overloads. This allows you to write tasks that use asynchronous code.
/// </para>
/// <para>
///   Use the <see cref="AsyncCommandBase"/> class as a base class for your command to get a default
///   implementation of the <see cref="ICommand.Run" qualifyHint="true"/>
/// </para>
/// </remarks>
public interface IAsyncCommand : ICommand
{
    /// <summary>
    /// Runs the command asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests. The default value is
    /// <see cref="CancellationToken.None" qualifyHint="true"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous run operation. The result of the task is the
    /// exit code for the command.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   Typically, your application's <c>Main()</c> method should return the exit code of the
    ///   command that was executed.
    /// </para>
    /// <para>
    ///   This method will only be invoked if you run commands with the
    ///   <see cref="CommandManager.RunCommandAsync(System.Threading.CancellationToken)" qualifyHint="true"/>
    ///   method or one of its overloads. Typically, it's recommended to implement the
    ///   <see cref="ICommand.Run" qualifyHint="true"/> method to invoke this method and wait for
    ///   it. Use the <see cref="AsyncCommandBase"/> class for a default implementation that does
    ///   this.
    /// </para>
    /// <para>
    ///   If a <see cref="CancellationToken"/> was passed to the
    ///   <see cref="CommandManager.RunCommandAsync(CancellationToken)" qualifyHint="true"/> method,
    ///   the <paramref name="cancellationToken"/> parameter will be set to that token. Otherwise,
    ///   the value will be <see cref="CancellationToken.None" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
