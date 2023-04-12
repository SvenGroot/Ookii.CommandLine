using System;

namespace Ookii.CommandLine;

/// <summary>
/// Indicates that the specified arguments type should use source generation.
/// TODO: Better help.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratedParserAttribute : Attribute
{
}
