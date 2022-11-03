using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine.Commands
{
    [Command]
    internal class AutomaticVersionCommand : ICommand
    {
        public int Run()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                Console.Write(Properties.Resources.UnknownVersion);
                return 1;
            }

            var attribute = assembly.GetCustomAttribute<ApplicationFriendlyNameAttribute>();
            var friendlyName = attribute?.Name ?? assembly.GetName().Name ?? string.Empty;
            CommandLineArgument.ShowVersion(assembly, friendlyName);

            return 0;
        }
    }
}
