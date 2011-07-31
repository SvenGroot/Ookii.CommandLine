' $Id: ReadCommand.vb 28 2011-06-26 06:42:21Z sgroot $
'
Imports Ookii.CommandLine
Imports System.ComponentModel
Imports System.IO
Imports System.Text

''' <summary>
''' This is a sample ShellCommand that can be invoked by specifying "read" as the first argument to the sample application.
''' Shell command argument parsing works just like a regular command line argument class. After the arguments have been parsed,
''' the Run method is invoked to execute the ShellCommand.
''' Check the Program.vb file to see how this command is invoked.
''' </summary>
<ShellCommand("read"), Description("Reads and displays data from a file using the specified encoding, wrapping the text to fit the console.")>
Public Class ReadCommand
    Inherits ShellCommand

    Private ReadOnly _fileName As String

    Public Sub New(<Description("The name of the file to read.")> ByVal fileName As String)
        ' The constructor parameters are the positionl command line arguments for the shell command. This command
        ' has only a single argument.
        If fileName Is Nothing Then _
            Throw New ArgumentNullException("fileName")

        _fileName = fileName
    End Sub

    ' A named argument to specify the encoding.
    ' Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
    ' instantiate the Encoding class ourselves in the run method.
    <CommandLineArgument("encoding", DefaultValue:="utf-8"), Description("The encoding to use to read the file. The default value is utf-8.")>
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
