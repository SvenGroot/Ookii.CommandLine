param($installPath, $toolsPath, $package, $project)

if( $project.Type -ne "C#" )
    { $project.ProjectItems.Item("SampleArguments.cs").Delete() }

if( $project.Type -ne "VB.NET" )
    { $project.ProjectItems.Item("SampleArguments.vb").Delete() }
