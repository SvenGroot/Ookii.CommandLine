// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.Collections.Generic;

namespace Ookii.CommandLine
{
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
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class AllowDuplicateDictionaryKeysAttribute : Attribute
    {
    }
}
