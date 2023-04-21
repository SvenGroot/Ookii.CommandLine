using Ookii.CommandLine.Support;
using Ookii.CommandLine.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Ookii.CommandLine.Commands;

[Command]
internal class AutomaticVersionCommand : ICommand
{
    private class ArgumentProvider : GeneratedArgumentProvider
    {
        private readonly LocalizedStringProvider _stringProvider;

        public ArgumentProvider(LocalizedStringProvider stringProvider)
            : base(typeof(AutomaticVersionCommand), null, null, null, null)
        {
            _stringProvider = stringProvider;
        }

        public override bool IsCommand => true;

        public override string Description => _stringProvider.AutomaticVersionCommandDescription();

        public override object CreateInstance(CommandLineParser parser) => new AutomaticVersionCommand(parser);

        public override IEnumerable<CommandLineArgument> GetArguments(CommandLineParser parser)
        {
            yield break;
        }
    }

    private readonly CommandLineParser _parser;

    public AutomaticVersionCommand(CommandLineParser parser)
    {
        _parser = parser;
    }

    public int Run()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            Console.WriteLine(Properties.Resources.UnknownVersion);
            return 1;
        }

        // We can't use _parser.ApplicationFriendlyName because we're interested in the entry
        // assembly, not the one containing this command.
        var attribute = assembly.GetCustomAttribute<ApplicationFriendlyNameAttribute>();
        var friendlyName = attribute?.Name ?? assembly.GetName().Name ?? string.Empty;
        CommandLineArgument.ShowVersion(_parser.StringProvider, assembly, friendlyName);
        return 0;
    }

    public static CommandLineParser CreateParser(ParseOptions options)
        => new(new ArgumentProvider(options.StringProvider), options);
}
