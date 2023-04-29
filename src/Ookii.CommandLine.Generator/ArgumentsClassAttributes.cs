using Microsoft.CodeAnalysis;

namespace Ookii.CommandLine.Generator;

internal readonly struct ArgumentsClassAttributes
{
    private readonly AttributeData? _parseOptions;
    private readonly AttributeData? _description;
    private readonly AttributeData? _applicationFriendlyName;
    private readonly AttributeData? _command;
    private readonly AttributeData? _generatedParser;
    private readonly List<AttributeData>? _classValidators;
    private readonly List<AttributeData>? _aliases;

    public ArgumentsClassAttributes(ISymbol symbol, TypeHelper typeHelper, SourceProductionContext context)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.CheckType(typeHelper.ParseOptionsAttribute, ref _parseOptions) ||
                attribute.CheckType(typeHelper.DescriptionAttribute, ref _description) ||
                attribute.CheckType(typeHelper.ApplicationFriendlyNameAttribute, ref _applicationFriendlyName) ||
                attribute.CheckType(typeHelper.CommandAttribute, ref _command) ||
                attribute.CheckType(typeHelper.ClassValidationAttribute, ref _classValidators) ||
                attribute.CheckType(typeHelper.AliasAttribute, ref _aliases) ||
                attribute.CheckType(typeHelper.GeneratedParserAttribute, ref _generatedParser))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostics.UnknownAttribute(attribute));
        }
    }

    public AttributeData? ParseOptions => _parseOptions;
    public AttributeData? Description => _description;
    public AttributeData? ApplicationFriendlyName => _applicationFriendlyName;
    public AttributeData? Command => _command;
    public AttributeData? GeneratedParser => _generatedParser;
    public List<AttributeData>? ClassValidators => _classValidators;
    public List<AttributeData>? Aliases => _aliases;
}
