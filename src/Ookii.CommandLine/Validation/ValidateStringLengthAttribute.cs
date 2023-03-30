using System;

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
    public class ValidateStringLengthAttribute : ArgumentValidationWithHelpAttribute
    {
        private readonly int _minimum;
        private readonly int _maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateStringLengthAttribute"/> class.
        /// </summary>
        /// <param name="minimum">The inclusive lower bound on the length.</param>
        /// <param name="maximum">The inclusive upper bound on the length.</param>
        public ValidateStringLengthAttribute(int minimum, int maximum = int.MaxValue)
        {
            _minimum = minimum;
            _maximum = maximum;
            CanValidateSpan = true;
        }

        /// <summary>
        /// Gets a value that indicates when validation will run.
        /// </summary>
        /// <value>
        /// <see cref="ValidationMode.BeforeConversion"/>.
        /// </value>
        public override ValidationMode Mode => ValidationMode.BeforeConversion;

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

        /// <summary>
        /// Determines if the argument's value's length is in the range.
        /// </summary>
        /// <param name="argument">The argument being validated.</param>
        /// <param name="value">
        ///   The raw string value of the argument.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool IsValid(CommandLineArgument argument, object? value)
        {
            var length = (value as string)?.Length ?? 0;
            return length >= _minimum && length <= _maximum;
        }

        /// <inheritdoc cref="IsValid(CommandLineArgument, object?)"/>
        public override bool IsSpanValid(CommandLineArgument argument, ReadOnlySpan<char> value)
        {
            var length = value.Length;
            return length >= _minimum && length <= _maximum;
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="argument">The argument that was validated.</param>
        /// <param name="value">Not used.</param>
        /// <returns>The error message.</returns>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateStringLengthFailed(argument.ArgumentName, this);

        /// <inheritdoc/>
        protected override string GetUsageHelpCore(CommandLineArgument argument)
            => argument.Parser.StringProvider.ValidateStringLengthUsageHelp(this);
    }
}
