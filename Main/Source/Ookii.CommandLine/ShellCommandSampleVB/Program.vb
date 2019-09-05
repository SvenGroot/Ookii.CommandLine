' Copyright (c) Sven Groot (Ookii.org)
'
' This code is published under the Microsoft Public License (Ms-PL).  A copy
' of the license should be distributed with the code.  It can also be found
' at https://github.com/SvenGroot/ookii.commandline. This notice, the author's name,
' and all copyright notices must remain intact in all applications,
' documentation, and source files.
Imports Ookii.CommandLine
Imports System.Reflection

Public Module Program

    Public Function Main(ByVal args() As String) As Integer
        ' Create a shell command based on the arguments. The CreateShellCommand method will catch any command line errors
        ' and print error details and usage information on the console, so we don't have to worry about that here.
        Dim options As New CreateShellCommandOptions
        options.UsageOptions.IncludeDefaultValueInDescription = True
        options.UsageOptions.IncludeAliasInDescription = True
        Dim command As ShellCommand = ShellCommand.CreateShellCommand(Assembly.GetExecutingAssembly(), args, 0, options)

        If command IsNot Nothing Then
            ' The command line arguments were successfully parsed, so run the command.
            command.Run()
            ' When using shell commands, it's good practice to return the value of the ExitStatus property to the operating system.
            ' The application or script that invoked your application can check the exit code from your application to
            ' see if you were successful or not. Error codes are completely application specific, but usually 0 is used to
            ' indicate success, and any other value indicates an error.
            ' Shell commands can set the ExitStatus property to indicate the exit code they want the application to return.
            Return command.ExitCode
        End If

        Return 1 ' Return an error status if the command couldn't be created.
    End Function

    ''' <summary>
    ''' Utility method used by the commands to write exception data to the console.
    ''' </summary>
    ''' <param name="message"></param>
    Public Sub WriteErrorMessage(ByVal message As String)
        Using writer As LineWrappingTextWriter = LineWrappingTextWriter.ForConsoleError()
            writer.WriteLine(message)
        End Using
    End Sub

End Module
