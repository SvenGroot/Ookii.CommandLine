' Copyright (c) Sven Groot (Ookii.org) 2012
'
' This code is published under the Microsoft Public License (Ms-PL).  A copy
' of the license should be distributed with the code.  It can also be found
' at http://ookiicommandline.codeplex.com. This notice, the author's name,
' and all copyright notices must remain intact in all applications,
' documentation, and source files.
Imports System.Reflection
Imports Ookii.CommandLine

Public Module Program

    Public Sub Main(ByVal args() As String)
        Dim arguments As ProgramArguments = ProgramArguments.Create(args)
        ' No need to do anything when the value is null; Create already printed errors and usage to the console
        If arguments IsNot Nothing Then
            ' This application doesn't do anything useful, it's just a sample of using CommandLineParser after all. We use reflection to print
            ' the values of all the properties of the sample's CommandLineArguments class, which correspond to the sample's command line arguments.

            ' We use the LineWrappingTextWriter to neatly wrap console output.
            Using writer As LineWrappingTextWriter = LineWrappingTextWriter.ForConsoleOut()
                ' Print the full command line as received by the application
                writer.WriteLine("The command line was: {0}", Environment.CommandLine)
                writer.WriteLine()
                ' Print the values of the arguments, using reflection to get all the property values
                writer.WriteLine("The following argument values were provided:")
                writer.WriteLine("Source: {0}", If(arguments.Source, "(Nothing)"))
                writer.WriteLine("Destination: {0}", If(arguments.Destination, "(Nothing)"))
                writer.WriteLine("Index: {0}", arguments.Index)
                writer.WriteLine("Date: {0}", If(arguments.Date Is Nothing, "(Nothing)", arguments.Date.ToString()))
                writer.WriteLine("Count: {0}", arguments.Count)
                writer.WriteLine("Verbose: {0}", arguments.Verbose)
                writer.WriteLine("Values: {0}", If(arguments.Values Is Nothing, "(Nothing)", "{ " & String.Join(", ", arguments.Values) & " }"))
                writer.WriteLine("Help: {0}", arguments.Help)
            End Using
        End If
    End Sub

End Module
