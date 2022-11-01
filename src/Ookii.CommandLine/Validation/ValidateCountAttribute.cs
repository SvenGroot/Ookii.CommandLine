using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates whether the number of items for a multi-value or dictionary argument is in the
    /// specified range.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   If the argument is optional and has no value, this validator will not be used, so no
    ///   values is valid regardless of the lower bound specified. If you want the argument to have
    ///   a value, make is a required argument.
    /// </para>
    /// <para>
    ///   This validator will not be checked until all arguments have been parsed.
    /// </para>
    /// <para>
    ///   If this validator is used on an argument that is not a multi-value or dictionary argument,
    ///   validation will always fail.
    /// </para>
    /// </remarks>
    public class ValidateCountAttribute : ArgumentValidationAttribute
    {
        private readonly int _minimum;
        private readonly int _maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateStringLengthAttribute"/> class.
        /// </summary>
        /// <param name="minimum">The inclusive lower bound on the number of elements.</param>
        /// <param name="maximum">The inclusive upper bound on the number of elements.</param>
        public ValidateCountAttribute(int minimum, int maximum = int.MaxValue)
        {
            _minimum = minimum;
            _maximum = maximum;
        }

        /// <inheritdoc/>
        public override ValidationMode Mode => ValidationMode.AfterParsing;

        /// <summary>
        /// Gets the inclusive lower bound on the string length.
        /// </summary>
        /// <value>
        /// The inclusive lower bound on the string length.
        /// </value>
        public int Minimum => _minimum;

        /// <summary>
        /// Get the inclusive upper bound on the string length.
        /// </summary>
        /// <value>
        /// The inclusive upper bound on the string length.
        /// </value>
        public int Maximum => _maximum;

        /// <inheritdoc/>
        public override bool IsValid(CommandLineArgument argument, object? value)
        {
            if (!argument.IsMultiValue)
                return false;

            var count = ((ICollection)argument.Value!).Count;
            return count >= _minimum && count <= _maximum;
        }


        /// <inheritdoc/>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateCountFailed(argument.ArgumentName, this);
    }
}
