using Ookii.CommandLine.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ookii.CommandLine.Support;

/// <summary>
/// This class is for internal use by the source generator, and should not be used in your code.
/// </summary>
/// <typeparam name="T">The command class.</typeparam>
public class GeneratedCommandInfoWithCustomParsing<T> : GeneratedCommandInfo
    where T : class, ICommandWithCustomParsing, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedCommandInfoWithCustomParsing{T}"/> class.
    /// </summary>
    /// <param name="manager">The command manager.</param>
    /// <param name="attribute">The <see cref="CommandAttribute"/>.</param>
    /// <param name="descriptionAttribute">The <see cref="DescriptionAttribute"/>.</param>
    /// <param name="aliasAttributes">A collection of <see cref="AliasAttribute"/> values.</param>
    /// <param name="parentCommandType">The type of the parent command.</param>
    public GeneratedCommandInfoWithCustomParsing(CommandManager manager,
                                                 CommandAttribute attribute,
                                                 DescriptionAttribute? descriptionAttribute = null,
                                                 IEnumerable<AliasAttribute>? aliasAttributes = null,
                                                 Type? parentCommandType = null)
        : base(manager, typeof(T), attribute, descriptionAttribute, aliasAttributes, parentCommandType: parentCommandType)
    {
    }

    /// <inheritdoc/>
    public override bool UseCustomArgumentParsing => true;

    /// <inheritdoc/>
    public override ICommandWithCustomParsing CreateInstanceWithCustomParsing() => new T();
}
