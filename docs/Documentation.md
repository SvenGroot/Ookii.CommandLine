# Introduction

Ookii.CommandLine is a library that helps you to parse command line arguments for your applications. It allows you to easily define a set of accepted arguments, and then parse the command line supplied to your application for those arguments. In addition, it allows you to generate usage help from the arguments that you defined which you can display to the user.

Ookii.CommandLine can be used with any kind of .Net application, whether Console, Windows Forms, or WPF. Although a limited subset of functionality—particularly related around generating usage help text—is geared primarily towards console applications that are invoked from the command prompt, the main command line parsing functionality is usable in any application that needs to process command line arguments.

To define a set of command line arguments, you create a class that will hold their values. The constructor parameters and properties of that class determine the set of arguments that are accepted. For each argument you can customize things like the argument name and whether or not an argument is required, and you can specify descriptions used to customize the usage help.

Two samples are provided with the library, one for basic command line parsing and one for shell commands. Both have a C# and Visual Basic version so you can see how to use the Ookii.CommandLine library in your own code. Besides the source code, they are also included as binaries so you can experience how the argument parsing works by trying them out.

## System requirements

Ookii.CommandLine is a class library for use in your own applications for [Microsoft .Net](https://dotnet.microsoft.com/). It can be used with applications targeting at least one of the following environments:

* Microsoft .Net Framework 2.0 or later
* Microsoft .Net Standard 2.0
* [Mono](http://www.mono-project.com/) 2.6 or later, using the .Net 2.0 or .Net 4.0 profile

To view and edit the source code of Ookii.CommandLine or the included sample applications, it is recommended to use the latest version of [Microsoft Visual Studio](https://visualstudio.microsoft.com/). Library documentation is generated using [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB).

## Contents

* [What's New in Ookii.CommandLine](What's%20New%20in%20Ookii.CommandLine.md)
* [Command Line Arguments in Ookii.CommandLine](Command%20Line%20Arguments%20in%20Ookii.CommandLine.md)
  * [Defining Command Line Arguments](Defining%20Command%20Line%20Arguments.md)
  * [Parsing Command Line Arguments](Parsing%20Command%20Line%20Arguments.md)
  * [Generating Usage Help](Generating%20Usage%20Help.md)
* [Shell Commands](Shell%20Commands.md)
* [Line Wrapping Text Writer](Line%20Wrapping%20Text%20Writer.md)
* [Code Snippets](Code%20Snippets.md)
* [Class Library Documentation](http://www.ookii.org/link.ashx?id=CommandLineDoc.md)
