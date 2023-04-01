using System;
using System.Collections.Generic;

namespace Ookii.CommandLine.Support;

public interface IArgumentProvider
{
    public Type ArgumentsType { get; }

    public string ApplicationFriendlyName { get; }

    public string Description { get; }

    public ParseOptionsAttribute? OptionsAttribute { get; }

    public IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser);

    public void RunValidators(CommandLineParser parser);

    public object CreateInstance(CommandLineParser parser);
}
