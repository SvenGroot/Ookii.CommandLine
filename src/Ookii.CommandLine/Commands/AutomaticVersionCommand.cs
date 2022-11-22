using System;
using System.Reflection;

namespace Ookii.CommandLine.Commands
{
    [Command]
    internal class AutomaticVersionCommand : ICommand
    {
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
    }
}
