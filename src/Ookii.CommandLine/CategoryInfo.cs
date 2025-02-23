using System;
using System.ComponentModel;

namespace Ookii.CommandLine;

/// <summary>
/// Provides information about an argument category.
/// </summary>
/// <seealso cref="CommandLineArgument.Category" qualifyHint="true"/>
/// <seealso cref="CommandLineArgumentAttribute.Category" qualifyHint="true"/>
/// <seealso cref="ParseOptionsAttribute.DefaultArgumentCategory" qualifyHint="true"/>
/// <threadsafety static="true" instance="false"/>
public readonly struct CategoryInfo
{
    private readonly CommandLineParser _parser;

    internal CategoryInfo(CommandLineParser parser, Enum category)
    {
        _parser = parser;
        Category = category;
    }

    /// <summary>
    /// Gets the category.
    /// </summary>
    /// <value>
    /// The category's enumeration value. The enumeration type depends on the type used with the
    /// <see cref="CommandLineArgumentAttribute.Category" qualifyHint="true"/> property.
    /// </value>
    public Enum Category { get; }

    /// <summary>
    /// Gets the description of the category, which is displayed in the usage help.
    /// </summary>
    /// <value>
    /// A string with the description of the category.
    /// </value>
    /// <remarks>
    /// <para>
    ///   The description for an argument category can be specified by applying the
    ///   <see cref="DescriptionAttribute"/> attribute to the enumeration value. If no description
    ///   specified, the string representation of the enumeration value is used.
    /// </para>
    /// </remarks>
    public string Description => _parser.GetCategoryDescription(Category);
}
