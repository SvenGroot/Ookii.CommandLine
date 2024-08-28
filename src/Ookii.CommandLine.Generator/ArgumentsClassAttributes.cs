using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Ookii.CommandLine.Generator;

internal readonly struct ArgumentsClassAttributes
{
    private readonly AttributeData? _parseOptions;
    private readonly AttributeData? _description;
    private readonly AttributeData? _usageFooter;
    private readonly AttributeData? _applicationFriendlyName;
    private readonly AttributeData? _command;
    private readonly AttributeData? _generatedParser;
    private readonly AttributeData? _parentCommand;
    private readonly List<AttributeData>? _classValidators;
    private readonly List<AttributeData>? _aliases;

    public ArgumentsClassAttributes(ITypeSymbol symbol, TypeHelper typeHelper)
    {
        // Exclude special types so we don't generate warnings for attributes on framework types.
        for (var current = symbol; current?.SpecialType == SpecialType.None; current = current.BaseType)
        {
            foreach (var attribute in current.GetAttributes())
            {
                AttributeData? generatedParser = null;
                if (attribute.CheckType(typeHelper.GeneratedParserAttribute, ref generatedParser))
                {
                    if (_generatedParser == null)
                    {
                        Debug.Assert(current.SymbolEquals(symbol));
                        _generatedParser = generatedParser;
                    }
                    // If we previously found a base class with generated Parse methods, we don't need to check again.
                    else if (!HasGeneratedBaseWithParseMethods)
                    {
                        HasGeneratedBase = true;
                        var hasParseMethods = generatedParser!.GetNamedArgument("GenerateParseMethods")?.Value as bool?;
                        if (hasParseMethods is bool value)
                        {
                            HasGeneratedBaseWithParseMethods = value;
                        }
                        else
                        {
                            // Default to true if it's not a command.
                            HasGeneratedBaseWithParseMethods = current.GetAttribute(typeHelper.CommandAttribute!) == null
                                || !current.ImplementsInterface(typeHelper.ICommand!);
                        }
                    }

                    continue;
                }

                _ = attribute.CheckType(typeHelper.ParseOptionsAttribute, ref _parseOptions) ||
                    attribute.CheckType(typeHelper.DescriptionAttribute, ref _description) ||
                    attribute.CheckType(typeHelper.UsageFooterAttribute, ref _usageFooter) ||
                    attribute.CheckType(typeHelper.ApplicationFriendlyNameAttribute, ref _applicationFriendlyName) ||
                    attribute.CheckType(typeHelper.CommandAttribute, ref _command) ||
                    attribute.CheckType(typeHelper.ClassValidationAttribute, ref _classValidators) ||
                    attribute.CheckType(typeHelper.ParentCommandAttribute, ref _parentCommand) ||
                    attribute.CheckType(typeHelper.AliasAttribute, ref _aliases);
            }
        }
    }

    public AttributeData? ParseOptions => _parseOptions;
    public AttributeData? Description => _description;
    public AttributeData? UsageFooter => _usageFooter;
    public AttributeData? ApplicationFriendlyName => _applicationFriendlyName;
    public AttributeData? Command => _command;
    public AttributeData? GeneratedParser => _generatedParser;
    public AttributeData? ParentCommand => _parentCommand;
    public List<AttributeData>? ClassValidators => _classValidators;
    public List<AttributeData>? Aliases => _aliases;
    public bool HasGeneratedBase { get; }
    public bool HasGeneratedBaseWithParseMethods { get; }
}
