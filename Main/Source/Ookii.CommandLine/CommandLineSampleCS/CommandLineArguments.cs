// Copyright (c) Sven Groot (Ookii.org) 2012
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at http://ookiicommandline.codeplex.com. This notice, the author's name,
// and all copyright notices must remain intact in all applications,
// documentation, and source files.
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Ookii.CommandLine;

namespace CommandLineSampleCS
{
    /// <summary>
    /// Class that defines the sample's command line arguments.
    /// </summary>
    [Description("Sample command line application. The application parses the command line and prints the results, but otherwise does nothing, and none of the arguments are actually used for anything.")]
    class CommandLineArguments
    {
        public CommandLineArguments([Description("The source data.")] string source,
                                    [Description("The destination data.")] string destination,
                                    [Description("The operation's index. This argument is optional, and the default value is 1."), ValueDescription("Number")] int index = 1)
        {
            // The parameters to the constructor become the positional arguments for the command line application. These can be specified by position as well a name.
            // Because the third parameter is an optional parameter, the corresponding command line argument is also optional, and its default value will be supplied if it is omitted.
            // The Description attribute for each parameter will be used when printing command line usage help.
            // The third parameter uses a custom value description so it will be shown as "[-index] <Number>" in the usage, rather than "[-index] <Int32>"
            if( source == null )
                throw new ArgumentNullException("source");
            if( destination == null )
                throw new ArgumentNullException("destination");

            Source = source;
            Destination = destination;
            Index = index;
        }

        // These properties are just so the Main function can retrieve the values of the positional arguments.
        // They are not used by the CommandLineParser because they have no NamedCommandLineArgument attribute.
        public string Source { get; private set; }
        public string Destination { get; private set; }
        public int Index { get; private set; }

        // This defines a argument that can be set from the command line by using "-id value" or "-id:value" (on Windows, you can also
        // use a forward slash (/) instead of a dash (-) by default; the argument name prefix can be customized using the CommandLineParser constructor property).
        // Because the Position property of the CommandLineArgumentAttribute is set to a non-negative value, this argument is positional and
        // can be supplied by position as well as by name. Positional arguments defined by properties always come after the arguments defined
        // by the constructor parameters, so this will be the fourth positional argument, even though the Position property is set to zero.
        // The position property determines the relative ordering of positional arguments defined by properties, not the actual position.
        // If the argument is not supplied, the CommandLineParser.Parse method will set it to the default value, which is "default" in this case.
        // The Description attribute is used when printing command line usage information.
        [CommandLineArgument("id", DefaultValue = "default", Position=0), Description("Sets the operation ID. The default value is \"default\".")]
        public string Identifier { get; set; }

        // This defines an argument whose name matches the property name ("Date" in this case). Note that the default comparer for
        // named arguments is case insensitive, so "-date" will work as well as "-Date" (or any other capitalization).
        // This argument is not positional, so it can be supplied only by name, for example as "-Date 2011-07-31".
        // This argument uses a nullable value type so you can easily tell when it has not been supplied even when the type is not a reference type.
        // For types other than string, CommandLineParser will use the TypeConverter for the argument's type to try to convert the string to
        // the correct type. You can use your own custom classes or structures for command line arguments as long as you create a TypeConverter for
        // the type.
        // The type conversion from string to DateTime is culture sensitive. Which culture is used is indicated by the CommandLineParser.Culture
        // property, which defaults to the user's current culture. Always pay attention when a conversion is culture specific (this goes for
        // dates, numbers, and some other types) and consider whether the current culture is the right choice for your application. In some cases
        // using CultureInfo.InvariantCulture could be more appropriate.
        [CommandLineArgument(), Description("Provides a date to the application; the format to use depends on your regional settings.")]
        public DateTime? Date { get; set; }

        // Another argument whose name matches the property name ("Count" in this case).
        // This argument uses a custom ValueDescription so it shows up as "-Count <Number>" in the usage rather than as "-Count <Int32>"
        [CommandLineArgument(ValueDescription = "Number"), Description("Provides the count for something to the application. This argument is required.")]
        public int Count { get; set; }

        // Non-positional arguments whose type is "bool" act as a switch; if they are supplied on the command line, their value will be true, otherwise
        // it will be false. You don't need to specify a value, just specify -v to set it to true. You can explicitly set the value by
        // using "-v:true" or "-v:false" if you want, but it is not needed.
        // If you give an argument the type bool?, it will be true if present, null if omitted, and false only when explicitly set to false using "-v:false"
        [CommandLineArgument("v"), Description("Print verbose information; this is an example of a switch argument.")]
        public bool Verbose { get; set; }

        // This is an argument with an array type, which means it can be specified multiple times. Every time it is specified, the value will be added to the array.
        // To set multiple values, simply repeat the -val argument, e.g. "-val foo -val bar -val baz" will set it to an array containing { "foo", "bar", "baz" }
        // Since no default value is specified, the property will be null if -val is not supplied at all.
        // Array arguments don't have to be string arrays, you can use an int[], DateTime[], YourClass[], anything works as long as a TypeConverter exists for the class
        // that can convert from a string to that type.
        [CommandLineArgument("val"), Description("This is an example of an array argument, which can be repeated multiple times to set more than one value.")]
        public string[] Values { get; set; }

        // This is another switch argument, like -v above. In Program.cs, we handle the CommandLineParser.ArgumentParsed event to cancel
        // command line processing when this argument is supplied. That way, we can print usage regardless of what other arguments are
        // present. For more details, see the CommandLineParser.ArgumentParser event handler in Program.cs
        [CommandLineArgument("?"), Description("Displays this help message.")]
        public bool Help { get; set; }
    }
}
