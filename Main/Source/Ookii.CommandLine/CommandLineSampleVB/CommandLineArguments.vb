' $Id: CommandLineArguments.vb 28 2011-06-26 06:42:21Z sgroot $
'
Imports System.ComponentModel
Imports Ookii.CommandLine

''' <summary>
''' Class that defines the sample's command line arguments.
''' </summary>
<Description("Sample command line application. The application parses the command line and prints the results, but otherwise does nothing, and none of the arguments are actually used for anything.")> _
Class CommandLineArguments

    Public Sub New(<Description("The source data.")> ByVal source As String,
                   <Description("The destination data.")> ByVal destination As String,
                   <Description("The operation's index. This argument is optional, and the default value is 1.")> Optional ByVal index As Integer = 1)
        ' The parameters to the constructor become the positional arguments for the command line application.
        ' Because the third parameter is an optional parameter, the corresponding command line argument is also optional, and its default value will be supplied if it is omitted.
        ' The Description attribute for each parameter will be used when printing command line usage information.
        If source Is Nothing Then _
            Throw New ArgumentNullException("source")
        If destination Is Nothing Then _
            Throw New ArgumentNullException("destination")

        Me.Source = source
        Me.Destination = destination
        Me.Index = index
    End Sub

    ' These properties are just so the Main function can retrieve the values of the positional arguments.
    ' They are not used by the CommandLineParser because they have no NamedCommandLineArgument attribute.
    Public Property Source As String
    Public Property Destination As String
    Public Property Index As Integer

    ' This defines a named argument that can be set from the command line by using /id:value (or -id:value on Unix by default; the named argument switch
    ' can be customized using the CommandLineParser.NamedArgumentSwitch property). If it is not supplied, the CommandLineParser.Parse method
    ' will set it to the supplied default value, which is "default" in this case.
    ' The Description attribute is used when printing command line usage information.
    <CommandLineArgument("id", DefaultValue:="default"), Description("Sets the operation ID. The default value is ""default"".")>
    Public Property Identifier As String

    ' This defines a named argument whose name matches the property name ("Date" in this case). Note that the default comparer for
    ' named arguments is case insensitive, so "/date" will work as well as "/Date" (or any other capitalization).
    ' This named argument uses a nullable value type so you can easily tell when it has not been specified even when the type is not a reference type.
    ' For types other than string, CommandLineParser will use the TypeConverter for the argument's type to try to convert the string to
    ' the correct type. You can use your own custom classes or structures for command line arguments as long as you create a TypeConverter for
    ' the type.
    ' If you need more control over the conversion, it can be more beneficial to create a string argument and do the conversion
    ' yourself. For example, DateTime's TypeConverter uses the current user's locale; if you want to force a specific format
    ' you could use a string argument and parse it manually with DateTime.ParseExact.
    <CommandLineArgument(), Description("Provides a date to the application; the format to use depends on your locale.")>
    Public Property [Date] As Date?

    ' Another named argument whose name matches the property name ("Count" in this case).
    ' This argument uses a custom ValueDescription so it shows up as "[/Count <Number>]" in the usage rather than as "[/Count <Int32>]"
    <CommandLineArgument(ValueDescription:="Number"), Description("Provides the count for something to the application.")>
    Public Property Count As Integer

    ' Named arguments whose type is "Boolean" act as a switch; if they are supplied on the command line, their value will be true, otherwise
    ' it will be false. You don't need to specify a value, just specify /v to set it to true. You can explicitly set the value by
    ' using /v:true or /v:false if you want, but it is not needed.
    <CommandLineArgument("v"), Description("Print verbose information.")>
    Public Property Verbose As Boolean

    ' This is a named argument with an array type, which means it can be specified multiple times. Every time it is specified, the value will be added to the array.
    ' To set multiple values, simply repeat the /val argument, e.g. /val:foo /val:bar /val:baz will set it to an array containing { "foo", "bar", "baz" }
    ' Since no default value is specified, the property will be Nothing if /val is not supplied at all.
    ' Array arguments don't have to be string arrays, you can use an Integer(), DateTime(), YourClass(), anything works as long as a TypeConverter exists for the class
    ' that can convert from a string to that type.
    <CommandLineArgument("val"), Description("This is an example of an array argument, which can be repeated multiple times to set more than one value.")>
    Public Property Values As String()

    ' This is another switch argument, like /v above. In Program.cs, we handle the CommandLineParser.ArgumentParsed event to cancel
    ' command line processing when this argument is supplied. That way, we can print usage regardless of what other arguments are
    ' present. For more details, see the CommandLineParser.ArgumentParser event handler in Program.cs
    <CommandLineArgument("?"), Description("Displays this help message.")>
    Public Property Help As Boolean

End Class
