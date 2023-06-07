using System;
using System.Collections.Generic;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Indicates that the target class is a command manager created using source generation.
/// </summary>
/// <remarks>
/// <para>
///   When using this attribute, source generation is used to find and instantiate subcommand
///   classes in the current assembly, or the assemblies specified using the <see cref="AssemblyNames"/> 
///   property. The target class will be modified to inherit from the <see cref="CommandManager"/>
///   class, and should be used instead of the <see cref="CommandManager"/> class to find, create,
///   and run commands.
/// </para>
/// <para>
///   To use source generation for the command line arguments of individual commands, use the
///   <see cref="GeneratedParserAttribute"/> attribute on the command class.
/// </para>
/// </remarks>
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
    ///   assemblies is not supported by this method; use the <see cref="CommandManager(IEnumerable{System.Reflection.Assembly}, CommandOptions?)"/>
    ///   constructor instead.
    /// </note>
    /// <para>
    ///   The names in this array can be either just the assembly name, or the full assembly
    ///   identity including version, culture, and public key token.
    /// </para>
    /// </remarks>
    public string[]? AssemblyNames { get; set; }
}
