using System;
using System.Collections.Generic;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Indicates that the class with this attribute uses code generation to provide commands to a
/// <see cref="CommandManager"/> class.
/// </summary>
/// <remarks>
/// TODO: Better docs.
/// </remarks>
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
