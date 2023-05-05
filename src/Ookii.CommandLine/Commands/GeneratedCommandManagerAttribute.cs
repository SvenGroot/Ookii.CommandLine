using System;

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
}
