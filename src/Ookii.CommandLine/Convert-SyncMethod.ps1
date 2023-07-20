param(
    [Parameter(Mandatory = $true)][string[]]$Path,
    [Parameter(Mandatory = $true)][string]$OutputDir
)

# This script takes async methods and makes the following replacements to generate a non-async
# version of that method.
$replacements = @(
    @("Async", ""), # Remove Async from identifiers
    @("async Task", "partial void"), # Function signature change
    @("await ", ""), # Remove await keyword
    @("ReadOnlyMemory", "ReadOnlySpan"), # Async stream functions uses Memory instead of span
    @(".Span", ""), # Needed to convert Memory usage to Span.
    @("async ", ""), # Remove keyword from async lambda
    @(", CancellationToken cancellationToken = default", ""), # Remove cancellation token parameter
    @(", CancellationToken cancellationToken", ""), # Remove cancellation token parameter
    @("(CancellationToken cancellationToken)", "()"), # Remove cancellation token parameter
    @(", cancellationToken", ""), # Remove cancellation token parameter value
    @("(cancellationToken)", "()") # Remove cancellation token parameter value
)

$files = Get-Item $Path
foreach ($file in $files)
{
    $outputPath = Join-Path $OutputDir ($file.Name.Replace("Async", "Sync"))
    Get-Content $file | ForEach-Object {
        # Regex replace generic Task<T> before the other replacements.
        $result = ($_ -creplace 'async Task<(.*?)>','partial $1')
        foreach ($item in $replacements) {
            $result = $result.Replace($item[0], $item[1])
        }

        $result
    } | Set-Content $outputPath
}
