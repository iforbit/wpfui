# Add using directives for reorganized Ribbon namespaces

$ribbonPath = "C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon"

# List of using statements to add
$usingStatements = @(
    "using Wpf.Ui.Controls.Core;",
    "using Wpf.Ui.Controls.Tabs;",
    "using Wpf.Ui.Controls.Groups;",
    "using Wpf.Ui.Controls.Buttons;",
    "using Wpf.Ui.Controls.Gallery;",
    "using Wpf.Ui.Controls.Backstage;",
    "using Wpf.Ui.Controls.Menu;",
    "using Wpf.Ui.Controls.Primitives;",
    "using Wpf.Ui.Controls.Scrolling;",
    "using Wpf.Ui.Controls.Interfaces;"
)

# Get all C# files in subfolders that need using statements
$folders = @("Automation\Peers", "AttachedProperties", "Helpers", "Services", "Data", "TemplateSelectors")

foreach ($folder in $folders) {
    $fullPath = Join-Path $ribbonPath $folder
    if (Test-Path $fullPath) {
        $files = Get-ChildItem -Path $fullPath -Filter "*.cs" -Recurse
        
        foreach ($file in $files) {
            $content = Get-Content $file.FullName -Raw -Encoding UTF8
            
            # Check if using statements are already present
            $needsUpdate = $false
            foreach ($using in $usingStatements) {
                if ($content -notmatch [regex]::Escape($using)) {
                    $needsUpdate = $true
                    break
                }
            }
            
            if ($needsUpdate) {
                # Find last using statement position
                if ($content -match '(?m)^using .*?;[\r\n]+') {
                    $lastUsingMatch = [regex]::Matches($content, '(?m)^using .*?;[\r\n]+') | Select-Object -Last 1
                    $insertPosition = $lastUsingMatch.Index + $lastUsingMatch.Length
                    
                    # Insert all using statements
                    $usingsToAdd = ($usingStatements -join "`r`n") + "`r`n"
                    $content = $content.Insert($insertPosition, $usingsToAdd)
                    
                    Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
                    Write-Host "Updated: $($file.Name)"
                }
            }
        }
    }
}

Write-Host "`nUsing directives added!"
