using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Validation
{
    /// <summary>
    /// Base class for argument validators.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Argument validators are executed before or after an argument's value is set, and allow
    ///   you to check whether an argument's value meets certain conditions.
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
    ///   Several built-in validators are provided, and you can derive from this class to create
    ///   custom validators.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public abstract class ArgumentValidationAttribute : Attribute
    {
        /// <summary>
        /// Gets a value that indicates when validation will run.
        /// </summary>
        /// <value>
        /// One of the values of the <see cref="ValidationMode"/> enumeration. If not overridden
        /// in a derived class, the value is <see cref="ValidationMode.AfterConversion"/>.
        /// </value>
        public virtual ValidationMode Mode => ValidationMode.AfterConversion;

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
        /// <param name="argument">The argument being validated.</param>
        /// <param name="value">
        ///   The argument value. If not <see langword="null"/>, this must be an instance of
        ///   <see cref="CommandLineArgument.ArgumentType"/>.
        /// </param>
        /// <exception cref="CommandLineArgumentException">
        ///   The <paramref name="value"/> parameter is not a valid value. The <see cref="CommandLineArgumentException.Category"/>
        ///   property will be the value of the <see cref="ErrorCategory"/> property.
        /// </exception>
        public void Validate(CommandLineArgument argument, object? value)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            if (!IsValid(argument, value))
                throw new CommandLineArgumentException(GetErrorMessage(argument, value), argument.ArgumentName, ErrorCategory);
        }

        /// <summary>
        /// Gets the error message to display if validation failed.
        /// </summary>
        /// <param name="argument">The argument that was validated.</param>
        /// <param name="value">
        ///   The argument value. If not <see langword="null"/>, this must be an instance of
        ///   <see cref="CommandLineArgument.ArgumentType"/>.
        /// </param>
        /// <returns>The error message.</returns>
        /// <remarks>
        /// <para>
        ///   Override this method in a derived class to provide a custom error message. Otherwise,
        ///   it will return a generic error message.
        /// </para>
        /// </remarks>
        public virtual string GetErrorMessage(CommandLineArgument argument, object? value)
            => argument.Parser.StringProvider.ValidationFailed(argument.ArgumentName);

        /// <summary>
        /// When overridden in a derived class, determines if the argument is valid.
        /// </summary>
        /// <param name="argument">The argument being validated.</param>
        /// <param name="value">
        ///   The argument value. If not <see langword="null"/>, this must be an instance of
        ///   <see cref="CommandLineArgument.ArgumentType"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        ///   For regular arguments, the <paramref name="value"/> parameter will be identical to
        ///   the <see cref="CommandLineArgument.Value"/> property. For multi-value or dictionary
        ///   arguments, the <paramref name="value"/> parameter will equal the last value added
        ///   to the collection or dictionary.
        /// </para>
        /// <para>
        ///   If the <see cref="Mode"/> property is <see cref="ValidationMode.AfterParsing"/>,
        ///   <paramref name="value"/> will always be <see langword="null"/>. Use the
        ///   <see cref="CommandLineArgument.Value"/> property instead.
        /// </para>
        /// </remarks>
        public abstract bool IsValid(CommandLineArgument argument, object? value);
    }
}
