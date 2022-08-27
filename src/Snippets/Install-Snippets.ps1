$versions="2005","2008","2010","2012","2013","2015","2017","2019","2022"
$sourcePath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$documentsPath = [Environment]::GetFolderPath("MyDocuments")
$snippetFolder = "Ookii.CommandLine"

foreach( $version in $versions )
{
    $targetDirectory = "$documentsPath\Visual Studio $version\Code Snippets"
    $csharpTargetDirectory = "$targetDirectory\Visual C#\My Code Snippets"
    $vbTargetDirectory = "$targetDirectory\Visual Basic\My Code Snippets"

    if( Test-Path $csharpTargetDirectory )
    {
        "Installing C# snippets for Visual Studio $version..."
        $csharpTargetDirectory = "$csharpTargetDirectory\$snippetFolder"
        if( !(Test-Path $csharpTargetDirectory) )
            { New-Item $csharpTargetDirectory -ItemType Directory | Out-Null }

        Copy-Item "$sourcePath\Visual C#\*.snippet" $csharpTargetDirectory
    }

    if( Test-Path $vbTargetDirectory )
    {
        "Installing VB snippets for Visual Studio $version..."
        $vbTargetDirectory = "$vbTargetDirectory\$snippetFolder"
        if( !(Test-Path $vbTargetDirectory) )
            { New-Item $vbTargetDirectory -ItemType Directory | Out-Null }

        Copy-Item "$sourcePath\Visual Basic\*.snippet" $vbTargetDirectory
    }
}