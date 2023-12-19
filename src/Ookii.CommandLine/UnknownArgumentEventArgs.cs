using System;

namespace Ookii.CommandLine;

public class UnknownArgumentEventArgs : EventArgs
{
    public UnknownArgumentEventArgs(string token, ReadOnlyMemory<char> name, ReadOnlyMemory<char> value)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Name = name;
        Value = value;
    }

    public string Token { get; }

    public ReadOnlyMemory<char> Name { get; }

    public ReadOnlyMemory<char> Value { get; }

    public bool Ignore { get; set; }

    /// <summary>
    /// Gets a value that indicates whether parsing should be canceled when the event handler
    /// returns.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="CancelMode"/> enumeration. The default value is
    /// <see cref="CancelMode.None" qualifyHint="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the event handler sets this property to a value other than <see cref="CancelMode.None" qualifyHint="true"/>,
    ///   command line processing will stop immediately, returning either <see langword="null"/> or
    ///   an instance of the arguments class according to the <see cref="CancelMode"/> value.
    /// </para>
    /// <para>
    ///   If you want usage help to be displayed after canceling, set the <see cref="CommandLineParser.HelpRequested" qualifyHint="true"/>
    ///   property to <see langword="true"/>.
    /// </para>
    /// </remarks>
    public CancelMode CancelParsing { get; set; }

}
