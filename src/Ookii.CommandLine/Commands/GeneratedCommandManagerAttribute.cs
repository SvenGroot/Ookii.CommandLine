using System;
using System.Collections.Generic;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Indicates that the target class is a command manager created using source generation.
/// </summary>
/// <remarks>
/// <para>
///   When using this attribute, source generation is used to determine which classes are available
///   at compile time, either in the assembly being compiled, or the assemblies specified using the
///   <see cref="AssemblyNames"/> property. The target class will be modified to inherit from the
///   <see cref="CommandManager"/> class, and should be used instead of the <see cref="CommandManager"/>
///   class to find, create, and run commands.
/// </para>
/// <para>
///   Using a class with this attribute avoids the use of runtime reflection to determine which
///   commands are available, improving performance and allowing your application to be trimmed.
/// </para>
/// <para>
///   To use source generation for the command line arguments of individual commands, use the
///   <see cref="GeneratedParserAttribute"/> attribute on each command class.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
/// <seealso href="https://github.com/SvenGroot/Ookii.CommandLine/blob/main/docs/SourceGeneration.md">Source generation</seealso>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratedCommandManagerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the names of the assemblies that contain the commands that the generated
    /// <see cref="CommandManager"/> will use.
    /// </summary>
    /// <value>
    /// An array with assembly names, or <see langword="null"/> to use the commands from the
    /// assembly containing the generated manager.
    /// </value>
    /// <remarks>
    /// <note>
    ///   The assemblies used must be directly referenced by your project. Dynamically loading
    ///   assemblies is not supported by this attribute; use the
    ///   <see cref="CommandManager(IEnumerable{System.Reflection.Assembly}, CommandOptions?)"/>
    ///   constructor instead for that purpose.
    /// </note>
    /// <para>
    ///   The names in this array can be either just the assembly name, or the full assembly
    ///   identity including version, culture, and public key token.
    /// </para>
    /// </remarks>
    public string[]? AssemblyNames { get; set; }
}
