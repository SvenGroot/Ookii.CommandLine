using System;
using System.Collections.Immutable;
using System.Security.Permissions;

namespace Ookii.CommandLine;

/// <summary>
/// The exception that is thrown when command line argument parsing failed due to an ambiguous
/// prefix alias.
/// </summary>
/// <remarks>
/// <para>
///   If automatic prefix aliases are enabled, and the user attempted to use an argument name that
///   could be a prefix alias for multiple arguments, this exception is thrown.
/// </para>
/// <para>
///   The value of the <see cref="CommandLineArgumentException.ArgumentName" qualifyHint="true"/>
///   property will indicate the prefix that the user attempted to use. The <see cref="PossibleMatches"/>
///   property contains an array of argument names or aliases that the prefix could match.
/// </para>
/// <para>
///   The <see cref="CommandLineArgumentException.Category" qualifyHint="true"/> property for this
///   exception type will always be
///   <see cref="CommandLineArgumentErrorCategory.AmbiguousPrefixAlias" qualifyHint="true"/>.
/// </para>
/// </remarks>
[Serializable]
public class AmbiguousPrefixAliasException : CommandLineArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousPrefixAliasException"/> class.
    /// </summary>
    public AmbiguousPrefixAliasException()
        : base(null, CommandLineArgumentErrorCategory.AmbiguousPrefixAlias)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousPrefixAliasException"/> class with
    /// the specified error message, argument name, and possible matches.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="argumentName">The name of the argument that was invalid.</param>
    /// <param name="possibleMatches">An immutable array containing the possible matches.</param>
    public AmbiguousPrefixAliasException(string? message, string? argumentName, ImmutableArray<string> possibleMatches)
        : base(message, argumentName, CommandLineArgumentErrorCategory.AmbiguousPrefixAlias)
    {
        PossibleMatches = possibleMatches;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousPrefixAliasException"/> class with serialized data. 
    /// </summary>
#if NET8_0_OR_GREATER
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
#endif
    protected AmbiguousPrefixAliasException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
        PossibleMatches = (ImmutableArray<string>?)info.GetValue("PossibleMatches", typeof(ImmutableArray<string>)) ?? [];
    }

    /// <summary>
    /// Gets the possible matches for the ambiguous prefix alias.
    /// </summary>
    /// <value>
    /// An immutable array containing the possible matches.
    /// </value>
    /// <remarks>
    /// <para>
    ///   This property contains an array of possible argument names or aliases that the prefix
    ///   could match. If an argument could match the prefix with more than one possible alias,
    ///   only the first match is included in the array.
    /// </para>
    /// </remarks>
    public ImmutableArray<string> PossibleMatches { get; }

    /// <summary>
    /// Sets the <see cref="System.Runtime.Serialization.SerializationInfo"/> object with the
    /// possible matches and additional exception information.
    /// </summary>
    /// <param name="info">The object that holds the serialized object data.</param>
    /// <param name="context">The contextual information about the source or destination.</param>
    /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
#if !NET6_0_OR_GREATER
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
#endif
#if NET8_0_OR_GREATER
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
#endif
    public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue("PossibleMatches", PossibleMatches);
    }
}
