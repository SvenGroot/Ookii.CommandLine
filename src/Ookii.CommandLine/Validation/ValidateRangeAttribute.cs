﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class ValidateRangeAttribute : ArgumentValidationAttribute
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
        public ValidateRangeAttribute(object? minimum, object? maximum)
        {
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

        /// <inheritdoc/>
        public override bool IsValid(CommandLineArgument argument, object? value)
        {
            var min = (IComparable?)argument.ConvertToArgumentTypeInvariant(Minimum);
            var max = (IComparable?)argument.ConvertToArgumentTypeInvariant(Maximum);

            if (min != null && min.CompareTo(value) > 0)
                return false;

            if (max != null && max.CompareTo(value) < 0)
                return false;

            return true;
        }

        /// <inheritdoc/>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidateRangeFailed(argument.ArgumentName, this);
    }
}