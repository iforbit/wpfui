# Remove Wpf.Ui.Controls.Automation.Peers namespace references

$ribbonPath = "C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon"

$files = Get-ChildItem -Path $ribbonPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    # Remove using statements
    if ($content -match 'using Wpf\.Ui\.Controls\.Automation\.Peers;') {
        $content = $content -replace 'using Wpf\.Ui\.Controls\.Automation\.Peers;\r?\n', ''
        $modified = $true
    }
    
    if ($content -match 'using Wpf\.Ui\.Controls\.Automation;') {
        $content = $content -replace 'using Wpf\.Ui\.Controls\.Automation;\r?\n', ''
        $modified = $true
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Removed Automation namespace from: $($file.Name)"
    }
}

Write-Host "`nAutomation namespace references removed!"
