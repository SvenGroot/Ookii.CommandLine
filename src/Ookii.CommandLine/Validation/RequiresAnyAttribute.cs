using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Validates whether at least one of the specified arguments is supplied.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This is a class validator, which should be applied to the class that defines arguments,
    ///   not to a specific argument.
    /// </para>
    /// <para>
    ///   Use this attribute if you have multiple arguments, only one of which needs to be supplied
    ///   at a time.
    /// </para>
    /// <para>
    ///   This attribute is useful when combined with the <see cref="ProhibitsAttribute"/> attribute.
    ///   If you have two mutually exclusive attribute, you cannot mark either of them as required.
    ///   For example, given arguments A and B, if B prohibits A but A is required, then B can
    ///   never be used.
    /// </para>
    /// <para>
    ///   Instead, you can use the <see cref="RequiresAnyAttribute"/> attribute to indicate that
    ///   the user must supply either A or B, and the <see cref="ProhibitsAttribute"/> attribute
    ///   to indicate that they cannot supply both at once.
    /// </para>
    /// <code>
    /// [RequiresAny(nameof(Address), nameof(Path))]
    /// class Arguments
    /// {
    ///     [CommandLineArgument]
    ///     public Uri Address { get; set; }
    ///     
    ///     [CommandLineArgument]
    ///     [Prohibits(nameof(Address))]
    ///     public string Path { get; set; }
    /// }
    /// </code>
    /// <note>
    ///   You can only use <c>nameof</c> if the name of the argument matches the name of the
    ///   property. Be careful if you have explicit names or are using <see cref="NameTransform"/>.
    /// </note>
    /// <para>
    ///   The names of the arguments are not validated when the attribute is created. If one of the
    ///   specified arguments does not exist, it is assumed to have no value.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class RequiresAnyAttribute : ClassValidationAttribute
    {
        private readonly string[] _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresAnyAttribute"/> class.
        /// </summary>
        /// <param name="argument1">The name of the first argument.</param>
        /// <param name="argument2">The name of the second argument.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="argument1"/> or <paramref name="argument2"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        ///   This constructor exists because <see cref="RequiresAnyAttribute.RequiresAnyAttribute(string[])"/>
        ///   is not CLS-compliant.
        /// </remarks>
        public RequiresAnyAttribute(string argument1, string argument2)
        {
            // This constructor exists to avoid a warning about non-CLS compliant types.
            if (argument1 == null)
                throw new ArgumentNullException(nameof(argument1));

            if (argument2 == null)
                throw new ArgumentNullException(nameof(argument2));

            _arguments = new[] { argument1, argument2 };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresAnyAttribute"/> class.
        /// </summary>
        /// <param name="arguments">The names of the arguments.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="arguments"/> or one of its items is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="arguments"/> contains fewer than two items.
        /// </exception>
        public RequiresAnyAttribute(params string[] arguments)
        {
            if (_arguments == null || _arguments.Any(a => a == null))
                throw new ArgumentNullException(nameof(arguments));

            if (_arguments.Length <= 1)
                throw new ArgumentException(Properties.Resources.RequiresAnySingleArgument, nameof(arguments));

            _arguments = arguments;
        }

        /// <summary>
        /// Gets the names of the arguments, one of which must be supplied on the command line.
        /// </summary>
        /// <value>
        /// The names of the arguments.
        /// </value>
        public string[] Arguments => _arguments;

        /// <summary>
        /// Gets the error category used for the <see cref="CommandLineArgumentException"/> when
        /// validation fails.
        /// </summary>
        /// <value>
        /// <see cref="CommandLineArgumentErrorCategory.MissingRequiredArgument"/>.
        /// </value>
        public override CommandLineArgumentErrorCategory ErrorCategory
            => CommandLineArgumentErrorCategory.MissingRequiredArgument;

        /// <summary>
        /// Gets or sets a value that indicates whether this validator's help should be included
        /// in the usage help.
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
        /// Determines if the at least one of the arguments in <see cref="Arguments"/> was
        /// supplied on the command line.
        /// </summary>
        /// <param name="parser">The argument parser being validated.</param>
        /// <returns>
        ///   <see langword="true"/> if the arguments are valid; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool IsValid(CommandLineParser parser)
            => _arguments.Any(name => parser.GetArgument(name)?.HasValue ?? false);

        /// <inheritdoc/>
        public override string GetErrorMessage(CommandLineParser parser)
            => parser.StringProvider.ValidateRequiresAnyFailed(_arguments);

        /// <summary>
        /// Gets the usage help message for this validator.
        /// </summary>
        /// <param name="parser">The parser is the validator is for.</param>
        /// <returns>
        /// The usage help message, or <see langword="null"/> if the <see cref="IncludeInUsageHelp"/>
        /// property is <see langword="false"/>.
        /// </returns>
        public override string? GetUsageHelp(CommandLineParser parser)
            => IncludeInUsageHelp ? parser.StringProvider.RequiresAnyUsageHelp(Arguments) : null;
    }
}
