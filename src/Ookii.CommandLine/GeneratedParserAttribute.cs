using Ookii.CommandLine.Commands;
using System;

namespace Ookii.CommandLine;

/// <summary>
/// Indicates that the target arguments type should use source generation.
/// </summary>
/// <remarks>
/// <para>
///   When this attribute is applied to a class that defines command line arguments, source
///   generation will be used to create a <see cref="CommandLineParser{T}"/> instance for those
///   arguments, instead of the normal approach which uses run-time reflection.
/// </para>
/// <para>
///   To use the generated parser, source generation will add several static methods to the target
///   class: the <see cref="IParserProvider{TSelf}.CreateParser"/> method, and the <see cref="IParser{TSelf}.Parse(ParseOptions?)"/>
///   method and its overload. If you are targeting an older version of .Net than .Net 7.0, the
///   same methods are added, but they will not implement the static interfaces.
/// </para>
/// <para>
///   Using these generted methods allows trimming your application without warnings, as they avoid the
///   regular constructors of the <see cref="CommandLineParser"/> and <see cref="CommandLineParser{T}"/>
///   class.
/// </para>
/// <para>
///   When using source generation with subcommands, you should also use a class with the
///   <see cref="GeneratedCommandManagerAttribute"/> attribute to access the commands.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
/// <seealso href="https://github.com/SvenGroot/Ookii.CommandLine/blob/main/docs/SourceGeneration.md">Source generation</seealso>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratedParserAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value that indicates whether to generate an implementation of the
    /// <see cref="IParser{TSelf}"/> interface for the arguments class.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to generate an implementation of the <see cref="IParser{TSelf}"/>
    /// interface; otherwise, <see langword="false"/>. The default value is <see langword="true"/>,
    /// but see the remarks.
    /// </value>
    /// <remarks>
    /// <para>
    ///   When this property is <see langword="true"/>, the source generator will add static
    ///   <c>Parse</c> methods to the arguments class which will create a parser and parse the
    ///   command line arguments in one go. If using .Net 7.0 or later, this will implement
    ///   the <see cref="IParser{TSelf}"/> interface on the class.
    /// </para>
    /// <para>
    ///   If this property is <see langword="false"/>, only the <see cref="IParserProvider{TSelf}"/>
    ///   interface will be implemented.
    /// </para>
    /// <para>
    ///   The default behavior is to generate an implementation of the <see cref="IParser{TSelf}"/>
    ///   interface methods unless this property is explicitly set to <see langword="false"/>.
    ///   However, if the class is a subcommand (it implements the <see cref="ICommand"/> interface
    ///   and has the <see cref="CommandAttribute"/> attribute), the default is to <em>not</em>
    ///   implement the <see cref="IParser{TSelf}"/> interface unless this property is explicitly
    ///   set to <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool GenerateParseMethods { get; set; } = true;
}
