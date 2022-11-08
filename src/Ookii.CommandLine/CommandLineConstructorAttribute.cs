// Copyright (c) Sven Groot (Ookii.org)
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
    }
}
