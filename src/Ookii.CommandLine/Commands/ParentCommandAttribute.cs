using System;
using System.Diagnostics.CodeAnalysis;

namespace Ookii.CommandLine.Commands;

/// <summary>
/// Indicates the parent command for a nested subcommand.
/// </summary>
/// <remarks>
/// <para>
///   If you wish to have a command with nested subcommands, apply this attribute to the children
///   of another command. The <see cref="CommandManager"/> class will only return commands whose
///   <see cref="ParentCommandTypeName"/> property value matches the <see cref="CommandOptions.ParentCommand" qualifyHint="true"/>
///   property.
/// </para>
/// <para>
///   The parent command type should be the type of another command. It may be a command derived
///   from the <see cref="ParentCommand"/> class, but this is not required. The
///   <see cref="ParentCommand"/> class makes implementing nested subcommands easy, but you may
///   also use any command with your own nested subcommand logic as a parent command.
/// </para>
/// <para>
///   To create a hierarchy of subcommands, the command with this attribute may itself also have
///   nested subcommands.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>   
[AttributeUsage(AttributeTargets.Class)]
public sealed class ParentCommandAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParentCommandAttribute"/> class.
    /// </summary>
    /// <param name="parentCommandTypeName">The type name of the parent command class.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parentCommandTypeName"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    ///   This constructor is not compatible with the <see cref="GeneratedCommandManagerAttribute"/>;
    ///   use <see cref="ParentCommandAttribute(Type)"/> instead.
    /// </para>
    /// </remarks>
    public ParentCommandAttribute(string parentCommandTypeName)
    {
        ParentCommandTypeName = parentCommandTypeName ?? throw new ArgumentNullException(nameof(parentCommandTypeName));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentCommandAttribute"/> class.
    /// </summary>
    /// <param name="parentCommandType">The <see cref="Type"/> of the parent command class.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parentCommandType"/> is <see langword="null"/>.
    /// </exception>
    public ParentCommandAttribute(Type parentCommandType)
    {
        ParentCommandTypeName = parentCommandType?.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(parentCommandType));
    }

    /// <summary>
    /// Gets or sets the name of the parent command type.
    /// </summary>
    /// <value>
    /// The type name.
    /// </value>
    public string ParentCommandTypeName { get; }

#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
    internal Type GetParentCommandType() => Type.GetType(ParentCommandTypeName, true)!;
}
