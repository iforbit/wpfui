# Restore all namespaces back to Wpf.Ui.Controls

$ribbonPath = "C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon"

# Get all C# files in all subfolders
$files = Get-ChildItem -Path $ribbonPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    
    # Replace all Wpf.Ui.Controls.* namespaces back to Wpf.Ui.Controls
    $content = $content -replace 'namespace Wpf\.Ui\.Controls\.[^;{]+', 'namespace Wpf.Ui.Controls'
    
    # Remove extra using statements we added
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Core;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Tabs;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Groups;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Buttons;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Gallery;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Backstage;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Menu;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Primitives;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Scrolling;\r?\n', ''
    $content = $content -replace 'using Wpf\.Ui\.Controls\.Interfaces;\r?\n', ''
    
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
    Write-Host "Restored: $($file.Name)"
}

Write-Host "`nNamespace restoration completed!"
