using ArgumentDependencies;
using Ookii.CommandLine;

var arguments = ProgramArguments.Parse();
if (arguments == null)
{
    return 1;
}

using var writer = LineWrappingTextWriter.ForConsoleOut();
if (arguments.Path != null)
{
    writer.WriteLine($"Path: {arguments.Path.FullName}");
}
else
{
    writer.WriteLine($"IP address: {arguments.Ip}");
    writer.WriteLine($"Port: {arguments.Port}");
}

return 0;
