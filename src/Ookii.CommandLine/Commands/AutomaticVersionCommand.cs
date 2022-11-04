﻿using System;
using System.Reflection;

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