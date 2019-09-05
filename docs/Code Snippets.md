# Code Snippets

Several code snippets are provided to make working with Ookii.CommandLine even easier:

- **clargclass:** Snippet for an argument class, including a static `Create` method that parses arguments.
- **clarg:** Snippet for a command line argument.
- **clargpos:** Snippet for a positional command line argument.
- **clargmulti:** Snippet for a multi-value command line argument.
- **clargdict:** Snippet for a dictionary command line argument.

All snippets are provided for C# and Visual Basic. To use them, either run the `Install-Snippets.ps1` script with PowerShell, or manually copy the snippet files to the "Visual Studio \<version>\Code Snippets\Visual C#\My Code Snippets" or "Visual Studio \<version>\Code Snippets\Visual Basic\My Code Snippets" folder located in your My Documents folder. Alternatively, use the snippet manager inside Visual Studio to import the snippet files.

The Install-Snippets.ps1 script will install the snippets for Visual Studio 2005 through 2019 if the snippet folders for those versions exist.

To uninstall, simply remove the files from those folders.

Snippets are not provided with the NuGet package; you must download the release from GitHub to get them. Of course, you can use the download from GitHub to get the snippets and then still use NuGet to add the library to your project.
