using System;

namespace Ookii.CommandLine;

/// <summary>
/// Specifies a separator for the values of multi-value arguments.
/// </summary>
/// <remarks>
/// <para>
///   Normally, you need to supply the argument multiple times to set multiple values, e.g.
///   by using <c>-Sample Value1 -Sample Value2</c>. If you specify the <see cref="MultiValueSeparatorAttribute"/>
///   attribute, it allows you to specify multiple values with a single argument by using a
///   separator.
/// </para>
/// <para>
///   There are two ways you can use separators for multi-value arguments: a white-space
///   separator, or an explicit separator string.
/// </para>
/// <para>
///   You enable the use of white-space separators with the <see cref="MultiValueSeparatorAttribute()"/>
///   constructor. A multi-value argument that allows white-space separators is able to consume
///   multiple values from the command line that follow it. All values that follow the name, up
///   until the next argument name, are considered values for this argument.
/// </para>
/// <para>
///   For example, if you use <c>-Sample Value1 Value2 Value3</c>, all three arguments after
///   <c>-Sample</c> are taken as values. In this case, it's not possible to supply any
///   positional arguments until another named argument has been supplied.
/// </para>
/// <para>
///   Using white-space separators will not work if the <see cref="CommandLineParser.AllowWhiteSpaceValueSeparator" qualifyHint="true"/>
///   property is <see langword="false"/> or if the argument is a multi-value switch argument.
/// </para>
/// <para>
///   Using the <see cref="MultiValueSeparatorAttribute(string)"/> constructor, you instead
///   specify an explicit character sequence to be used as a separator. For example, if the
///   separator is set to a comma, you can use <c>-Sample Value1,Value2</c>.
/// </para>
/// <note>
///   If you specify an explicit separator for a multi-value argument, it will <em>not</em> be
///   possible to use the separator in the individual argument values. There is no way to
///   escape it.
/// </note>
/// <para>
///   Even if the <see cref="MultiValueSeparatorAttribute"/> is specified it is still possible to use
///   multiple arguments to specify multiple values. For example, using a comma as the separator, 
///   <c>-Sample Value1,Value2 -Sample Value3</c> will mean the argument "Sample" has three values.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
[AttributeUsage(AttributeTargets.Property)]
public class MultiValueSeparatorAttribute : Attribute
{
    private readonly string? _separator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiValueSeparatorAttribute"/> class
    /// using white-space as the separator.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   A multi-value argument that allows white-space separators is able to consume multiple
    ///   values from the command line that follow it. All values that follow the name, up until
    ///   the next argument name, are considered values for this argument.
    /// </para>
    /// </remarks>
    public MultiValueSeparatorAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiValueSeparatorAttribute"/> class.
    /// </summary>
    /// <param name="separator">The separator that separates the values.</param>
    /// <remarks>
    /// <note>
    ///   If you specify a separator for a multi-value argument, it will <em>not</em> be possible
    ///   to use the separator character in the individual argument values. There is no way to escape it.
    /// </note>
    /// </remarks>
    public MultiValueSeparatorAttribute(string separator)
    {
        _separator = separator;
    }

    /// <summary>
    /// Gets the separator for the values of a multi-value argument.
    /// </summary>
    /// <value>
    /// The separator for the argument values, or <see langword="null"/> to indicate that
    /// white-space separators are allowed.
    /// </value>
    /// <remarks>
    /// <note>
    ///   If you specify a separator for a multi-value argument, it will <em>not</em> be possible
    ///   to use the separator character in the individual argument values. There is no way to escape it.
    /// </note>
    /// <para>
    ///   A multi-value argument that allows white-space separators is able to consume multiple
    ///   values from the command line that follow it. All values that follow the name, up until
    ///   the next argument name, are considered values for this argument.
    /// </para>
    /// </remarks>
    public virtual string? Separator
    {
        get { return _separator; }
    }
}
