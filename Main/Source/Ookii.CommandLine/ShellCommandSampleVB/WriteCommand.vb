' Copyright (c) Sven Groot (Ookii.org) 2011
'
' This code is published under the Microsoft Public License (Ms-PL).  A copy
' of the license should be distributed with the code.  It can also be found
' at http://ookiicommandline.codeplex.com. This notice, the author's name,
' and all copyright notices must remain intact in all applications,
' documentation, and source files.
Imports System.ComponentModel
Imports Ookii.CommandLine
Imports System.IO
Imports System.Text

''' <summary>
''' This is a sample ShellCommand that can be invoked by specifying "write" as the first argument to the sample application.
''' Shell command argument parsing works just like a regular command line argument class. After the arguments have been parsed,
''' the Run method is invoked to execute the ShellCommand.
''' Check the Program.vb file to see how this command is invoked.
''' </summary>
<ShellCommand("write"), Description("Writes lines to a file, wrapping them to the specified width.")>
Public Class WriteCommand
    Inherits ShellCommand

    Private ReadOnly _fileName As String
    Private ReadOnly _lines() As String

    Public Sub New(<Description("The name of the file to write to.")> ByVal fileName As String,
                   <Description("The lines of text to write from the file; if no lines are specified, this application will read from standard input instead.")> Optional ByVal lines() As String = Nothing)
        ' The constructor parameters are the positionl command line arguments for the shell command. This command
        ' has a single required argument, and an optional argument that can have multiple values.
        If fileName Is Nothing Then _
            Throw New ArgumentNullException("fileName")

        _fileName = fileName
        _lines = lines
    End Sub

    ' A named argument to specify the encoding.
    ' Because Encoding doesn't have a TypeConverter, we simple accept the name of the encoding as a string and
    ' instantiate the Encoding class ourselves in the run method.
    <CommandLineArgument("encoding", DefaultValue:="utf-8"), Description("The encoding to use to write the file. The default value is utf-8.")>
    Public Property EncodingName As String

    ' A named argument that specifies the maximum line length of the output
    <CommandLineArgument("length", DefaultValue:=79), Description("The maximum length of the lines in the file, or zero to have no limit. The default value is 79.")>
    Public Property MaximumLineLength As Integer

    ' A named argument switch that indicates it's okay to overwrite files.
    <CommandLineArgument("overwrite", DefaultValue:=False), Description("When this option is specified, the file will be overwritten if it already exists.")>
    Public Property OverwriteFile As Boolean

    Public Overrides Sub Run()
        ' This method is invoked after all command line arguments have been parsed
        Try
            ' Check if we're allowed to overwrite the file.
            If Not OverwriteFile AndAlso File.Exists(_fileName) Then
                ' The Main method will return the exit status to the operating system. The numbers are made up for the sample, they don't mean anything.
                ' Usually, 0 means success, and any other value indicates an error.
                Program.WriteErrorMessage("File already exists.")
                ExitCode = 3
            Else
                ' We use a LineWrappingTextWriter to neatly wrap the output.
                Using writer As StreamWriter = New StreamWriter(_fileName, False, Encoding.GetEncoding(EncodingName)),
                      lineWriter As LineWrappingTextWriter = New LineWrappingTextWriter(writer, MaximumLineLength, True)
                    ' Write the specified content to the file
                    If _lines Is Nothing OrElse _lines.Length = 0 Then
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
                        For Each line As String In _lines
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
