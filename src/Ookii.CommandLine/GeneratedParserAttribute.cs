using Ookii.CommandLine.Commands;
using System;

namespace Ookii.CommandLine;

/// <summary>
/// Indicates that the specified arguments type should use source generation.
/// TODO: Better help.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratedParserAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value that indicates whether to generate static <c>Parse</c> methods for the
    /// arguments class.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to generate static <c>Parse</c> methods; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>, but see the remarks.
    /// </value>
    /// <remarks>
    /// <para>
    ///   When this property is <see langword="true"/>, the source generator will add static
    ///   <c>Parse</c> methods to the arguments class which will create a parser and parse the
    ///   command line arguments in one go. If using .Net 7.0 or later, this will implement
    ///   the <see cref="IParser{TSelf}"/> interface on the class.
    /// </para>
    /// <para>
    ///   The default behavior is to generate the static <c>Parse</c> methods unless this property
    ///   is explicitly set to <see langword="false"/>. However, if the class is a command (it
    ///   implements the <see cref="ICommand"/> interface and has the <see cref="CommandAttribute"/>
    ///   attribute), the default is to <em>not</em> generate the static <c>Parse</c> methods
    ///   unless this property is explicitly set to <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool GenerateParseMethods { get; set; } = true;
}
