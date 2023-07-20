using System;

namespace Ookii.CommandLine;

/// <summary>
/// Provides data for the <see cref="CommandLineParser.DuplicateArgument" qualifyHint="true"/> event.
/// </summary>
/// <threadsafety static="true" instance="false"/>
public class DuplicateArgumentEventArgs : EventArgs
{
    private readonly CommandLineArgument _argument;
    private readonly ReadOnlyMemory<char> _memoryValue;
    private readonly string? _stringValue;
    private bool _hasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateArgumentEventArgs"/> class.
    /// </summary>
    /// <param name="argument">The argument that was specified more than once.</param>
    /// <param name="newValue">
    /// The raw new value of the argument, or <see langword="null"/> if the argument has no value.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="argument"/> is <see langword="null"/>
    /// </exception>
    public DuplicateArgumentEventArgs(CommandLineArgument argument, string? newValue)
    {
        _argument = argument ?? throw new ArgumentNullException(nameof(argument));
        _stringValue = newValue;
        _hasValue = newValue != null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateArgumentEventArgs"/> class.
    /// </summary>
    /// <param name="argument">The argument that was specified more than once.</param>
    /// <param name="hasValue"><see langword="true"/> if the argument has a value; otherwise, <see langword="false"/>.</param>
    /// <param name="newValue">The raw new value of the argument.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="argument"/> is <see langword="null"/>
    /// </exception>
    public DuplicateArgumentEventArgs(CommandLineArgument argument, bool hasValue, ReadOnlyMemory<char> newValue)
    {
        _argument = argument ?? throw new ArgumentNullException(nameof(argument));
        _memoryValue = newValue;
        _hasValue = hasValue;
    }

    /// <summary>
    /// Gets the argument that was specified more than once.
    /// </summary>
    /// <value>
    /// The <see cref="CommandLineArgument"/> that was specified more than once.
    /// </value>
    public CommandLineArgument Argument => _argument;

    /// <summary>
    /// Gets the new value that will be assigned to the argument.
    /// </summary>
    /// <value>
    /// The raw string value provided on the command line, before conversion, or <see langword="null"/>
    /// if the argument is a switch argument that was provided without an explicit value.
    /// </value>
    public string? NewValue => _hasValue ? (_stringValue ?? _memoryValue.ToString()) : null;

    /// <summary>
    /// Gets or sets a value that indicates whether the value of the argument should stay
    /// unmodified.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to preserve the current value of the argument, or <see langword="false"/>
    /// to replace it with the value of the <see cref="NewValue"/> property. The default value
    /// is <see langword="false"/>.
    /// </value>
    public bool KeepOldValue { get; set; }
}
