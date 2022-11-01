using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates that the value of an argument is not <see langword="null"/> or an empty string.
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
    public class ValidateNotNullOrEmptyAttribute : ArgumentValidationAttribute
    {
        /// <inheritdoc/>
        public override ValidationMode Mode => ValidationMode.BeforeConversion;

        /// <inheritdoc/>
        public override bool IsValid(CommandLineArgument argument, object? value)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        /// <inheritdoc/>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
        {
            if (value == null)
                return argument.Parser.StringProvider.NullArgumentValue(argument.ArgumentName);
            else
                return argument.Parser.StringProvider.ValidateNotEmptyFailed(argument.ArgumentName);
        }
    }
}
