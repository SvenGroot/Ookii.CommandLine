using System;
using System.Security.Permissions;

namespace Ookii.CommandLine;

/// <summary>
/// The exception that is thrown when command line parsing failed due to an invalid command line.
/// </summary>
/// <remarks>
/// <para>
///   This exception indicates that the command line passed to the
///   <see cref="CommandLineParser{T}.Parse(ReadOnlyMemory{string})" qualifyHint="true"/> method, or
///   another parsing method, was invalid for the arguments defined by the
///   <see cref="CommandLineParser"/> instance.
/// </para>
/// <para>
///   Use the <see cref="Category"/> property to determine the exact cause of the exception.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
[Serializable]
public class CommandLineArgumentException : Exception
{
    private readonly string? _argumentName;
    private readonly CommandLineArgumentErrorCategory _category;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class. 
    /// </summary>
    public CommandLineArgumentException() { }

    /// <inheritdoc/>
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with a specified error message.
    /// </summary>
    public CommandLineArgumentException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with a
    /// specified error message and category.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="category">The category of this error.</param>
    public CommandLineArgumentException(string? message, CommandLineArgumentErrorCategory category)
        : base(message)
    {
        _category = category;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with
    /// a specified error message, argument name and category.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="argumentName">The name of the argument that was invalid.</param>
    /// <param name="category">The category of this error.</param>
    public CommandLineArgumentException(string? message, string? argumentName, CommandLineArgumentErrorCategory category)
        : base(message)
    {
        _argumentName = argumentName;
        _category = category;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with
    /// a specified error message and a reference to the inner <see cref="Exception"/> that is
    /// the cause of this <see cref="CommandLineArgumentException"/>. 
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">
    /// The <see cref="Exception"/> that is the cause of the current <see cref="CommandLineArgumentException"/>,
    /// or <see langword="null"/> if no inner <see cref="Exception"/> is specified.
    /// </param>
    public CommandLineArgumentException(string? message, Exception? inner) : base(message, inner) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with
    /// a specified error message, category, and a reference to the inner <see cref="Exception"/> that is
    /// the cause of this <see cref="CommandLineArgumentException"/>. 
    /// </summary>
    /// <param name="message">The error message that explains the reason for the <see cref="CommandLineArgumentException"/>.</param>
    /// <param name="category">The category of this error.</param>
    /// <param name="inner">
    /// The <see cref="Exception"/> that is the cause of the current <see cref="CommandLineArgumentException"/>,
    /// or a <see langword="null"/> if no inner <see cref="Exception"/> is specified.
    /// </param>
    public CommandLineArgumentException(string? message, CommandLineArgumentErrorCategory category, Exception? inner)
        : base(message, inner)
    {
        _category = category;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with
    /// a specified error message, argument name, category, and a reference to the inner
    /// <see cref="Exception"/> that is the cause of this <see cref="CommandLineArgumentException"/>. 
    /// </summary>
    /// <param name="message">The error message that explains the reason for the <see cref="CommandLineArgumentException"/>.</param>
    /// <param name="argumentName">
    /// The name of the argument that was invalid, or <see langword="null"/> if the error was not
    /// caused by a particular argument.
    /// </param>
    /// <param name="category">The category of this error.</param>
    /// <param name="inner">
    /// The <see cref="Exception"/> that is the cause of the current <see cref="CommandLineArgumentException"/>,
    /// or a <see langword="null"/> if no inner <see cref="Exception"/> is specified.
    /// </param>
    public CommandLineArgumentException(string? message, string? argumentName, CommandLineArgumentErrorCategory category, Exception? inner)
        : base(message, inner)
    {
        _argumentName = argumentName;
        _category = category;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with serialized data. 
    /// </summary>
    protected CommandLineArgumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        _argumentName = info.GetString("ArgumentName");
        _category = (CommandLineArgumentErrorCategory?)info.GetValue("Category", typeof(CommandLineArgumentErrorCategory)) ?? CommandLineArgumentErrorCategory.Unspecified;
    }

    /// <summary>
    /// Gets the name of the argument that was invalid.
    /// </summary>
    /// <value>
    /// The name of the invalid argument, or <see langword="null"/> if the error does not refer to a specific argument.
    /// </value>
    public string? ArgumentName
    {
        get { return _argumentName; }
    }

    /// <summary>
    /// Gets the category of this error.
    /// </summary>
    /// <value>
    /// One of the values of the <see cref="CommandLineArgumentErrorCategory"/> enumeration indicating the kind of error that occurred.
    /// </value>
    public CommandLineArgumentErrorCategory Category
    {
        get { return _category; }
    }

    /// <summary>
    /// Sets the <see cref="System.Runtime.Serialization.SerializationInfo"/> object with the
    /// argument name and additional exception information.
    /// </summary>
    /// <param name="info">The object that holds the serialized object data.</param>
    /// <param name="context">The contextual information about the source or destination.</param>
    /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
#if !NET6_0_OR_GREATER
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
#endif
    public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        base.GetObjectData(info, context);

        info.AddValue("ArgumentName", ArgumentName);
        info.AddValue("Category", Category);
    }
}
