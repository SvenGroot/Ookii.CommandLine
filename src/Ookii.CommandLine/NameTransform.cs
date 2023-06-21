namespace Ookii.CommandLine;

/// <summary>
/// Indicates how to transform the property, parameter, or method name if an argument doesn't
/// have an explicit name.
/// </summary>
/// <seealso cref="ParseOptionsAttribute.ArgumentNameTransform" qualifyHint="true"/>
/// <seealso cref="ParseOptions.ArgumentNameTransform" qualifyHint="true"/>
/// <seealso cref="ParseOptions.ValueDescriptionTransform" qualifyHint="true"/>
/// <seealso cref="Commands.CommandOptions.CommandNameTransform"/>
/// <seealso cref="NameTransformExtensions.Apply" qualifyHint="true"/>
public enum NameTransform
{
    /// <summary>
    /// The names are used without modification.
    /// </summary>
    None,
    /// <summary>
    /// The names are transformed to PascalCase. This removes all underscores, and the first
    /// character, and every character after an underscore, is changed to uppercase. The case of
    /// other characters is not changed.
    /// </summary>
    PascalCase,
    /// <summary>
    /// The names are transformed to camelCase. Similar to <see cref="PascalCase"/>, but the
    /// first character will not be uppercase.
    /// </summary>
    CamelCase,
    /// <summary>
    /// The names are transformed to dash-case. This removes leading and trailing underscores,
    /// changes all characters to lower-case, replaces underscores with a dash, and reduces
    /// consecutive underscores to a single dash. A dash is inserted before previously
    /// capitalized letters.
    /// </summary>
    DashCase,
    /// <summary>
    /// The names are transformed to snake_case. Similar to <see cref="DashCase"/>, but uses an
    /// underscore instead of a dash.
    /// </summary>
    SnakeCase
}
