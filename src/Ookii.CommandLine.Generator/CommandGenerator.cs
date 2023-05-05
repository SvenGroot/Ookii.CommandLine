using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class CommandGenerator
{
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

        // TODO: Providers with custom command lists.
        foreach (var command in _commands)
        {
            var isGenerated = command.Attributes != null;
            var useCustomParsing = command.Type.ImplementsInterface(_typeHelper.ICommandWithCustomParsing);
            var commandTypeName = command.Type.ToDisplayString();
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

            var attributes = command.Attributes ?? new ArgumentsClassAttributes(command.Type, _typeHelper, _context);
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
                if (isGenerated)
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
}
