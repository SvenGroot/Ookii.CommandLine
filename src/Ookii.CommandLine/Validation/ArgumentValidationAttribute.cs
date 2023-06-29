using Ookii.CommandLine.Commands;
using System;

namespace Ookii.CommandLine.Validation;

/// <summary>
/// Base class for argument validators.
/// </summary>
/// <remarks>
/// <para>
///   Argument validators are executed before or after an argument's value is set, and allow
///   you to check whether an argument's value meets certain conditions.
/// </para>
/// <para>
///   If validation fails, the validator will throw a <see cref="CommandLineArgumentException"/>
///   with the category specified in the <see cref="ErrorCategory"/> property. The
///   <see cref="CommandLineParser{T}.ParseWithErrorHandling()" qualifyHint="true"/> method, the
///   <see cref="CommandLineParser.Parse{T}(string[], int, ParseOptions?)" qualifyHint="true"/> method,
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
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
public abstract class ArgumentValidationAttribute : Attribute
{
    /// <summary>
    /// Gets a value that indicates when validation will run.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="ValidationMode"/> enumeration. If not overridden
    /// in a derived class, the value is <see cref="ValidationMode.AfterConversion" qualifyHint="true"/>.
    /// </value>
    public virtual ValidationMode Mode => ValidationMode.AfterConversion;

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
    /// Validates the argument value, and throws an exception if validation failed.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must be a string or an instance of
    ///   <see cref="CommandLineArgument.ArgumentType" qualifyHint="true"/>.
    /// </param>
    /// <exception cref="CommandLineArgumentException">
    ///   The <paramref name="value"/> parameter is not a valid value. The <see cref="CommandLineArgumentException.Category" qualifyHint="true"/>
    ///   property will be the value of the <see cref="ErrorCategory"/> property.
    /// </exception>
    public void Validate(CommandLineArgument argument, object? value)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        if (!IsValid(argument, value))
        {
            throw new CommandLineArgumentException(GetErrorMessage(argument, value), argument.ArgumentName, ErrorCategory);
        }
    }

    /// <summary>
    /// Validates the argument value, and throws an exception if validation failed.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must be an instance of
    ///   <see cref="CommandLineArgument.ArgumentType" qualifyHint="true"/>.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if validation was performed and successful; <see langword="false"/>
    ///   if this validator doesn't support validating spans and the <see cref="Validate"/>
    ///   method should be used instead.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   The <see cref="CommandLineParser"/> class will only call this method if the
    ///   <see cref="Mode"/> property is <see cref="ValidationMode.BeforeConversion" qualifyHint="true"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="CommandLineArgumentException">
    ///   The <paramref name="value"/> parameter is not a valid value. The <see cref="CommandLineArgumentException.Category" qualifyHint="true"/>
    ///   property will be the value of the <see cref="ErrorCategory"/> property.
    /// </exception>
    public bool ValidateSpan(CommandLineArgument argument, ReadOnlySpan<char> value)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        var result = IsSpanValid(argument, value);
        if (result == false)
        {
            throw new CommandLineArgumentException(GetErrorMessage(argument, value.ToString()), argument.ArgumentName, ErrorCategory);
        }

        return result != null;
    }


    /// <summary>
    /// When overridden in a derived class, determines if the argument is valid.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must be a string or an
    ///   instance of <see cref="CommandLineArgument.ArgumentType" qualifyHint="true"/>.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   If the <see cref="Mode"/> property is <see cref="ValidationMode.BeforeConversion" qualifyHint="true"/>,
    ///   the <paramref name="value"/> parameter will be the raw string value provided by the
    ///   user on the command line.
    /// </para>
    /// <para>
    ///   If the <see cref="Mode"/> property is <see cref="ValidationMode.AfterConversion" qualifyHint="true"/>,
    ///   for regular arguments, the <paramref name="value"/> parameter will be identical to
    ///   the <see cref="CommandLineArgument.Value" qualifyHint="true"/> property. For multi-value or dictionary
    ///   arguments, the <paramref name="value"/> parameter will be equal to the last value added
    ///   to the collection or dictionary.
    /// </para>
    /// <para>
    ///   If the <see cref="Mode"/> property is <see cref="ValidationMode.AfterParsing" qualifyHint="true"/>,
    ///   <paramref name="value"/> will always be <see langword="null"/>. Use the
    ///   <see cref="CommandLineArgument.Value" qualifyHint="true"/> property instead.
    /// </para>
    /// <para>
    ///   If you need to check the type of the argument, use the <see cref="CommandLineArgument.ElementType" qualifyHint="true"/>
    ///   property unless you want to get the collection type for a multi-value or dictionary
    ///   argument.
    /// </para>
    /// </remarks>
    public abstract bool IsValid(CommandLineArgument argument, object? value);

    /// <summary>
    /// When overridden in a derived class, determines if the argument is valid.
    /// </summary>
    /// <param name="argument">The argument being validated.</param>
    /// <param name="value">
    ///   The raw string argument value provided by the user on the command line.
    /// </param>
    /// <returns>
    ///   <see langword="null"/> if this validator doesn't support validating spans, and the
    ///   regular <see cref="IsValid"/> method should be called instead; <see langword="true"/>
    ///   if the value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    ///   The <see cref="CommandLineParser"/> class will only call this method if the
    ///   <see cref="Mode"/> property is <see cref="ValidationMode.BeforeConversion" qualifyHint="true"/>.
    /// </para>
    /// <para>
    ///   If you need to check the type of the argument, use the <see cref="CommandLineArgument.ElementType" qualifyHint="true"/>
    ///   property unless you want to get the collection type for a multi-value or dictionary
    ///   argument.
    /// </para>
    /// <para>
    ///   The base class implementation returns <see langword="null"/>.
    /// </para>
    /// </remarks>
    public virtual bool? IsSpanValid(CommandLineArgument argument, ReadOnlySpan<char> value) => null;

    /// <summary>
    /// Gets the error message to display if validation failed.
    /// </summary>
    /// <param name="argument">The argument that was validated.</param>
    /// <param name="value">
    ///   The argument value. If not <see langword="null"/>, this must be an instance of
    ///   <see cref="CommandLineArgument.ArgumentType" qualifyHint="true"/>.
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
