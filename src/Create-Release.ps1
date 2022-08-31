# This script is used to create a distribution folder that can be packaged into a zip file for release.
# Before running this script, make sure you have built a Release version of the solution, and have created
# an up-to-date Documentation.chm file.
param(
    [parameter(Mandatory=$true, Position=0)][string]$TargetPath
)

function Copy-ReleaseItems([string]$DirectoryName, [string[]]$Items)
{
    $target = Join-Path $TargetPath $DirectoryName
    New-Item $target -ItemType Directory | Out-Null
    $Items | 
        ForEach-Object { Join-Path $PSScriptRoot $_ } | 
        Copy-Item -Destination $target -Recurse
}

$commonItems = "..\README.md",
    "..\docs\license.md",
    "Snippets\bin\Release\Ookii.CommandLine.Snippets.vsix"

$docItems = "..\docs\*.md",
    "..\docs\Help\Documentation.chm",

$netFrameworkItems = "Ookii.CommandLine\bin\Release\net20\Ookii.CommandLine.dll",
    "Ookii.CommandLine\bin\Release\net20\Ookii.CommandLine.xml",
    "Ookii.CommandLine\bin\Release\net20\Ookii.CommandLine.pdb",
    "CommandLineSampleCS\bin\Release\net20\CommandLineSampleCS.exe",
    "ShellCommandSampleCS\bin\Release\net20\ShellCommandSampleCS.exe"

$netStandardItems = "Ookii.CommandLine\bin\Release\netstandard2.0\Ookii.CommandLine.dll",
    "Ookii.CommandLine\Ookii.CommandLine.xml",
    "Ookii.CommandLine\bin\Release\netstandard2.0\Ookii.CommandLine.pdb"

$net6Items = "Ookii.CommandLine\bin\Release\net6.0\Ookii.CommandLine.dll",
    "Ookii.CommandLine\Ookii.CommandLine.xml",
    "Ookii.CommandLine\bin\Release\net6.0\Ookii.CommandLine.pdb"


if( [System.IO.Directory]::GetFileSystemEntries($TargetPath).Length -gt 0 ) {
    throw "Target directory not empty." 
}

$commonItems | 
    ForEach-Object { Join-Path $PSScriptRoot $_ } | 
    Copy-Item -Destination $TargetPath -Recurse

Copy-ReleaseItems "docs" $docItems
Copy-ReleaseItems "net20" $netFrameworkItems
Copy-ReleaseItems "netstandard2.0" $netStandardItems
Copy-ReleaseItems "net6.0" $net6Items
