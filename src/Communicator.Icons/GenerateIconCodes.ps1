# PowerShell script to parse tabler-icons.css and generate IconCodes.cs

$cssFile = Join-Path $PSScriptRoot "Assets\Fonts\tabler-icons.css"
$outputFile = Join-Path $PSScriptRoot "IconCodes.cs"

Write-Host "Reading CSS file: $cssFile" -ForegroundColor Cyan

# Read the CSS file
$css = Get-Content $cssFile -Raw

# Regex to match icon definitions: .ti-icon-name:before { content: "\unicode"; }
$pattern = '\.ti-([a-z0-9-]+):before\s*\{\s*content:\s*"\\([0-9a-f]+)"\s*;\s*\}'
$matches = [regex]::Matches($css, $pattern)

Write-Host "Found $($matches.Count) icon definitions" -ForegroundColor Green

# Single dictionary for all icons (keep original names with -filled suffix)
$allIcons = @{}
$filledCount = 0
$outlineCount = 0

foreach ($match in $matches) {
    $name = $match.Groups[1].Value
    $unicode = $match.Groups[2].Value
    
    $allIcons[$name] = $unicode
    
    if ($name -match '-filled$') {
        $filledCount++
    } else {
        $outlineCount++
    }
}

Write-Host "Outline icons: $outlineCount" -ForegroundColor Yellow
Write-Host "Filled icons: $filledCount" -ForegroundColor Yellow

# Generate C# code
$csCode = @"
using System.Collections.Generic;

namespace UX.Icons;

/// <summary>
/// Maps Tabler icon names to Unicode characters
/// Auto-generated from tabler-icons.css
/// Total icons: $($allIcons.Count) ($outlineCount outline, $filledCount filled)
/// 
/// Usage:
/// - Outline icons: use base name (e.g., "video", "alert-triangle")
/// - Filled icons: use name with -filled suffix (e.g., "video-filled", "alert-triangle-filled")
/// </summary>
internal static class IconCodes
{
    // All icons in a single dictionary with original names
    private static readonly Dictionary<string, string> Icons = new()
    {
"@

# Add all icons (sorted alphabetically)
$allIcons.GetEnumerator() | Sort-Object Key | ForEach-Object {
    $hex = $_.Value
    $codePoint = [int]"0x$hex"
    
    # For characters above U+FFFF, use \U with 8 digits, otherwise use \u with 4 digits
    if ($codePoint -le 0xFFFF) {
        $unicode = "\u$hex"
    } else {
        $unicode = "\U" + $codePoint.ToString("X8").ToLower().PadLeft(8, '0')
    }
    
    $csCode += "        { `"$($_.Key)`", `"$unicode`" },`n"
}

$csCode += @"
    };

    /// <summary>
    /// Get Unicode character for an icon name
    /// </summary>
    /// <param name="iconName">Icon name (e.g., "video" for outline, "video-filled" for filled)</param>
    public static string? GetUnicode(string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
            return null;

        var normalizedName = iconName.ToLowerInvariant().Trim();
        return Icons.TryGetValue(normalizedName, out var unicode) ? unicode : null;
    }

    /// <summary>
    /// Check if an icon exists
    /// </summary>
    /// <param name="iconName">Icon name (e.g., "video" for outline, "video-filled" for filled)</param>
    public static bool Exists(string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
            return false;

        var normalizedName = iconName.ToLowerInvariant().Trim();
        return Icons.ContainsKey(normalizedName);
    }

    /// <summary>
    /// Get all available icon names
    /// </summary>
    public static IEnumerable<string> GetAllIconNames()
    {
        return Icons.Keys;
    }

    /// <summary>
    /// Get all outline icon names (icons without -filled suffix)
    /// </summary>
    public static IEnumerable<string> GetOutlineIconNames()
    {
        return Icons.Keys.Where(k => !k.EndsWith("-filled"));
    }

    /// <summary>
    /// Get all filled icon names (icons with -filled suffix)
    /// </summary>
    public static IEnumerable<string> GetFilledIconNames()
    {
        return Icons.Keys.Where(k => k.EndsWith("-filled"));
    }
}
"@

# Write to file
$csCode | Out-File -FilePath $outputFile -Encoding UTF8 -NoNewline

Write-Host "`nGenerated IconCodes.cs with:" -ForegroundColor Green
Write-Host "  - $outlineCount outline icons" -ForegroundColor Cyan
Write-Host "  - $filledCount filled icons" -ForegroundColor Cyan
Write-Host "  - Total: $($allIcons.Count) icons" -ForegroundColor Cyan
Write-Host "  - Output: $outputFile" -ForegroundColor Cyan
Write-Host "`nDone!" -ForegroundColor Green
