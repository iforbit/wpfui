# Fix Automation Peer references - change fully qualified names

$ribbonPath = "C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon"

$files = Get-ChildItem -Path $ribbonPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    # Replace fully qualified AutomationPeer references with simple class names
    if ($content -match 'Wpf\.Ui\.Controls\.Automation\.Peers\.') {
        $content = $content -replace 'new Wpf\.Ui\.Controls\.Automation\.Peers\.(\w+AutomationPeer)', 'new $1'
        $content = $content -replace 'Wpf\.Ui\.Controls\.Automation\.Peers\.GetOrCreateAutomationPeer', 'GetOrCreateAutomationPeer'
        $modified = $true
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Fixed: $($file.Name)"
    }
}

Write-Host "`nAutomation Peer references fixed!"
