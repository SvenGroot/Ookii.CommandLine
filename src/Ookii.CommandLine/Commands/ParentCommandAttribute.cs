using System;
using System.Diagnostics.CodeAnalysis;

namespace Ookii.CommandLine.Commands;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ParentCommandAttribute : Attribute
{
    public ParentCommandAttribute(string parentCommandTypeName)
    {
        ParentCommandTypeName = parentCommandTypeName ?? throw new ArgumentNullException(nameof(parentCommandTypeName));
    }

    public ParentCommandAttribute(Type parentCommandType)
    {
        ParentCommandTypeName = parentCommandType?.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(parentCommandType));
    }

    public string ParentCommandTypeName { get; }

#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("Trimming cannot be used when determining commands via reflection. Use the GeneratedCommandManagerAttribute instead.")]
#endif
    internal Type GetParentCommandType() => Type.GetType(ParentCommandTypeName, true)!;
}
