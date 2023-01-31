param(
    [Parameter(Mandatory = $true)][string[]]$Path,
    [Parameter(Mandatory = $true)][string]$OutputDir
)

$files = Get-Item $Path
foreach ($file in $files)
{
    $outputPath = Join-Path $OutputDir ($file.Name.Replace("Async", "Sync"))
    Get-Content $file | ForEach-Object {
        ($_.Replace("partial async", "async") -creplace 'async Task<(.*?)>','partial $1').Replace("Async", "").Replace("async Task", "partial void").Replace("await ", "").Replace("ReadOnlyMemory", "ReadOnlySpan").Replace("StringMemory", "StringSpan").Replace(".Span", "").Replace("async ", "")
    } | Set-Content $outputPath
}
