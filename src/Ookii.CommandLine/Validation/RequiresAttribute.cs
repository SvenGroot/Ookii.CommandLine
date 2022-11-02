using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates that an argument can only be used together with other arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This attribute can be used to indicate that an argument can only be used in combination
    ///   with one or more other attributes. If one or more of the dependencies does not have
    ///   a value, validation will fail.
    /// </para>
    /// <para>
    ///   This validator will not be checked until all arguments have been parsed.
    /// </para>
    /// <para>
    ///   If validation fails, a <see cref="CommandLineArgumentException"/> is thrown with the
    ///   error category set to <see cref="CommandLineArgumentErrorCategory.DependencyFailed"/>.
    /// </para>
    /// <para>
    ///   Names of arguments that are dependencies are not validated when the attribute is created.
    ///   If one of the specified arguments does not exist, validation will always fail.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class RequiresAttribute : ArgumentValidationAttribute
    {
        private string? _dependency;
        private string[]? _dependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresAttribute"/> class.
        /// </summary>
        /// <param name="dependency">The name of the argument that this argument depends on.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="dependency"/> is <see langword="null"/>.
        /// </exception>
        public RequiresAttribute(string dependency)
        {
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresAttribute"/> class with multiple
        /// dependencies.
        /// </summary>
        /// <param name="dependencies">The names of the arguments that this argument depends on.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="dependencies"/> is <see langword="null"/>.
        /// </exception>
        public RequiresAttribute(params string[] dependencies)
        {
            _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        }

        /// <summary>
        /// Gets the names of the arguments that the argument with this attribute depends upon.
        /// </summary>
        /// <value>
        /// An array of argument names.
        /// </value>
        public string[] RequiredArguments => _dependencies ?? new[] { _dependency! };

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
        public override bool IsValid(CommandLineArgument argument, object? value)
        {
            if (_dependency != null)
                return argument.Parser.GetArgument(_dependency)?.HasValue ?? false;

            Debug.Assert(_dependencies != null);
            return _dependencies
                .All(name => argument.Parser.GetArgument(name)?.HasValue ?? false);
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="argument">The argument that was validated.</param>
        /// <param name="value">Not used.</param>
        /// <returns>The error message.</returns>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateRequiresFailed(argument.MemberName, RequiredArguments);
    }
}
