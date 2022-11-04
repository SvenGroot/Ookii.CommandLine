using System.Globalization;
using System.Text.RegularExpressions;

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
    /// <para>
    ///   This validator does not add any help text to the argument description.
    /// </para>
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
        /// <param name="options">A combination of <see cref="RegexOptions"/> values to use.</param>
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

        /// <summary>
        /// Gets a value that indicates when validation will run.
        /// </summary>
        /// <value>
        /// <see cref="ValidationMode.BeforeConversion"/>.
        /// </value>
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

        /// <summary>
        /// Determines if the argument's value matches the pattern.
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
            if (value is not string stringValue)
                return false;

            return Pattern.IsMatch(stringValue);
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="argument">The argument that was validated.</param>
        /// <param name="value">Not used.</param>
        /// <returns>The value of the <see cref="ErrorMessage"/> property, or a generic message
        /// if it's <see langword="null"/>.</returns>
        public override string GetErrorMessage(CommandLineArgument argument, object? value)
        {
            if (ErrorMessage == null)
                return base.GetErrorMessage(argument, value);

            return string.Format(CultureInfo.CurrentCulture, ErrorMessage, argument.ArgumentName, value, _pattern);
        }
    }
}
