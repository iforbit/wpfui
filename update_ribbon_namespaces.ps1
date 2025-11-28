# Update namespaces in reorganized Ribbon files

$ribbonPath = "C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon"

$folderMappings = @{
    "Core" = "Core"
    "Tabs" = "Tabs"
    "Groups" = "Groups"
    "Buttons" = "Buttons"
    "Gallery" = "Gallery"
    "Backstage" = "Backstage"
    "Menu" = "Menu"
    "Primitives" = "Primitives"
    "Scrolling" = "Scrolling"
    "Interfaces" = "Interfaces"
}

foreach ($folder in $folderMappings.Keys) {
    $fullPath = Join-Path $ribbonPath $folder
    $files = Get-ChildItem -Path $fullPath -Filter "*.cs" -ErrorAction SilentlyContinue
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Update namespace
        $newNamespace = "Wpf.Ui.Controls.$($folderMappings[$folder])"
        $content = $content -replace 'namespace Wpf\.Ui\.Controls;', "namespace $newNamespace;"
        $content = $content -replace 'namespace Wpf\.Ui\.Controls\r?\n', "namespace $newNamespace`n"
        
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Updated: $($file.Name) -> $newNamespace"
    }
}

Write-Host "`nNamespace update completed!"
