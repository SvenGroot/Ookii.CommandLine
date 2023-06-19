using System;
using System.Collections.Generic;

namespace Ookii.CommandLine;

/// <summary>
/// Indicates that a dictionary argument accepts the same key more than once.
/// </summary>
/// <remarks>
/// <para>
///   If this attribute is applied to an argument whose type is <see cref="Dictionary{TKey,TValue}"/> or
///   <see cref="IDictionary{TKey,TValue}"/>, a duplicate key will simply overwrite the previous value.
/// </para>
/// <para>
///   If this attribute is not applied, a <see cref="CommandLineArgumentException"/> with a <see cref="CommandLineArgumentException.Category"/>
///   of <see cref="CommandLineArgumentErrorCategory.InvalidDictionaryValue"/> will be thrown when a duplicate key is specified.
/// </para>
/// <para>
///   The <see cref="AllowDuplicateDictionaryKeysAttribute"/> is ignored if it is applied to any other type of argument.
/// </para>
/// </remarks>
/// <threadsafety static="true" instance="false"/>
/// <seealso cref="CommandLineArgument.AllowsDuplicateDictionaryKeys"/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AllowDuplicateDictionaryKeysAttribute : Attribute
{
}
