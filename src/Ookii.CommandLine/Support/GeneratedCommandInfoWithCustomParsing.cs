using Ookii.CommandLine.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ookii.CommandLine.Support;

/// <inheritdoc/>
public class GeneratedCommandInfoWithCustomParsing<T> : GeneratedCommandInfo
    where T : class, ICommandWithCustomParsing, new()
{
    /// <inheritdoc/>
    public GeneratedCommandInfoWithCustomParsing(CommandManager manager,
                                                 CommandAttribute attribute,
                                                 DescriptionAttribute? descriptionAttribute = null,
                                                 IEnumerable<AliasAttribute>? aliasAttributes = null)
        : base(manager, typeof(T), attribute, descriptionAttribute, aliasAttributes)
    {
    }

    /// <inheritdoc/>
    public override bool UseCustomArgumentParsing => true;

    /// <inheritdoc/>
    public override ICommandWithCustomParsing CreateInstanceWithCustomParsing() => new T();
}
