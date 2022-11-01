using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates that the string length of an argument's value is in the specified range.
    /// </summary>
    /// <remarks>
    /// <note>
    ///   If the argument's type is not <see cref="string"/>, this validator uses the raw string
    ///   value provided by the user, before type conversion takes place.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ValidateStringLengthAttribute : ArgumentValidationAttribute
    {
        private readonly int _minimumLength;
        private readonly int _maximumLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateStringLengthAttribute"/> class.
        /// </summary>
        /// <param name="minimumLength">The inclusive lower bound on the length.</param>
        /// <param name="maximumLength">The inclusive upper bound on the length.</param>
        public ValidateStringLengthAttribute(int minimumLength, int maximumLength = int.MaxValue)
        {
            _minimumLength = minimumLength;
            _maximumLength = maximumLength;
        }

        /// <inheritdoc/>
        public override ValidationMode Mode => ValidationMode.BeforeConversion;

        /// <summary>
        /// Gets the inclusive lower bound on the string length.
        /// </summary>
        /// <value>
        /// The inclusive lower bound on the string length.
        /// </value>
        public int MinimumLength => _minimumLength;

        /// <summary>
        /// Get the inclusive upper bound on the string length.
        /// </summary>
        /// <value>
        /// The inclusive upper bound on the string length.
        /// </value>
        public int MaximumLength => _maximumLength;

        /// <inheritdoc/>
        public override bool IsValid(CommandLineArgument argument, object? value)
        {
            var length = (value as string)?.Length ?? 0;
            return length >= _minimumLength && length <= _maximumLength;
        }

        /// <inheritdoc/>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateStringLengthFailed(argument.ArgumentName, this);
    }
}
