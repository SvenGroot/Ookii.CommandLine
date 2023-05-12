using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class CommandGenerator
{
    #region Nested types

    private class CommandVisitor : SymbolVisitor
    {
        private readonly TypeHelper _typeHelper;

        public CommandVisitor(TypeHelper typeHelper)
        {
            _typeHelper = typeHelper;
        }

        public List<(INamedTypeSymbol, ArgumentsClassAttributes?)> Commands { get; } = new();

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.DeclaredAccessibility == Accessibility.Public && symbol.ImplementsInterface(_typeHelper.ICommand))
            {
                var attributes = new ArgumentsClassAttributes(symbol, _typeHelper, null);
                if (attributes.Command != null)
                {
                    Commands.Add((symbol, attributes));
                }
            }
        }
    }

    #endregion

    private readonly TypeHelper _typeHelper;
    private readonly SourceProductionContext _context;
    private readonly List<(INamedTypeSymbol Type, ArgumentsClassAttributes? Attributes)> _commands = new();
    private readonly List<INamedTypeSymbol> _managers = new();

    public CommandGenerator(TypeHelper typeHelper, SourceProductionContext context)
    {
        _typeHelper = typeHelper;
        _context = context;
    }

    public void AddGeneratedCommand(INamedTypeSymbol type, ArgumentsClassAttributes attributes)
    {
        _commands.Add((type, attributes));
    }

    public void AddCommand(INamedTypeSymbol type)
    {
        _commands.Add((type, null));
    }

    public void AddManager(INamedTypeSymbol provider)
    {
        _managers.Add(provider);
    }

    public void Generate()
    {
        foreach (var manager in _managers)
        {
            var source = GenerateManager(manager);
            if (source != null)
            {
                _context.AddSource(manager.ToDisplayString().ToIdentifier(".g.cs"), SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    private string? GenerateManager(INamedTypeSymbol manager)
    {
        AttributeData? descriptionAttribute = null;
        foreach (var attribute in manager.ContainingAssembly.GetAttributes())
        {
            if (attribute.AttributeClass?.DerivesFrom(_typeHelper.AssemblyDescriptionAttribute) ?? false)
            {
                descriptionAttribute = attribute;
                break;
            }
        }

        var builder = new SourceBuilder(manager.ContainingNamespace);
        builder.AppendLine($"partial class {manager.Name} : Ookii.CommandLine.Commands.CommandManager");
        builder.OpenBlock();
        builder.AppendLine("private class GeneratedProvider : Ookii.CommandLine.Support.CommandProvider");
        builder.OpenBlock();
        builder.AppendLine("public override Ookii.CommandLine.Support.ProviderKind Kind => Ookii.CommandLine.Support.ProviderKind.Generated;");
        builder.AppendLine();
        builder.AppendLine("public override string? GetApplicationDescription()");
        if (descriptionAttribute != null)
        {
            builder.AppendLine($"    => ({descriptionAttribute.CreateInstantiation()}).Description;");
        }
        else
        {
            builder.AppendLine("    => null;");
        }

        builder.AppendLine();
        builder.AppendLine("public override System.Collections.Generic.IEnumerable<Ookii.CommandLine.Commands.CommandInfo> GetCommandsUnsorted(Ookii.CommandLine.Commands.CommandManager manager)");
        builder.OpenBlock();

        var generatedManagerAttribute = manager.GetAttribute(_typeHelper.GeneratedCommandManagerAttribute!)!;
        if (generatedManagerAttribute.GetNamedArgument("AssemblyNames") is TypedConstant assemblies)
        {
            foreach (var assembly in assemblies.Values)
            {
                var commands = GetCommands(assembly.Value as string, manager);
                if (commands == null)
                {
                    return null;
                }

                foreach (var (command, attributes) in commands)
                {
                    GenerateCommand(builder, command, attributes);
                }
            }
        }
        else
        {
            foreach (var (command, attributes) in _commands)
            {
                GenerateCommand(builder, command, attributes);
            }
        }

        // Makes sure the function compiles if there are no commands.
        builder.AppendLine("yield break;");
        builder.CloseBlock(); // GetCommandsUnsorted
        builder.CloseBlock(); // provider class
        builder.AppendLine();
        builder.AppendLine($"public {manager.Name}(Ookii.CommandLine.Commands.CommandOptions? options = null)");
        builder.AppendLine($"    : base(new GeneratedProvider(), options)");
        builder.OpenBlock();
        builder.CloseBlock(); // ctor
        builder.CloseBlock(); // manager class
        return builder.GetSource();
    }

    private void GenerateCommand(SourceBuilder builder, INamedTypeSymbol commandType, ArgumentsClassAttributes? commandAttributes)
    {
        var useCustomParsing = commandType.ImplementsInterface(_typeHelper.ICommandWithCustomParsing);
        var commandTypeName = commandType.ToDisplayString();
        if (useCustomParsing)
        {
            builder.AppendLine($"yield return new Ookii.CommandLine.Support.GeneratedCommandInfoWithCustomParsing<{commandTypeName}>(");
        }
        else
        {
            builder.AppendLine("yield return new Ookii.CommandLine.Support.GeneratedCommandInfo(");
        }

        builder.IncreaseIndent();
        builder.AppendLine("manager");
        if (!useCustomParsing)
        {
            builder.AppendLine($", typeof({commandTypeName})");
        }

        var attributes = commandAttributes ?? new ArgumentsClassAttributes(commandType, _typeHelper, _context);
        builder.AppendLine($", {attributes.Command!.CreateInstantiation()}");
        if (attributes.Description != null)
        {
            builder.AppendLine($", descriptionAttribute: {attributes.Description.CreateInstantiation()}");
        }

        if (attributes.Aliases != null)
        {
            builder.AppendLine($", aliasAttributes: new Ookii.CommandLine.AliasAttribute[] {{ {string.Join(", ", attributes.Aliases.Select(a => a.CreateInstantiation()))} }}");
        }

        if (!useCustomParsing)
        {
            if (attributes.GeneratedParser != null)
            {
                builder.AppendLine($", createParser: options => {commandTypeName}.CreateParser(options)");
            }
            else
            {
                builder.AppendLine($", createParser: options => new CommandLineParser<{commandTypeName}>(options)");
            }
        }

        builder.DecreaseIndent();
        builder.AppendLine(");");
    }

    private IEnumerable<(INamedTypeSymbol, ArgumentsClassAttributes?)>? GetCommands(string? assemblyName, ITypeSymbol manager)
    {
        if (assemblyName == null)
        {
            _context.ReportDiagnostic(Diagnostics.InvalidAssemblyName(manager, "null"));
            return null;
        }

        AssemblyIdentity? identity = null;
        if (assemblyName.Contains(","))
        {
            if (!AssemblyIdentity.TryParseDisplayName(assemblyName, out identity))
            {
                _context.ReportDiagnostic(Diagnostics.InvalidAssemblyName(manager, assemblyName));
                return null;
            }

            if (_typeHelper.Compilation.Assembly.Identity.Equals(identity))
            {
                return _commands;
            }
        }
        else if (_typeHelper.Compilation.Assembly.Name == assemblyName)
        {
            return _commands;
        }

        IAssemblySymbol? foundAssembly = null;
        foreach (var reference in  _typeHelper.Compilation.References)
        {
            if (_typeHelper.Compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
            {
                if (identity != null ? identity.Equals(assembly.Identity) : assembly.Name == assemblyName)
                {
                    foundAssembly = assembly;
                    break;
                }
            }
        }

        if (foundAssembly == null)
        {
            _context.ReportDiagnostic(Diagnostics.UnknownAssemblyName(manager, assemblyName));
            return null;
        }

        var visitor = new CommandVisitor(_typeHelper);
        visitor.VisitAssembly(foundAssembly);
        return visitor.Commands;
    }
}
