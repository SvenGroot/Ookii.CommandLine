using System;
using System.Diagnostics;

namespace Ookii.CommandLine.Support;

/// <summary>
/// Represents information about a dictionary argument determined by the source generator.
/// </summary>
/// <remarks>
/// This class is used by the source generator when using the <see cref="GeneratedParserAttribute"/>
/// attribute. It should not normally be used by other code.
/// </remarks>
/// <threadsafety static="true" instance="false"/>
/// <typeparam name="TKey">The key type of the dictionary.</typeparam>
/// <typeparam name="TValue">The value type of the dictionary.</typeparam>
public class GeneratedDictionaryArgument<TKey, TValue> : GeneratedArgumentBase
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedDictionaryArgument{TKey, TValue}"/>
    /// class.
    /// </summary>
    /// <param name="info">The argument creation information.</param>
    public GeneratedDictionaryArgument(in ArgumentCreationInfo info) : base(info)
    {
        Debug.Assert(info.Kind == ArgumentKind.Dictionary);
        Debug.Assert(typeof(TKey) == info.KeyType);
        Debug.Assert(typeof(TValue) == info.ValueType);
    }

    private protected override IValueHelper CreateDictionaryValueHelper()
        => new DictionaryValueHelper<TKey, TValue>(DictionaryInfo!.AllowDuplicateKeys, AllowNull);

    private protected override IValueHelper CreateMultiValueHelper() => throw new NotImplementedException();
}
