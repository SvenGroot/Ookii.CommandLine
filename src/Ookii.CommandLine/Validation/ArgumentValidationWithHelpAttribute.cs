namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Base class for argument validators that have usage help.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   It's not required for argument validators that have help to derive from this class;
    ///   it's sufficient to derive from the <see cref="ArgumentValidationAttribute"/> class
    ///   directly and override the <see cref="ArgumentValidationAttribute.GetUsageHelp"/> method.
    ///   This class just adds some common functionality to make it easier.
    /// </para>
    /// </remarks>
    public abstract class ArgumentValidationWithHelpAttribute : ArgumentValidationAttribute
    {
        /// <summary>
        /// Gets or sets a value that indicates whether this validator's help should be included
        /// in the argument's description.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to include it in the description; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        ///   This has no effect if the <see cref="WriteUsageOptions.IncludeValidatorsInDescription"/>
        ///   property is <see langword="false"/>.
        /// </para>
        /// <para>
        ///   The help text is the value returned by <see cref="GetUsageHelp"/>.
        /// </para>
        /// </remarks>
        public bool IncludeInUsageHelp { get; set; } = true;

        /// <summary>
        /// Gets the usage help message for this validator.
        /// </summary>
        /// <param name="argument">The argument is the validator is for.</param>
        /// <returns>
        /// The usage help message, or <see langword="null"/> if the <see cref="IncludeInUsageHelp"/>
        /// property is <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   This function is only called if the <see cref="WriteUsageOptions.IncludeValidatorsInDescription"/>
        ///   property is <see langword="true"/>.
        /// </para>
        /// </remarks>

        public override string? GetUsageHelp(CommandLineArgument argument)
            => IncludeInUsageHelp ? GetUsageHelpCore(argument) : null;

        /// <summary>
        /// Gets the usage help message for this validator.
        /// </summary>
        /// <param name="argument">The argument is the validator is for.</param>
        /// <returns>
        /// The usage help message.
        /// </returns>
        protected abstract string GetUsageHelpCore(CommandLineArgument argument);
    }
}
