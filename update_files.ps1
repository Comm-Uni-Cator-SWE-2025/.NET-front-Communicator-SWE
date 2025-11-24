$folders = @("src/Communicator.UX", "src/Communicator.Core", "src/Communicator.Icons")
$files = Get-ChildItem -Path $folders -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $fileName = $file.Name
    $modified = $false

    # Remove pragma warning disable
    if ($content -match "#pragma warning disable") {
        $content = [Regex]::Replace($content, "#pragma warning disable.*\r?\n?", "")
        $modified = $true
    }

    # Add header if not present
    if ($content -notmatch "File: .*$fileName") {
        $header = @"
/*
 * -----------------------------------------------------------------------------
 *  File: $fileName
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */

"@
        $content = $header + $content
        $modified = $true
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Updated $($file.FullName)"
    }
}
