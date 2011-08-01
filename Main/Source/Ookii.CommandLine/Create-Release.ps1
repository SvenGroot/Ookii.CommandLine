param(
    [parameter(Mandatory=$true, Position=0)]$TargetPath
)

$distItems = "Ookii.CommandLine\bin\Release\Ookii.CommandLine.dll","Ookii.CommandLine\bin\Release\Ookii.CommandLine.xml",
    "CommandLineSampleCS\bin\Release\CommandLineSampleCS.exe","ShellCommandSampleCS\bin\Release\ShellCommandSampleCS.exe",
    "..\..\Docs\User Guide.html","..\..\Docs\Help\Documentation.chm","..\..\Docs\license.txt"

$scriptPath = Split-Path $MyInvocation.MyCommand.Path
New-Item $TargetPath -Type "directory" -Force | Out-Null
if( [System.IO.Directory]::GetFileSystemEntries($TargetPath).Length -gt 0 )
    { throw "Target directory not empty." }
    
Write-Host "Copying source files..."
$targetSourcePath = Join-Path $TargetPath Source
New-Item $targetSourcePath -Type "directory" -Force | Out-Null
Copy-Item $(Join-Path $scriptPath *) $targetSourcePath -Recurse -Force
Get-ChildItem -LiteralPath $targetSourcePath -Recurse -Force | foreach { $_.Attributes = $_.Attributes -band (-bnot ([System.IO.FileAttributes]::ReadOnly -bor [System.IO.FileAttributes]::Hidden)) }

Write-Host "Removing unneeded files."
$itemsToDelete = Get-ChildItem -LiteralPath $targetSourcePath -Recurse -Force | 
    where { ($_.PSIsContainer -and "bin","obj","TestResults" -icontains $_.Name) -or (-not $_.PSIsContainer -and ".vssscc",".vspscc",".suo",".user" -icontains [System.IO.Path]::GetExtension($_.Name)) }
    
$itemsToDelete | foreach { Remove-Item $_.FullName -Force -Recurse }

Write-Host "Removing source control bindings"
Get-ChildItem -LiteralPath $targetSourcePath -Recurse |
    where { [System.IO.Path]::GetExtension($_.Name) -ieq ".sln" } |
    foreach {
        $solution = Get-Content $_.FullName
        $inTfsBlock = $false
        $solution | 
            foreach { 
                if( $_.Trim() -eq "GlobalSection(TeamFoundationVersionControl) = preSolution" )
                    { $inTfsBlock = $true }
                
                if( $inTfsBlock )
                {
                    if( $_.Trim() -eq "EndGlobalSection" )
                        { $inTfsBlock = $false }
                }
                else
                    { $_ }
            } | 
            sc $_.FullName -Encoding UTF8
    }

Get-ChildItem -LiteralPath $targetSourcePath -Recurse | 
    where { ".csproj",".vbproj" -icontains [System.IO.Path]::GetExtension($_.Name) } |
    foreach {
        $xml = [xml](Get-Content $_.FullName)
        $xml.SelectNodes("//*[starts-with(local-name(), 'Scc')]") | foreach { $_.RemoveAll() }
        $xml.Project.PropertyGroup | where { $_.PreBuildEvent -and $_.PreBuildEvent.StartsWith("PowerShell.exe") } | foreach { $_.PreBuildEvent = "REM " + $_.PreBuildEvent }
        $xml.Save($_.FullName)
    }

Write-Host "Copying distribution items"    
$distItems | foreach { Copy-Item $(Join-Path $scriptPath $_) $targetPath }
Get-ChildItem -LiteralPath $targetPath | foreach { $_.Attributes = $_.Attributes -band (-bnot ([System.IO.FileAttributes]::ReadOnly -bor [System.IO.FileAttributes]::Hidden)) } 
