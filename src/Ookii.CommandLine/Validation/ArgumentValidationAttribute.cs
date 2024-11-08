using Ookii.CommandLine.Commands;
using System;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Base class for argument validators.
/// </summary>
/// <remarks>
/// <para>
///   Argument validators are executed before and after an argument's value is set, and allow
///   you to check whether an argument's value meets certain conditions.
/// </para>
/// <para>
///   If validation fails, the validator will throw a <see cref="CommandLineArgumentException"/>
///   with the category specified in the <see cref="ErrorCategory"/> property. The
///   <see cref="CommandLineParser{T}.ParseWithErrorHandling()" qualifyHint="true"/> method, the
///   <see cref="CommandLineParser.Parse{T}(string[], ParseOptions?)" qualifyHint="true"/> method,
///   the generated <see cref="IParser{TSelf}.Parse(Ookii.CommandLine.ParseOptions?)" qualifyHint="true"/>,
///   and the <see cref="CommandManager"/> class will automatically display the error message and
///   usage help if validation failed.
/// </para>
/// <para>
///   Several built-in validators are provided, and you can derive from this class to create
///   custom validators.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public abstract class ArgumentValidationAttribute : Attribute
{
    /// <summary>
    /// Gets the error category used for the <see cref="CommandLineArgumentException"/> when
    /// validation fails.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="CommandLineArgumentErrorCategory"/> enumeration. If not overridden
    /// in a derived class, the value is <see cref="CommandLineArgumentErrorCategory.ValidationFailed" qualifyHint="true"/>.
    /// </value>
    public virtual CommandLineArgumentErrorCategory ErrorCategory => CommandLineArgumentErrorCategory.ValidationFailed;

    /// <summary>
    /// Validates the argument raw argument value, and throws an exception if validation failed.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   A string memory region containing the raw argument value as it was provided on the
    ///   command line.
    /// </param>
    /// <exception cref="CommandLineArgumentException">
    ///   The <paramref name="value"/> parameter is not a valid value. The <see cref="CommandLineArgumentException.Category" qualifyHint="true"/>
    ///   property will be the value of the <see cref="ErrorCategory"/> property.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This method calls the <see cref="IsValidPreConversion"/> method to do the actual validation.
    /// </para>
    /// </remarks>
    public void ValidatePreConversion(CommandLineArgument argument, ReadOnlyMemory<char> value)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        if (!IsValidPreConversion(argument, value))
        {
            var message = GetErrorMessage(argument, value.ToString());
            throw new CommandLineArgumentException(message, argument.ArgumentName, ErrorCategory);
        }
    }

    /// <summary>
    /// Validates the argument value after it was converted to the argument's type, and throws an
    /// exception if validation failed.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must bean instance of
    ///   <see cref="CommandLineArgument.ElementType" qualifyHint="true"/>.
    /// </param>
    /// <exception cref="CommandLineArgumentException">
    ///   The <paramref name="value"/> parameter is not a valid value. The <see cref="CommandLineArgumentException.Category" qualifyHint="true"/>
    ///   property will be the value of the <see cref="ErrorCategory"/> property.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This method calls the <see cref="IsValidPostConversion"/> method to do the actual
    ///   validation.
    /// </para>
    /// </remarks>
    public void ValidatePostConversion(CommandLineArgument argument, object? value)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        if (!IsValidPostConversion(argument, value))
        {
            var message = GetErrorMessage(argument, value);
            throw new CommandLineArgumentException(message, argument.ArgumentName, ErrorCategory);
        }
    }

    /// <summary>
    /// Validates the argument value after it was converted to the argument's type, and throws an
    /// exception if validation failed.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <exception cref="CommandLineArgumentException">
    ///   The parameter's value is not a valid. The <see cref="CommandLineArgumentException.Category" qualifyHint="true"/>
    ///   property will be the value of the <see cref="ErrorCategory"/> property.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This method calls the <see cref="IsValidPostParsing"/> method to do the actual
    ///   validation.
    /// </para>
    /// </remarks>
    public void ValidatePostParsing(CommandLineArgument argument)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        if (!IsValidPostParsing(argument))
        {
            var message = GetErrorMessage(argument, null);
            throw new CommandLineArgumentException(message, argument.ArgumentName, ErrorCategory);
        }
    }

    /// <summary>
    /// Determines if the argument's raw string value is valid.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   A string memory region containing the raw argument value as it was provided on the
    ///   command line.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <note>
    ///   Do not throw an exception from this method if validation fails. Instead, return
    ///   <see langword="false"/> and provide an error message using the <see cref="GetErrorMessage"/>
    ///   method.
    /// </note>
    /// <para>
    ///   The default implementation always returns <see langword="true"/>.
    /// </para>
    /// </remarks>
    public virtual bool IsValidPreConversion(CommandLineArgument argument, ReadOnlyMemory<char> value) => true;

    /// <summary>
    /// Determines if the argument's value is valid after it was converted to the argument's type.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must be an
    ///   instance of <see cref="CommandLineArgument.ElementType" qualifyHint="true"/>.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <note>
    ///   Do not throw an exception from this method if validation fails. Instead, return
    ///   <see langword="false"/> and provide an error message using the <see cref="GetErrorMessage"/>
    ///   method.
    /// </note>
    /// <para>
    ///   The default implementation always returns <see langword="true"/>.
    /// </para>
    /// </remarks>
    public virtual bool IsValidPostConversion(CommandLineArgument argument, object? value) => true;

    /// <summary>
    /// Determines if the argument's value is valid after all arguments have been parsed.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <note>
    ///   Do not throw an exception from this method if validation fails. Instead, return
    ///   <see langword="false"/> and provide an error message using the <see cref="GetErrorMessage"/>
    ///   method.
    /// </note>
    /// <para>
    ///   This method is called even if the argument was not specified on the command line. Check
    ///   the <see cref="CommandLineArgument.HasValue" qualifyHint="true"/> to see if a value was
    ///   provided.
    /// </para>
    /// <para>
    ///   The default implementation always returns <see langword="true"/>.
    /// </para>
    /// </remarks>
    public virtual bool IsValidPostParsing(CommandLineArgument argument) => true;

    /// <summary>
    /// Gets the error message to display if validation failed.
    /// </summary>
    /// <param name="argument">The argument that was validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must be a <see cref="string"/> or an
    ///   instance of <see cref="CommandLineArgument.ElementType" qualifyHint="true"/>.
    /// </param>
    /// <returns>The error message.</returns>
    /// <remarks>
    /// <para>
    ///   The <paramref name="value"/> parameter is a <see cref="string"/> if the
    ///   <see cref="IsValidPreConversion"/> method failed, and an instance of
    ///   <see cref="CommandLineArgument.ElementType" qualifyHint="true"/> if the
    ///   <see cref="IsValidPostConversion"/> method failed. If the <see cref="IsValidPostParsing"/>
    ///   method failed, <paramref name="value"/> will be <see langword="null"/>.
    /// </para>
    /// <para>
    ///   Override this method in a derived class to provide a custom error message. Otherwise,
    ///   it will return a generic error message.
    /// </para>
    /// </remarks>
    public virtual string GetErrorMessage(CommandLineArgument argument, object? value)
        => argument.Parser.StringProvider.ValidationFailed(argument.ArgumentName);

    /// <summary>
    /// Gets the usage help message for this validator.
    /// </summary>
    /// <param name="argument">The argument that the validator is for.</param>
    /// <returns>
    /// The usage help message, or <see langword="null"/> if there is none. The
    /// base implementation always returns  <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   This function is only called if the <see cref="UsageWriter.IncludeValidatorsInDescription" qualifyHint="true"/>
    ///   property is <see langword="true"/>.
    /// </para>
    /// </remarks>
    public virtual string? GetUsageHelp(CommandLineArgument argument) => null;
}
