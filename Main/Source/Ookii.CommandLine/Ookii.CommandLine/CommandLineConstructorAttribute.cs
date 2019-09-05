// Copyright (c) Sven Groot (Ookii.org)
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;

namespace Ookii.CommandLine
{
    /// <summary>
    /// Indicates the constructor that should be used by the <see cref="CommandLineParser"/> class, if a class has multiple public constructors.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   If a class has only one public constructor, it is not necessary to use this attribute.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class CommandLineConstructorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineConstructorAttribute"/> class.
        /// </summary>
        public CommandLineConstructorAttribute()
        {
        }
    }
}
