using System;
using System.Collections.Immutable;

namespace Ookii.CommandLine;

[Serializable]
public class AmbiguousPrefixAliasException : CommandLineArgumentException
{
    public AmbiguousPrefixAliasException(string? message, string? argumentName, ImmutableArray<string> possibleMatches)
        : base(message, argumentName, CommandLineArgumentErrorCategory.AmbiguousPrefixAlias)
    {
        PossibleMatches = possibleMatches;
    }

    public ImmutableArray<string> PossibleMatches { get; }
}
