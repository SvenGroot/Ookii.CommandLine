﻿using Ookii.CommandLine.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

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
    /// This class is for internal use by the source generator, and should not be used in your code.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="commandType"></param>
    /// <param name="attribute"></param>
    /// <param name="descriptionAttribute"></param>
    /// <param name="aliasAttributes"></param>
    /// <param name="createParser"></param>
    public GeneratedCommandInfo(CommandManager manager,
                                Type commandType,
                                CommandAttribute attribute,
                                DescriptionAttribute? descriptionAttribute = null,
                                IEnumerable<AliasAttribute>? aliasAttributes = null,
                                Func<ParseOptions?, CommandLineParser>? createParser = null)
        : base(commandType, attribute, manager)
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