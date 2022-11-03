using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates that an argument cannot be used together with other arguments.
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
    public class ProhibitsAttribute : DependencyValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresAttribute"/> class.
        /// </summary>
        /// <param name="argument">The name of the argument that this argument prohibits.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public ProhibitsAttribute(string argument)
            : base(false, argument)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresAttribute"/> class with multiple
        /// dependencies.
        /// </summary>
        /// <param name="arguments">The names of the arguments that this argument prohibits.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="arguments"/> is <see langword="null"/>.
        /// </exception>
        public ProhibitsAttribute(params string[] arguments)
            : base(false, arguments)
        {
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="argument">The argument that was validated.</param>
        /// <param name="value">Not used.</param>
        /// <returns>The error message.</returns>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateProhibitsFailed(argument.MemberName, GetArguments(argument.Parser));

        /// <inheritdoc/>
        protected override string GetUsageHelpCore(CommandLineArgument argument)
            => argument.Parser.StringProvider.ProhibitsUsageHelp(GetArguments(argument.Parser));
    }
}
