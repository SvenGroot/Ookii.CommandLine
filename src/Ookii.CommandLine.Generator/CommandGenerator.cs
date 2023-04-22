using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class CommandGenerator
{
    private readonly TypeHelper _typeHelper;
    private readonly SourceProductionContext _context;
    private readonly List<(INamedTypeSymbol Type, AttributeData CommandAttribute, AttributeData? DescriptionAttribute,
        List<AttributeData>? AliasAttributes)> _commands = new();

    private readonly List<INamedTypeSymbol> _providers = new();

    public CommandGenerator(TypeHelper typeHelper, SourceProductionContext context)
    {
        _typeHelper = typeHelper;
        _context = context;
    }

    public void AddCommand(INamedTypeSymbol type, AttributeData commandAttribute, AttributeData? descriptionAttribute, List<AttributeData>? aliasAttributes)
    {
        _commands.Add((type, commandAttribute, descriptionAttribute, aliasAttributes));
    }

    public void AddProvider(INamedTypeSymbol provider)
    {
        _providers.Add(provider);
    }

    public void Generate()
    {
        foreach (var provider in _providers)
        {
            var source = GenerateProvider(provider);
            if (source != null)
            {
                _context.AddSource(provider.ToDisplayString().ToIdentifier(".g.cs"), SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    private string? GenerateProvider(INamedTypeSymbol provider)
    {
        AttributeData? descriptionAttribute = null;
        foreach (var attribute in provider.ContainingAssembly.GetAttributes())
        {
            if (attribute.AttributeClass?.DerivesFrom(_typeHelper.AssemblyDescriptionAttribute) ?? false)
            {
                descriptionAttribute = attribute;
                break;
            }
        }

        var builder = new SourceBuilder(provider.ContainingNamespace);
        builder.AppendLine($"partial class {provider.Name} : Ookii.CommandLine.Support.CommandProvider");
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

            builder.AppendLine($", {command.CommandAttribute.CreateInstantiation()}");
            if (command.DescriptionAttribute != null)
            {
                builder.AppendLine($", descriptionAttribute: {command.DescriptionAttribute.CreateInstantiation()}");
            }

            if (command.AliasAttributes != null)
            {
                builder.AppendLine($", aliasAttributes: new Ookii.CommandLine.AliasAttribute[] {{ {string.Join(", ", command.AliasAttributes.Select(a => a.CreateInstantiation()))} }}");
            }

            if (!useCustomParsing)
            {
                builder.AppendLine($", createParser: options => {commandTypeName}.CreateParser(options)");
            }

            builder.DecreaseIndent();
            builder.AppendLine(");");
        }

        // Makes sure the function compiles if there are no commands.
        builder.AppendLine("yield break;");
        builder.CloseBlock(); // GetCommandsUnsorted
        builder.AppendLine();

        // TODO: Make optional.
        builder.AppendLine("public static Ookii.CommandLine.Commands.CommandManager CreateCommandManager(Ookii.CommandLine.Commands.CommandOptions? options = null)");
        builder.AppendLine($"    => new Ookii.CommandLine.Commands.CommandManager(new {provider.ToDisplayString()}(), options);");
        builder.CloseBlock(); // class
        return builder.GetSource();
    }
}
