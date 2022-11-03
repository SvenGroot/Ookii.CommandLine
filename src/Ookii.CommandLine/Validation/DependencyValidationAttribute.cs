using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Base class for the <see cref="RequiresAttribute"/> and <see cref="ProhibitsAttribute"/> class.
    /// </summary>
    public abstract class DependencyValidationAttribute : ArgumentValidationWithHelpAttribute
    {
        private readonly string? _argument;
        private readonly string[]? _arguments;
        private bool _requires;

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
        /// <param name="value">
        ///   The argument value. If not <see langword="null"/>, this must be an instance of
        ///   <see cref="CommandLineArgument.ArgumentType"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
        /// </returns>
        public sealed override bool IsValid(CommandLineArgument argument, object? value)
        {
            if (_argument != null)
            {
                if (_requires)
                    return argument.Parser.GetArgument(_argument)?.HasValue ?? false;
                else
                    return !argument.Parser.GetArgument(_argument)?.HasValue ?? false;
            }

            Debug.Assert(_arguments != null);
            if (_requires)
            {
                return _arguments
                    .All(name => argument.Parser.GetArgument(name)?.HasValue ?? false);
            }
            else
            {
                return _arguments
                    .Any(name => argument.Parser.GetArgument(name)?.HasValue ?? true);
            }
        }
    }
}
