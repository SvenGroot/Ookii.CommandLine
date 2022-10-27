using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ookii.CommandLine
{
    [ShellCommand]
    internal class AutomaticVersionCommand : ShellCommand
    {
        public override void Run()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                Console.Write(Properties.Resources.UnknownVersion);
                return;
            }

            var attribute = assembly.GetCustomAttribute<ApplicationFriendlyNameAttribute>();
            var friendlyName = attribute?.Name ?? assembly.GetName().Name ?? string.Empty;
            CommandLineArgument.ShowVersion(assembly, friendlyName);
        }
    }
}
