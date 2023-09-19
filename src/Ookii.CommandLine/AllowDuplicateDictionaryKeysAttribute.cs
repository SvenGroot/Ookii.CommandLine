using System;
using System.Collections.Generic;

namespace Ookii.CommandLine;

/// <summary>
/// Indicates that a dictionary argument accepts the same key more than once.
/// </summary>
/// <remarks>
/// <para>
///   If this attribute is applied to an argument whose type is <see cref="Dictionary{TKey,TValue}"/>
///   or another type that implements the<see cref="IDictionary{TKey,TValue}"/> interface, a
///   duplicate key will simply overwrite the previous value.
/// </para>
/// <para>
///   If this attribute is not applied, a <see cref="CommandLineArgumentException"/> with a
///   the <see cref="CommandLineArgumentException.Category" qualifyHint="true"/> property set to
///   <see cref="CommandLineArgumentErrorCategory.InvalidDictionaryValue" qualifyHint="true"/> will
///   be thrown when a duplicate key is specified.
/// </para>
/// <para>
///   The <see cref="AllowDuplicateDictionaryKeysAttribute"/> is ignored if it is applied to any
///   other type of argument.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="true"/>
/// <seealso cref="DictionaryArgumentInfo.AllowDuplicateKeys" qualifyHint="true"/>
[AttributeUsage(AttributeTargets.Property)]
public sealed class AllowDuplicateDictionaryKeysAttribute : Attribute
{
}
