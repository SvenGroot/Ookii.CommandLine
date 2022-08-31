Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports Ookii.CommandLine

''' <summary>
''' This is a sample ShellCommand that can be invoked by specifying "write" as the first argument to the sample application.
''' Shell command argument parsing works just like a regular command line argument class. After the arguments have been parsed,
''' the Run method is invoked to execute the ShellCommand.
''' Check the Program.vb file to see how this command is invoked.
''' </summary>
<ShellCommand("write"), Description("Writes lines to a file, wrapping them to the specified width.")>
Public Class WriteCommand
    Inherits ShellCommand

    ' A positional argument to specify the file name
    <CommandLineArgument(Position:=0, IsRequired:=True), Description("The name of the file to write to.")>
    Public Property FileName As String

    ' A positional multi-value argument to specify the text to write
    <CommandLineArgument(Position:=1), Description("The lines of text to write from the file; if no lines are specified, this application will read from standard input instead.")>
    Public Property Lines As String()

    ' A named argument to specify the encoding.
    ' Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
    ' instantiate the Encoding class ourselves in the run method.
    <CommandLineArgument("Encoding", DefaultValue:="utf-8"), Description("The encoding to use to write the file.")>
    Public Property EncodingName As String

    ' A named argument that specifies the maximum line length of the output
    <CommandLineArgument(DefaultValue:=79), [Alias]("Length"), Description("The maximum length of the lines in the file, or zero to have no limit.")>
    Public Property MaximumLineLength As Integer

    ' A named argument switch that indicates it's okay to overwrite files.
    <CommandLineArgument, Description("When this option is specified, the file will be overwritten if it already exists.")>
    Public Property Overwrite As Boolean

    Public Overrides Sub Run()
        ' This method is invoked after all command line arguments have been parsed
        Try
            ' Check if we're allowed to overwrite the file.
            If Not Overwrite AndAlso File.Exists(FileName) Then
                ' The Main method will return the exit status to the operating system. The numbers are made up for the sample, they don't mean anything.
                ' Usually, 0 means success, and any other value indicates an error.
                Program.WriteErrorMessage("File already exists.")
                ExitCode = 3
            Else
                ' We use a LineWrappingTextWriter to neatly wrap the output.
                Using writer As New StreamWriter(FileName, False, Encoding.GetEncoding(EncodingName)),
                      lineWriter As New LineWrappingTextWriter(writer, MaximumLineLength, True)
                    ' Write the specified content to the file
                    If Lines Is Nothing OrElse Lines.Length = 0 Then
                        ' Read from standard input. You can pipe a file to the input, or use it interactively (in that case, press CTRL-Z to send an EOF character and stop writing).
                        Dim line As String
                        Do
                            line = Console.ReadLine()
                            If line IsNot Nothing Then
                                lineWriter.WriteLine(line)
                            End If
                        Loop Until line Is Nothing
                    Else
                        ' Write the specified lines
                        For Each line As String In Lines
                            lineWriter.WriteLine(line)
                        Next
                    End If
                End Using
            End If
        Catch ex As ArgumentException ' Happens if the encoding name is invalid
            Program.WriteErrorMessage(ex.Message)
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
