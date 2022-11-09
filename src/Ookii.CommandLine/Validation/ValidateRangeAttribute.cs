using System;
using System.ComponentModel;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates whether an argument value is in the specified range.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This attribute can only be used with argument's whose type implements <see cref="IComparable"/>.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ValidateRangeAttribute : ArgumentValidationWithHelpAttribute
    {
        private readonly object? _minimum;
        private readonly object? _maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateRangeAttribute"/> class.
        /// </summary>
        /// <param name="minimum">
        ///   The inclusive lower bound of the range, or <see langword="null"/> if
        ///   the range has no lower bound.
        /// </param>
        /// <param name="maximum">
        ///   The inclusive upper bound of the range, or <see langword="null"/> if
        ///   the range has no upper bound.
        /// </param>
        /// <remarks>
        /// <para>
        ///   When not <see langword="null"/>, both <paramref name="minimum"/> and <paramref name="maximum"/>
        ///   must be an instance of the argument type, or a type that can be converted to the
        ///   argument type using its <see cref="TypeConverter"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <paramref name="minimum"/> and <paramref name="maximum"/> are both <see langword="null"/>.
        /// </exception>
        public ValidateRangeAttribute(object? minimum, object? maximum)
        {
            if (minimum == null && maximum == null)
            {
                throw new ArgumentException(Properties.Resources.MinMaxBothNull);
            }

            _minimum = minimum;
            _maximum = maximum;
        }

        /// <summary>
        /// Gets the inclusive lower bound of the range.
        /// </summary>
        /// <value>
        /// The inclusive lower bound of the range, or <see langword="null"/> if
        /// the range has no lower bound.
        /// </value>
        public virtual object? Minimum => _minimum;

        /// <summary>
        /// Gets the inclusive upper bound of the range.
        /// </summary>
        /// <value>
        /// The inclusive upper bound of the range, or <see langword="null"/> if
        /// the range has no upper bound.
        /// </value>
        public virtual object? Maximum => _maximum;

        /// <summary>
        /// Determines if the argument's value is in the range.
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
            var min = (IComparable?)argument.ConvertToArgumentTypeInvariant(Minimum);
            var max = (IComparable?)argument.ConvertToArgumentTypeInvariant(Maximum);

            if (min != null && min.CompareTo(value) > 0)
            {
                return false;
            }

            if (max != null && max.CompareTo(value) < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="argument">The argument that was validated.</param>
        /// <param name="value">Not used.</param>
        /// <returns>The error message.</returns>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateRangeFailed(argument.ArgumentName, this);

        /// <inheritdoc/>
        protected override string GetUsageHelpCore(CommandLineArgument argument)
            => argument.Parser.StringProvider.ValidateRangeUsageHelp(this);
    }
}
