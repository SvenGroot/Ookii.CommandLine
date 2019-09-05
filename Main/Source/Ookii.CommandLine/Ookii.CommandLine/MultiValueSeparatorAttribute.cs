// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.Collections.Generic;
using System.Text;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Specifies a separator for the values of multi-value arguments
    /// </summary>
    /// <remarks>
    /// <note>
    ///   If you specify a separator for a multi-value argument, it will <em>not</em> be possible
    ///   to use the separator character in the individual argument values. There is no way to escape it.
    /// </note>
    /// <para>
    ///   Normally, the values for a multi-value argument can only be specified by specifying the argument
    ///   multiple times, e.g. by using <c>-Sample Value1 -Sample Value2</c>. If you specify the
    ///   <see cref="MultiValueSeparatorAttribute"/> it allows you to specify multiple values with a single
    ///   argument by separating them with the specified separator. For example, if the separator is set
    ///   to a comma, you can use <c>-Sample Value1,Value2</c>. In this example, it is no longer possible
    ///   to have an argument value containing a comma.
    /// </para>
    /// <para>
    ///   Even if the <see cref="MultiValueSeparatorAttribute"/> is specified it is still possible to use
    ///   multiple arguments to specify multiple values. For example, using a comma as the separator, 
    ///   <c>-Sample Value1,Value2 -Sample Value3</c> will mean the argument "Sample" has three values.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "It's allowed to derive from this attribute to allow custom determination of the separator.")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class MultiValueSeparatorAttribute : Attribute
    {
        private readonly string _separator;

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
        /// The separator for the argument values.
        /// </value>
        /// <remarks>
        /// <note>
        ///   If you specify a separator for a multi-value argument, it will <em>not</em> be possible
        ///   to use the separator character in the individual argument values. There is no way to escape it.
        /// </note>
        /// </remarks>
        public virtual string Separator
        {
            get { return _separator; }
        }
    }
}
