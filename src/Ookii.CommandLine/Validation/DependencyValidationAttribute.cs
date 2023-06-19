using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Base class for the <see cref="RequiresAttribute"/> and <see cref="ProhibitsAttribute"/> class.
/// </summary>
public abstract class DependencyValidationAttribute : ArgumentValidationWithHelpAttribute
{
    private readonly string? _argument;
    private readonly string[]? _arguments;
    private readonly bool _requires;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyValidationAttribute"/> class.
    /// </summary>
    /// <param name="requires">
    ///   <see langword="true"/> if this is a requires dependency, or <see langword="false"/>
    ///   for a prohibits dependency.
    /// </param>
    /// <param name="argument">The name of the argument that this argument depends on.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="argument"/> is <see langword="null"/>.
    /// </exception>
    public DependencyValidationAttribute(bool requires, string argument)
    {
        _argument = argument ?? throw new ArgumentNullException(nameof(argument));
        _requires = requires;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyValidationAttribute"/> class with multiple
    /// dependencies.
    /// </summary>
    /// <param name="requires">
    ///   <see langword="true"/> if this is a requires dependency, or <see langword="false"/>
    ///   for a prohibits dependency.
    /// </param>
    /// <param name="arguments">The names of the arguments that this argument depends on.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="arguments"/> is <see langword="null"/>.
    /// </exception>
    public DependencyValidationAttribute(bool requires, params string[] arguments)
    {
        _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        _requires = requires;
    }

    /// <summary>
    /// Gets the names of the arguments that the validator checks against.
    /// </summary>
    /// <value>
    /// An array of argument names.
    /// </value>
    public string[] Arguments => _arguments ?? new[] { _argument! };

    /// <summary>
    /// Gets a value that indicates when validation will run.
    /// </summary>
    /// <value>
    /// <see cref="ValidationMode.AfterParsing"/>.
    /// </value>
    public override ValidationMode Mode => ValidationMode.AfterParsing;

    /// <summary>
    /// Gets the error category used for the <see cref="CommandLineArgumentException"/> when
    /// validation fails.
    /// </summary>
    /// <value>
    /// <see cref="CommandLineArgumentErrorCategory.ValidationFailed"/>.
    /// </value>
    public override CommandLineArgumentErrorCategory ErrorCategory => CommandLineArgumentErrorCategory.DependencyFailed;

    /// <summary>
    /// Determines if the dependencies are met.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">Not used</param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///   One of the argument names in the <see cref="Arguments"/> property refers to an
    ///   argument that doesn't exist.
    /// </exception>
    public sealed override bool IsValid(CommandLineArgument argument, object? value)
    {
        var args = GetArguments(argument.Parser);
        if (_requires)
        {
            return args.All(a => a.HasValue);
        }
        else
        {
            return args.All(a => !a.HasValue);
        }
    }

    /// <summary>
    /// Resolves the argument names in the <see cref="Arguments"/> property to their actual
    /// <see cref="CommandLineArgument"/> property.
    /// </summary>
    /// <param name="parser">The <see cref="CommandLineParser"/> instance.</param>
    /// <returns>A list of the arguments.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="parser"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   One of the argument names in the <see cref="Arguments"/> property refers to an
    ///   argument that doesn't exist.
    /// </exception>
    public IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser)
    {
        if (parser == null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        if (_argument != null)
        {
            var arg = parser.GetArgument(_argument) ?? throw GetUnknownDependencyException(_argument);
            return Enumerable.Repeat(arg, 1);
        }

        Debug.Assert(_arguments != null);
        return _arguments
            .Select(name => parser.GetArgument(name) ?? throw GetUnknownDependencyException(name));
    }

    private InvalidOperationException GetUnknownDependencyException(string name)
    {
        return new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.UnknownDependencyFormat, GetType().Name, name));
    }
}
