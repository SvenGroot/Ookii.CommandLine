using System;

namespace Ookii.CommandLine.Terminal;

/// <summary>
/// Handles the lifetime of virtual terminal support.
/// </summary>
/// <remarks>
/// On Windows, this restores the terminal mode to its previous value when disposed or
/// destructed. On other platforms, this does nothing.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
public sealed class VirtualTerminalSupport : IDisposable
{
    private StandardStream? _restoreStream;

    internal VirtualTerminalSupport(bool supported)
    {
        IsSupported = supported;
        GC.SuppressFinalize(this);
    }

    internal VirtualTerminalSupport(StandardStream restoreStream)
    {
        IsSupported = true;
        _restoreStream = restoreStream;
    }

    /// <summary>
    /// Cleans up resources for the <see cref="VirtualTerminalSupport"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This method will disable VT support on Windows if it was enabled by the call to
    ///   <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/> or
    ///   <see cref="VirtualTerminal.EnableVirtualTerminalSequences" qualifyHint="true"/> that
    ///   created this instance.
    /// </para>
    /// </remarks>
    ~VirtualTerminalSupport()
    {
        ResetConsoleMode();
    }

    /// <summary>
    /// Gets a value that indicates whether virtual terminal sequences are supported.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if virtual terminal sequences are supported; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool IsSupported { get; }

    /// <summary>
    /// Cleans up resources for the <see cref="VirtualTerminalSupport"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This method will disable VT support on Windows if it was enabled by the call to
    ///   <see cref="VirtualTerminal.EnableColor" qualifyHint="true"/> or
    ///   <see cref="VirtualTerminal.EnableVirtualTerminalSequences" qualifyHint="true"/> that
    ///   created this instance.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        ResetConsoleMode();
        GC.SuppressFinalize(this);
    }

    private void ResetConsoleMode()
    {
        if (_restoreStream is StandardStream stream)
        {
            VirtualTerminal.SetVirtualTerminalSequences(stream, false);
            _restoreStream = null;
        }
    }
}
