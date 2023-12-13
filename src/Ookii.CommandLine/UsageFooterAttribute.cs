using System;
using System.ComponentModel;

namespace Ookii.CommandLine;

/// <summary>
/// Gets or sets a footer that will be added to the usage help for an arguments class.
/// </summary>
/// <remarks>
/// <para>
///   The <see cref="DescriptionAttribute"/> attribute provides text that's written at the top of
///   the usage help. The <see cref="UsageFooterAttribute"/> attribute does the same thing, but for
///   text that's written at the bottom of the usage help.
/// </para>
/// <para>
///   The footer will only be used when the full usage help is shown, using
///   <see cref="UsageHelpRequest.Full" qualifyHint="true"/>.
/// </para>
/// <para>
///   You can derive from this attribute to use an alternative source for the footer, such as a
///   resource table that can be localized.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class UsageFooterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsageFooterAttribute"/> class.
    /// </summary>
    /// <param name="footer">The footer text.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="footer"/> is <see langword="null"/>.
    /// </exception>
    public UsageFooterAttribute(string footer)
    {
        FooterValue = footer ?? throw new ArgumentNullException(nameof(footer));
    }

    /// <summary>
    /// Gets the footer text.
    /// </summary>
    /// <value>
    /// The footer text.
    /// </value>
    public virtual string Footer => FooterValue;

    /// <summary>
    /// Gets the footer text stored in this attribute.
    /// </summary>
    /// <value>
    /// The footer text.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The base class implementation of the <see cref="Footer"/> property returns the value of
    ///   this property.
    /// </para>
    /// </remarks>
    protected string FooterValue { get; }
}
