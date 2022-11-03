using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{

    /// <summary>
    /// Validates that the value of an argument is not <see langword="null"/>, an empty string, or
    /// a string containing only white-space characters.
    /// </summary>
    /// <remarks>
    /// <note>
    ///   If the argument's type is not <see cref="string"/>, this validator uses the raw string
    ///   value provided by the user, before type conversion takes place.
    /// </note>
    /// <para>
    ///   The value can only be <see langword="null"/> before type conversion if the argument is
    ///   a switch and no explicit value was provided.
    /// </para>
    /// <para>
    ///   If the argument is optional, validation is only performed if the argument is specified,
    ///   so the value may still be <see langword="null"/> if the argument is not supplied, if that
    ///   is the default value.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ValidateNotNullOrWhiteSpaceAttribute : ArgumentValidationWithHelpAttribute
    {
        /// <summary>
        /// Gets a value that indicates when validation will run.
        /// </summary>
        /// <value>
        /// <see cref="ValidationMode.BeforeConversion"/>.
        /// </value>
        public override ValidationMode Mode => ValidationMode.BeforeConversion;

        /// <summary>
        /// Determines if the argument's value is not null or only white-space characters.
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
            return !string.IsNullOrWhiteSpace(value as string);
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="argument">The argument that was validated.</param>
        /// <param name="value">Not used.</param>
        /// <returns>The error message.</returns>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
        {
            if (value == null)
                return argument.Parser.StringProvider.NullArgumentValue(argument.ArgumentName);
            else
                return argument.Parser.StringProvider.ValidateNotWhiteSpaceFailed(argument.ArgumentName);
        }

        /// <inheritdoc/>
        protected override string GetUsageHelpCore(CommandLineArgument argument)
            => argument.Parser.StringProvider.ValidateNotWhiteSpaceUsageHelp();

    }
}
