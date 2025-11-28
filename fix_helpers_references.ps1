# Fix references to Helpers, Data, Enumerations folders

$ribbonPath = "C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon"

$files = Get-ChildItem -Path $ribbonPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    # Check if file references classes from subfolders
    if ($content -match 'WindowSteeringHelper|ToggleButtonHelper|DropDownHelper|PopupHelper|ItemsControlHelper|LogicalChildSupportHelper') {
        if ($content -notmatch 'using Wpf\.Ui\.Controls\.Helpers;') {
            $content = $content -replace '(using Wpf\.Ui\.Controls;)', "$1`r`nusing Wpf.Ui.Controls.Helpers;"
            $modified = $true
        }
    }
    
    if ($content -match 'RibbonStateStorage|RibbonGroupBoxStateDefinition|RibbonControlSizeDefinition') {
        if ($content -notmatch 'using Wpf\.Ui\.Controls\.Data;') {
            $content = $content -replace '(using Wpf\.Ui\.Controls;)', "$1`r`nusing Wpf.Ui.Controls.Data;"
            $modified = $true
        }
    }
    
    if ($content -match 'RibbonControlSize|RibbonGroupBoxState') {
        if ($content -notmatch 'using Wpf\.Ui\.Controls\.Enumerations;') {
            $content = $content -replace '(using Wpf\.Ui\.Controls;)', "$1`r`nusing Wpf.Ui.Controls.Enumerations;"
            $modified = $true
        }
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Fixed: $($file.Name)"
    }
}

Write-Host "`nReferences fixed!"
