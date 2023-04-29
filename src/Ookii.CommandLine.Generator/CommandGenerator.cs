using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Ookii.CommandLine.Generator;

internal class CommandGenerator
{
    private readonly TypeHelper _typeHelper;
    private readonly SourceProductionContext _context;
    private readonly List<(INamedTypeSymbol Type, ArgumentsClassAttributes? Attributes)> _commands = new();

    private readonly List<INamedTypeSymbol> _providers = new();

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
        builder.AppendLine();

        // TODO: Make optional.
        builder.AppendLine("public static Ookii.CommandLine.Commands.CommandManager CreateCommandManager(Ookii.CommandLine.Commands.CommandOptions? options = null)");
        builder.AppendLine($"    => new Ookii.CommandLine.Commands.CommandManager(new {provider.ToDisplayString()}(), options);");
        builder.CloseBlock(); // class
        return builder.GetSource();
    }
}
