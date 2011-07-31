Imports System.Reflection
Imports Ookii.CommandLine

Public Module Program

    ' Create a command line parser for the type that defines this sample's command line arguments.
    ' The parser is a field here with the WithEvents keyword to more easily handle the CommandLineParser.ArgumentParsed event;
    ' of course, you can also use the AddHandler keyword to handle that event.
    ' The ArgumentParsed event is used by this sample to stop processing after the -? argument is received.
    Private WithEvents _parser As New CommandLineParser(GetType(CommandLineArguments))

    Public Sub Main(ByVal args() As String)
        Try
            ' Parse the command line arguments. This will throw a CommandLineArgumentException if the right arguments weren't supplied
            Dim arguments As CommandLineArguments = DirectCast(_parser.Parse(args), CommandLineArguments)

            If arguments Is Nothing Then
                ' Nothing means that parsing was cancelled by our ArgumentParsed event handler,
                ' which indicates the -? argument was supplied, so we should print usage.
                _parser.WriteUsageToConsole()
            Else
                ' We use the LineWrappingTextWriter to neatly wrap console output.
                Using writer As LineWrappingTextWriter = LineWrappingTextWriter.ForConsoleOut()
                    ' Print the full command line as received by the application
                    writer.WriteLine("The command line was: {0}", Environment.CommandLine)
                    writer.WriteLine()
                    ' This application doesn't do anything useful, it's just a sample of using CommandLineParser after all. We use reflection to print
                    ' the values of all the properties of the sample's CommandLineArguments class, which correspond to the sample's command line arguments.
                    writer.WriteLine("The following arguments were provided:")

                    Dim properties() As PropertyInfo = GetType(CommandLineArguments).GetProperties()
                    For Each prop As PropertyInfo In properties
                        If prop.PropertyType.IsArray Then
                            ' Print a list of all the values for an array argument.
                            writer.Write("{0}: ", prop.Name)
                            Dim array As Array = CType(prop.GetValue(arguments, Nothing), Array)
                            If array Is Nothing Then
                                writer.WriteLine("(null)")
                            Else
                                writer.Write("{ ")
                                For x = 0 To array.GetUpperBound(0)
                                    If x > 0 Then
                                        writer.Write(", ")
                                    End If
                                    writer.Write(array.GetValue(x))
                                Next
                                writer.WriteLine(" }")
                            End If
                        Else
                            writer.WriteLine("{0}: {1}", prop.Name, If(prop.GetValue(arguments, Nothing), "(null)"))
                        End If
                    Next
                End Using
            End If
        Catch ex As CommandLineArgumentException
            ' We use the LineWrappingTextWriter to neatly wrap console output.
            Using writer As LineWrappingTextWriter = LineWrappingTextWriter.ForConsoleError()
                ' Tell the user what went wrong
                writer.WriteLine(ex.Message)
                writer.WriteLine()
            End Using
            ' Print usage information so the user can see how to correctly invoke the program
            _parser.WriteUsageToConsole()
        End Try
    End Sub

    Private Sub _parser_ArgumentParsed(ByVal sender As Object, ByVal e As Ookii.CommandLine.ArgumentParsedEventArgs) Handles _parser.ArgumentParsed
        ' When we receive the -? argument, we immediately cancel processing. That way, CommandLineParser(Of T).Parse will
        ' return Nothing, and the Main method will display usage, even if the correct number of positional arguments was supplied.
        ' Try it: just call the sample with "CommandLineSampleVB.exe foo bar -?", which will print usage even though both the source and destination
        ' arguments are supplied.
        If e.Argument.ArgumentName = "?" Then
            e.Cancel = True
        End If
    End Sub

End Module
