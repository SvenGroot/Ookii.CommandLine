using Ookii.CommandLine.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Ookii.CommandLine.Support;

#if NET6_0_OR_GREATER
[RequiresUnreferencedCode("Command information cannot be statically determined using reflection. Consider using the GeneratedParserAttribute and GeneratedCommandManagerAttribute.", Url = CommandLineParser.UnreferencedCodeHelpUrl)]
#endif
internal class ReflectionCommandProvider : CommandProvider
{
    private readonly Assembly? _assembly;
    private readonly IEnumerable<Assembly>? _assemblies;
    private readonly Assembly _callingAssembly;

    public ReflectionCommandProvider(Assembly assembly, Assembly callingAssembly)
    {
        _assembly = assembly;
        _callingAssembly = callingAssembly;
    }

    public ReflectionCommandProvider(IEnumerable<Assembly> assemblies, Assembly callingAssembly)
    {
        _assemblies = assemblies;
        _callingAssembly = callingAssembly;
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
                   where type.Assembly == _callingAssembly || type.IsPublic
                   let info = CommandInfo.TryCreate(type, manager)
                   where info != null
                   select info;
        }
    }

    public override string? GetApplicationDescription()
        => (_assembly ?? _assemblies?.FirstOrDefault())?.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
}
