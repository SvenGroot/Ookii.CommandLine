using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates that an argument's value matches the specified <see cref="Regex"/>.
    /// </summary>
    /// <remarks>
    /// <note>
    ///   If the argument's type is not <see cref="string"/>, this validator uses the raw string
    ///   value provided by the user, before type conversion takes place.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ValidatePatternAttribute : ArgumentValidationAttribute
    {
        private readonly string _pattern;
        private Regex? _patternRegex;
        private readonly RegexOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatePatternAttribute"/> class.
        /// </summary>
        /// <param name="pattern">The regular expression to match against.</param>
        /// <param name="options">A combinatino of <see cref="RegexOptions"/> values to use.</param>
        /// <remarks>
        /// <para>
        ///   This constructor does not validate if the regular expression specified in <paramref name="pattern"/>
        ///   is valid. The <see cref="Regex"/> instance is not constructed until the validation
        ///   is performed.
        /// </para>
        /// </remarks>
        public ValidatePatternAttribute(string pattern, RegexOptions options = RegexOptions.None)
        {
            _pattern = pattern;
            _options = options;
        }

        /// <inheritdoc/>
        public override ValidationMode Mode => ValidationMode.BeforeConversion;


        /// <summary>
        /// Gets or sets a custom error message to use.
        /// </summary>
        /// <value>
        /// A compound format string for the error message to use, or <see langword="null"/> to
        /// use a generic error message.
        /// </value>
        /// <remarks>
        /// <para>
        ///   If this property is <see langword="null"/>, the message returned by
        ///   <see cref="ArgumentValidationAttribute.GetErrorMessage"/> will be used.
        /// </para>
        /// <para>
        ///   This property is a compound format string, and may have three placeholders:
        ///   {0} for the argument name, {1} for the value, and {2} for the pattern.
        /// </para>
        /// </remarks>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets the pattern that values must match.
        /// </summary>
        /// <value>
        /// The <see cref="Regex"/> pattern that values must match.
        /// </value>
        public Regex Pattern => _patternRegex ??= new Regex(_pattern, _options);

        /// <inheritdoc/>
        public override bool IsValid(CommandLineArgument argument, object? value)
        {
            if (value is not string stringValue)
                return false;

            return Pattern.IsMatch(stringValue);
        }

        /// <inheritdoc/>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
        {
            if (ErrorMessage == null)
                return base.GetErrorMessage(argument, value);

            return string.Format(CultureInfo.CurrentCulture, ErrorMessage, argument.ArgumentName, value, _pattern);
        }
    }
}
