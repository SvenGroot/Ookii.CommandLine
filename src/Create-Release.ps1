# This script is used to create a distribution folder that can be packaged into a zip file for release.
param(
    [parameter(Mandatory=$true, Position=0)][string]$TargetPath
)

[xml]$project = Get-Content "$PSScriptRoot/Ookii.CommandLine/Ookii.CommandLine.csproj"
$frameworks = $project.Project.PropertyGroup.TargetFrameworks -split ";"

# Publish each version of the library.
foreach ($framework in $frameworks) {
    if ($framework) {
        dotnet publish "$PSScriptRoot/Ookii.CommandLine" --configuration Release --framework $framework --output "$TargetPath/$framework"
    }
}

# Publish each sample
$samples = Get-ChildItem -Directory "$PSScriptRoot/Samples"
foreach ($sample in $samples) {
    $name = $sample.Name
    if ($name -ieq "Wpf") {
        $framework = "net6.0-windows"
    } else {
        $framework = "net6.0"
    }

    dotnet publish $sample --configuration Release --framework $framework --output "$TargetPath/Samples/$name"
}

# Copy global items.
Copy-Item "$PSScriptRoot/../license.md" $TargetPath
Copy-Item "$PSScriptRoot/Snippets/bin/Release/Ookii.CommandLine.Snippets.vsix" $TargetPath

# Create readme.txt files.
$url = "https://github.com/SvenGroot/Ookii.CommandLine"
"For documentation and other information, see:",$url | Set-Content "$TargetPath/readme.txt"
"For descriptions of each sample, see:",$url | Set-Content "$TargetPath/Samples/readme.txt"
