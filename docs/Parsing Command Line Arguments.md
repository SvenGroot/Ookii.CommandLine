# Parsing command line arguments

When you have defined the command line arguments, you can now parse the command line to determine the argument values.

In order to do this, you first must create an instance of the {{CommandLineParser}} class, passing the type of the class that defines the parameters to the constructor. Then call the {{CommandLineParser.Parse}} method, passing the arguments your application received.

If argument parsing is successful, the {{CommandLineParser.Parse}} method will create a new instance of the class defining the arguments, passing the values parsed from the command line to the constructor parameters (if any). It will then set the value of each property to the value of the corresponding argument. This is not done in any particular order, so do not write code that makes assumptions about this. Finally, it will return this instance.

If argument parsing fails, the {{CommandLineParser.Parse}} method will throw a {{CommandLineArgumentException}} exception. Argument parsing can fail because not all required arguments were specified, an argument name was supplied without a value for an argument that’s not a switch, an unknown argument name was supplied, a non-multi-value argument was specified more than once, too many positional argument values were supplied, or argument value conversion failed for one of the arguments. Check the {{CommandLineParser.Category}} property to determine the cause of the exception.

A typical usage sample for the {{CommandLineParser}} class is as follows:

{code: C#}
static void Main(string[]() args)
{
    CommandLineParser parser = new CommandLineParser(typeof(MyArguments));
    try
    {
        MyArguments arguments = (MyArguments)parser.Parse(args);
        RunApplication(arguments);
    }
    catch( CommandLineArgumentException ex )
    {
        Console.WriteLine(ex.Message);
        parser.WriteUsageToConsole();
    }
}
{code: C#}

This sample tries to parse the arguments, and if it fails prints the error message and usage help to the console.

The {{CommandLineParser}} class provides several ways to customize the parsing behavior; check the class library documentation for more details.