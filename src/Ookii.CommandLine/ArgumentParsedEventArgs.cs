using System;

namespace Ookii.CommandLine;

/// <summary>
/// Provides data for the <see cref="CommandLineParser.ArgumentParsed" qualifyHint="true"/> event.
/// </summary>
/// <threadsafety static="true" instance="false"/>
public class ArgumentParsedEventArgs : EventArgs
{
    private readonly CommandLineArgument _argument;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentParsedEventArgs"/> class.
    /// </summary>
    /// <param name="argument">The argument that has been parsed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    public ArgumentParsedEventArgs(CommandLineArgument argument)
    {
        _argument = argument ?? throw new ArgumentNullException(nameof(argument));
    }

    /// <summary>
    /// Gets the argument that was parsed.
    /// </summary>
    /// <value>
    /// The <see cref="CommandLineArgument"/> instance for the argument.
    /// </value>
    public CommandLineArgument Argument
    {
        get { return _argument; }
    }

    /// <summary>
    /// Gets a value that indicates whether parsing should be canceled when the event handler
    /// returns.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="CancelMode"/> enumeration. The default value is the
    /// value of the <see cref="CommandLineArgumentAttribute.CancelParsing" qualifyHint="true"/>
    /// property, or the return value of a method argument.
    /// </value>
    /// <remarks>
    /// <para>
    ///   If the event handler sets this property to a value other than <see cref="CancelMode.None" qualifyHint="true"/>,
    ///   command line processing will stop immediately, returning either <see langword="null"/> or
    ///   an instance of the arguments class according to the <see cref="CancelMode"/> value.
    /// </para>
    /// <para>
    ///   If you want usage help to be displayed after canceling, set the value to
    ///   <see cref="CancelMode.AbortWithHelp" qualifyHint="true"/>
    /// </para>
    /// </remarks>
    /// <seealso cref="CommandLineArgument.CancelParsing" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.AutoHelpArgument" qualifyHint="true"/>
    /// <seealso cref="ParseOptions.AutoVersionArgument" qualifyHint="true"/>
    public CancelMode CancelParsing { get; set; }
}
