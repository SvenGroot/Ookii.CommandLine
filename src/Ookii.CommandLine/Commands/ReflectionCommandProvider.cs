using Ookii.CommandLine.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine.Commands;

#if NET6_0_OR_GREATER
[RequiresUnreferencedCode("Trimming is not possible when determining commands using reflection. Use the GeneratedCommandProviderAttribute instead.")]
#endif
internal class ReflectionCommandProvider : CommandProvider
{
    private readonly Assembly? _assembly;
    private readonly IEnumerable<Assembly>? _assemblies;

    public ReflectionCommandProvider(Assembly assembly)
    {
        _assembly = assembly;
    }

    public ReflectionCommandProvider(IEnumerable<Assembly> assemblies)
    {
        _assemblies = assemblies;
        if (_assemblies.Any(a => a == null))
        {
            throw new ArgumentNullException(nameof(assemblies));
        }
    }

    public override ProviderKind Kind => ProviderKind.Reflection;

    public override IEnumerable<CommandInfo> GetCommandsUnsorted(CommandManager manager)
    {
        {
            IEnumerable<Type> types;
            if (_assembly != null)
            {
                types = _assembly.GetTypes();
            }
            else
            {
                Debug.Assert(_assemblies != null);
                types = _assemblies.SelectMany(a => a.GetTypes());
            }

            return from type in types
                   let info = CommandInfo.TryCreate(type, manager)
                   where info != null
                   select info;
        }
    }

    public override string? GetApplicationDescription()
        => (_assembly ?? _assemblies?.FirstOrDefault())?.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
}
