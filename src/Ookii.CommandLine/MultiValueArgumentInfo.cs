using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine;

/// <summary>
/// Provides information that only applies to multi-value and dictionary arguments.
/// </summary>
/// <threadsafety static="true" instance="false"/>
public sealed class MultiValueArgumentInfo
{
    /// <summary>
    /// Creates a new instance of the <see cref="MultiValueArgumentInfo"/> class.
    /// </summary>
    /// <param name="separator">
    /// The separator between multiple values in the same token, or <see langword="null"/> if no
    /// separator is used.
    /// </param>
    /// <param name="allowWhiteSpaceSeparator">
    /// <see langword="true"/> if the argument can consume multiple tokens; otherwise,
    /// <see langword="false"/>.
    /// </param>
    public MultiValueArgumentInfo(string? separator, bool allowWhiteSpaceSeparator)
    {
        Separator = separator;
        AllowWhiteSpaceSeparator = allowWhiteSpaceSeparator;
    }


    /// <summary>
    /// Gets the separator that can be used to supply multiple values in a single argument token.
    /// </summary>
    /// <value>
    /// The separator, or <see langword="null"/> if no separator is used.
    /// </value>
    /// <seealso cref="MultiValueSeparatorAttribute"/>
    public string? Separator { get; }

    /// <summary>
    /// Gets a value that indicates whether or not the argument can consume multiple following
    /// argument tokens.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the argument consume multiple following tokens; otherwise,
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    ///   A multi-value argument that allows white-space separators is able to consume multiple
    ///   values from the command line that follow it. All values that follow the name, up until
    ///   the next argument name, are considered values for this argument.
    /// </para>
    /// </remarks>
    /// <seealso cref="MultiValueSeparatorAttribute"/>
    public bool AllowWhiteSpaceSeparator { get; internal set; }
}
