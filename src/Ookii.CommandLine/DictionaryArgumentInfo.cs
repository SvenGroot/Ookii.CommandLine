using Ookii.CommandLine.Conversion;
using System;

namespace Ookii.CommandLine;

/// <summary>
/// Provides information that only applies to dictionary arguments.
/// </summary>
public sealed class DictionaryArgumentInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryArgumentInfo"/> class.
    /// </summary>
    /// <param name="allowDuplicateDictionaryKeys">
    /// <see langword="true"/> if duplicate dictionary keys are allowed; otherwise,
    /// <see langword="false"/>.
    /// </param>
    /// <param name="keyType">The type of the dictionary's keys.</param>
    /// <param name="valueType">The type of the dictionary's values.</param>
    /// <param name="keyValueSeparator">The separator between the keys and values.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="keyType"/> or <paramref name="valueType"/> or <paramref name="keyValueSeparator"/>
    /// is <see langword="null"/>.
    /// </exception>
    public DictionaryArgumentInfo(bool allowDuplicateDictionaryKeys, Type keyType, Type valueType, string keyValueSeparator)
    {
        AllowDuplicateDictionaryKeys = allowDuplicateDictionaryKeys;
        KeyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
        ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        KeyValueSeparator = keyValueSeparator ?? throw new ArgumentNullException(nameof(keyValueSeparator));
    }

    /// <summary>
    /// Gets a value indicating whether this argument, if it is a dictionary argument, allows duplicate keys.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this argument allows duplicate keys; otherwise, <see langword="false"/>.
    /// </value>
    /// <seealso cref="AllowDuplicateDictionaryKeysAttribute"/>
    public bool AllowDuplicateDictionaryKeys { get; }

    /// <summary>
    /// Gets the type of the keys of a dictionary argument.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the keys in the dictionary.
    /// </value>
    public Type KeyType { get; }

    /// <summary>
    /// Gets the type of the values of a dictionary argument.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the values in the dictionary.
    /// </value>
    public Type ValueType { get; }

    /// <summary>
    /// Gets the separator for key/value pairs if this argument is a dictionary argument.
    /// </summary>
    /// <value>
    /// The custom value specified using the <see cref="KeyValueSeparatorAttribute"/> attribute, or
    /// <see cref="KeyValuePairConverter.DefaultSeparator" qualifyHint="true"/> if no attribute was
    /// present.
    /// </value>
    /// <seealso cref="KeyValueSeparatorAttribute"/>
    public string KeyValueSeparator { get; }
}
