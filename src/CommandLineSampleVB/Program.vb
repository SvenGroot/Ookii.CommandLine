Imports System.Reflection
Imports Ookii.CommandLine

Public Module Program

    Public Sub Main()
        Dim arguments As ProgramArguments = ProgramArguments.Create()
        ' No need to do anything when the value is null; Create already printed errors and usage to the console
        If arguments Is Nothing Then
            Return
        End If

        ' We use the LineWrappingTextWriter to neatly wrap console output.
        Using writer As LineWrappingTextWriter = LineWrappingTextWriter.ForConsoleOut()
            ' Print the values of the arguments.
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
    End Sub

End Module
