using Ookii.CommandLine.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Ookii.CommandLine.Support;

/// <summary>
/// This class is for internal use by the source generator, and should not be used in your code.
/// </summary>
public class GeneratedCommandInfo : CommandInfo
{
    private readonly DescriptionAttribute? _descriptionAttribute;
    private readonly IEnumerable<string>? _aliases;
    private readonly Func<ParseOptions?, CommandLineParser>? _createParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedCommandInfo"/> class.
    /// </summary>
    /// <param name="manager">The command manager.</param>
    /// <param name="commandType">The type of the command.</param>
    /// <param name="attribute">The <see cref="CommandAttribute"/>.</param>
    /// <param name="descriptionAttribute">The <see cref="DescriptionAttribute"/>.</param>
    /// <param name="aliasAttributes">A collection of <see cref="AliasAttribute"/> values.</param>
    /// <param name="createParser">A delegate that creates a command line parser for the command when invoked.</param>
    /// <param name="parentCommandType">The type of the parent command.</param>
    public GeneratedCommandInfo(CommandManager manager,
                                Type commandType,
                                CommandAttribute attribute,
                                DescriptionAttribute? descriptionAttribute = null,
                                IEnumerable<AliasAttribute>? aliasAttributes = null,
                                Func<ParseOptions?, CommandLineParser>? createParser = null,
                                Type? parentCommandType = null)
        : base(commandType, attribute, manager, parentCommandType)
    {
        _descriptionAttribute = descriptionAttribute;
        _aliases = aliasAttributes?.Select(a => a.Alias);
        _createParser = createParser;
    }

    /// <inheritdoc/>
    public override string? Description => _descriptionAttribute?.Description;

    /// <inheritdoc/>
    public override bool UseCustomArgumentParsing => false;

    /// <inheritdoc/>
    public override IEnumerable<string> Aliases => _aliases ?? Enumerable.Empty<string>();

    /// <inheritdoc/>
    public override CommandLineParser CreateParser()
    {
        if (_createParser == null)
        {
            throw new InvalidOperationException(Properties.Resources.NoParserForCustomParsingCommand);
        }

        return _createParser(Manager.Options);
    }

    /// <inheritdoc/>
    public override ICommandWithCustomParsing CreateInstanceWithCustomParsing()
        => throw new InvalidOperationException(Properties.Resources.NoCustomParsing);
}
