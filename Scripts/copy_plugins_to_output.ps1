Push-Location ..

$outputDir = Join-Path -Path (Get-Location) -ChildPath "kuiper/bin/Debug/net10.0/win-x64/Plugins"
$pluginDirs = Get-ChildItem -Path "Plugins"
foreach ($pluginDir in $pluginDirs) {
    $pluginName = $pluginDir.Name
    $pluginDllPath = Join-Path -Path $pluginDir.FullName -ChildPath "/bin/Debug/net10.0/$pluginName.dll"
    if (Test-Path -Path $pluginDllPath) {
        $destinationPath = Join-Path -Path $outputDir -ChildPath "$pluginName.dll"
        Copy-Item -Path $pluginDllPath -Destination $destinationPath -Force
        Write-Host "Copied $pluginDllPath to $destinationPath"
    } else {
        Write-Warning "DLL not found for plugin: $pluginName at path: $pluginDllPath"
    }
}

Pop-Location