using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Base class for argument class validators.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Class validators are executed when all arguments have been parsed, and allow you to check
    ///   whether the whole set of arguments meets a condition. Use this instead of <see cref="ArgumentValidationAttribute"/>
    ///   if the type of validation being performed doesn't belong to a specific argument, or must
    ///   be performed even if the argument(s) don't have values.
    /// </para>
    /// <para>
    ///   If validation fails, it will throw a <see cref="CommandLineArgumentException"/> with
    ///   the category specified in the <see cref="ErrorCategory"/> property. The
    ///   <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)"/>,
    ///   <see cref="ShellCommand.CreateShellCommand(System.Reflection.Assembly, string?, string[], int, CreateShellCommandOptions?)"/>
    ///   and <see cref="ShellCommand.RunShellCommand(System.Reflection.Assembly, string?, string[], int, CreateShellCommandOptions?)"/>
    ///   methods will automatically display the error message and usage help if validation failed.
    /// </para>
    /// <para>
    ///   A built-in validator is provided, and you can derive from this class to create custom
    ///   validators.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ClassValidationAttribute : Attribute
    {
        /// <summary>
        /// Gets the error category used for the <see cref="CommandLineArgumentException"/> when
        /// validation fails.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="CommandLineArgumentErrorCategory"/> enumeration. If not overridden
        /// in a derived class, the value is <see cref="CommandLineArgumentErrorCategory.ValidationFailed"/>.
        /// </value>
        public virtual CommandLineArgumentErrorCategory ErrorCategory => CommandLineArgumentErrorCategory.ValidationFailed;

        /// <summary>
        /// Validates the argument value, and throws an exception if validation failed.
        /// </summary>
        /// <param name="parser">The argument parser being validated.</param>
        /// <exception cref="CommandLineArgumentException">
        ///   The combination of arguments in the <paramref name="parser"/> is not valid.  The
        ///   <see cref="CommandLineArgumentException.Category"/> property will be the value of the
        ///   <see cref="ErrorCategory"/> property.
        /// </exception>
        public void Validate(CommandLineParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            if (!IsValid(parser))
                throw new CommandLineArgumentException(GetErrorMessage(parser), null, ErrorCategory);
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="parser">The argument parser that was validated.</param>
        /// <returns>The error message.</returns>
        /// <remarks>
        /// <para>
        ///   Override this method in a derived class to provide a custom error message. Otherwise,
        ///   it will return a generic error message.
        /// </para>
        /// </remarks>
        public virtual string GetErrorMessage(CommandLineParser parser)
            => parser.StringProvider.ClassValidationFailed();

        /// <summary>
        /// When overridden in a derived class, determines if the arguments are valid.
        /// </summary>
        /// <param name="parser">The argument parser being validated.</param>
        /// <returns>
        ///   <see langword="true"/> if the arguments are valid; otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool IsValid(CommandLineParser parser);

        /// <summary>
        /// Gets the usage help message for this validator.
        /// </summary>
        /// <param name="parser">The parser is the validator is for.</param>
        /// <returns>
        /// The usage help message, or <see langword="null"/> if there is none. The
        /// base implementation always returns  <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   This function is only called if the <see cref="WriteUsageOptions.IncludeValidatorsInDescription"/>
        ///   property is <see langword="true"/>.
        /// </para>
        /// </remarks>
        public virtual string? GetUsageHelp(CommandLineParser parser) => null;
    }
}
