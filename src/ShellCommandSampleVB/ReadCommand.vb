Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports Ookii.CommandLine

''' <summary>
''' This is a sample ShellCommand that can be invoked by specifying "read" as the first argument to the sample application.
''' Shell command argument parsing works just like a regular command line argument class. After the arguments have been parsed,
''' the Run method is invoked to execute the ShellCommand.
''' Check the Program.vb file to see how this command is invoked.
''' </summary>
<ShellCommand("read"), Description("Reads and displays data from a file using the specified encoding, wrapping the text to fit the console.")>
Public Class ReadCommand
    Inherits ShellCommand

    ' A positional argument to specify the file name.
    <CommandLineArgument(Position:=0, Isrequired:=True), Description("The name of the file to read.")>
    Public Property FileName As String

    ' A named argument to specify the encoding.
    ' Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
    ' instantiate the Encoding class ourselves in the run method.
    <CommandLineArgument("Encoding", DefaultValue:="utf-8"), Description("The encoding to use to read the file.")>
    Public Property EncodingName As String

    Public Overrides Sub Run()
        ' This method is invoked after all command line arguments have been parsed
        Try
            ' We use a LineWrappingTextWriter to neatly wrap console output
            Using writer As LineWrappingTextWriter = LineWrappingTextWriter.ForConsoleOut(),
                  reader As StreamReader = New StreamReader(_fileName, Encoding.GetEncoding(EncodingName))
                ' Write the contents of the file to the console
                Dim line As String
                Do
                    line = reader.ReadLine()
                    If line IsNot Nothing Then
                        writer.WriteLine(line)
                    End If
                Loop Until line Is Nothing
            End Using
        Catch ex As ArgumentException ' Happens if the encoding name is invalid
            Program.WriteErrorMessage(ex.Message)
            ' The Main method will return the exit status to the operating system. The numbers are made up for the sample, they don't mean anything.
            ' Usually, 0 means success, and any other value indicates an error.
            ExitCode = 2
        Catch ex As IOException
            Program.WriteErrorMessage(ex.Message)
            ExitCode = 2
        Catch ex As UnauthorizedAccessException
            Program.WriteErrorMessage(ex.Message)
            ExitCode = 2
        End Try
    End Sub
End Class
