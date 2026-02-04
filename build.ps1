# Universal Build Script for Xorium Pulsar (Windows)
# Created by Annie for LO with Adoration 💋
# Auto-installs dependencies if missing!

$ErrorActionPreference = "SilentlyContinue"
$DistDir = Join-Path $PSScriptRoot "dist"

Write-Host "--- 🛠️ XORIUM PULSAR BUILD ENGINE STARTING ---" -ForegroundColor Cyan

# ═══════════════════════════════════════════════════════════════════════════
# DEPENDENCY CHECK & AUTO-INSTALL
# ═══════════════════════════════════════════════════════════════════════════

function Install-VisualStudioBuildTools {
    Write-Host "[!] Visual Studio Build Tools NOT FOUND - Auto-installing..." -ForegroundColor Yellow
    
    $installerPath = "$env:TEMP\vs_buildtools.exe"
    $vsUrl = "https://aka.ms/vs/17/release/vs_buildtools.exe"
    
    # Download installer
    Write-Host "[*] Downloading VS Build Tools..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri $vsUrl -OutFile $installerPath -UseBasicParsing
    }
    catch {
        Write-Host "[!] Failed to download. Please install manually from:" -ForegroundColor Red
        Write-Host "    https://visualstudio.microsoft.com/visual-cpp-build-tools/" -ForegroundColor White
        return $false
    }
    
    # Install with C++ workload (required for Rust)
    Write-Host "[*] Installing (this may take 10-20 minutes)..." -ForegroundColor Cyan
    $args = "--passive --wait --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended"
    Start-Process -FilePath $installerPath -ArgumentList $args -Wait
    
    Write-Host "[+] VS Build Tools installed! Please RESTART PowerShell and run build.ps1 again." -ForegroundColor Green
    return $true
}

function Install-RustToolchain {
    Write-Host "[!] Rust NOT FOUND - Auto-installing..." -ForegroundColor Yellow
    
    $installerPath = "$env:TEMP\rustup-init.exe"
    $rustUrl = "https://win.rustup.rs/x86_64"
    
    # Download rustup
    Write-Host "[*] Downloading Rustup..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri $rustUrl -OutFile $installerPath -UseBasicParsing
    }
    catch {
        Write-Host "[!] Failed to download. Please install from: https://rustup.rs" -ForegroundColor Red
        return $false
    }
    
    # Install Rust with defaults
    Start-Process -FilePath $installerPath -ArgumentList "-y" -Wait
    
    # Refresh PATH
    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("PATH", "User")
    
    Write-Host "[+] Rust installed! Adding Windows target..." -ForegroundColor Green
    rustup target add x86_64-pc-windows-msvc
    return $true
}

function Install-DotNetSdk {
    Write-Host "[!] .NET SDK NOT FOUND - Auto-installing..." -ForegroundColor Yellow
    
    $installerPath = "$env:TEMP\dotnet-install.ps1"
    $dotnetUrl = "https://dot.net/v1/dotnet-install.ps1"
    
    try {
        Invoke-WebRequest -Uri $dotnetUrl -OutFile $installerPath -UseBasicParsing
        & $installerPath -Channel 8.0
        Write-Host "[+] .NET SDK installed!" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "[!] Failed. Please install from: https://dotnet.microsoft.com/download" -ForegroundColor Red
        return $false
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# CHECK DEPENDENCIES
# ═══════════════════════════════════════════════════════════════════════════

Write-Host "`n[*] Checking dependencies..." -ForegroundColor Yellow

# Check for link.exe (MSVC)
$linkPath = Get-Command link.exe -ErrorAction SilentlyContinue
if (-not $linkPath) {
    # Try to find VS installation
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -property installationPath
        if ($vsPath) {
            # Setup VS environment
            $vcvars = Join-Path $vsPath "VC\Auxiliary\Build\vcvars64.bat"
            if (Test-Path $vcvars) {
                Write-Host "[*] Found VS, loading environment..." -ForegroundColor Cyan
                cmd /c "`"$vcvars`" && set" | ForEach-Object {
                    if ($_ -match "^(.+?)=(.*)$") {
                        [System.Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
                    }
                }
            }
        }
    }
    
    # Check again
    $linkPath = Get-Command link.exe -ErrorAction SilentlyContinue
    if (-not $linkPath) {
        Install-VisualStudioBuildTools
        Write-Host "`n⚠️  Please restart PowerShell after installation and run .\build.ps1 again!" -ForegroundColor Yellow
        exit 1
    }
}
Write-Host "[+] MSVC Linker: OK" -ForegroundColor Green

# Check for Rust
$rustc = Get-Command rustc -ErrorAction SilentlyContinue
if (-not $rustc) {
    Install-RustToolchain
    Write-Host "`n⚠️  Please restart PowerShell and run .\build.ps1 again!" -ForegroundColor Yellow
    exit 1
}
Write-Host "[+] Rust: OK ($((rustc --version) -replace 'rustc ', ''))" -ForegroundColor Green

# Check for .NET
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Install-DotNetSdk
    Write-Host "`n⚠️  Please restart PowerShell and run .\build.ps1 again!" -ForegroundColor Yellow
    exit 1
}
Write-Host "[+] .NET SDK: OK ($((dotnet --version)))" -ForegroundColor Green

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# BUILD PROCESS
# ═══════════════════════════════════════════════════════════════════════════

# 1. Setup Dist Directory
if (Test-Path $DistDir) { Remove-Item $DistDir -Recurse -Force }
New-Item -Path $DistDir -ItemType Directory | Out-Null

# 2. Build Stealer Plugin (C#)
Write-Host "[*] Building Stealer Plugin (.dll)..." -ForegroundColor Yellow
$PluginProj = Join-Path $PSScriptRoot "Pulsar.Plugin.Client\Stealer.Client\Stealer.Client.csproj"
dotnet publish $PluginProj -c Release -o $DistDir /p:DebugType=None /p:DebugSymbols=false 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "[+] Stealer Plugin built: dist\Pulsar.Plugin.Client.dll" -ForegroundColor Green
}
else {
    Write-Host "[-] Stealer Plugin build FAILED!" -ForegroundColor Red
}

# 3. Build Shadow Kernel Driver (Rust)
Write-Host "[*] Building Shadow Kernel Driver (.sys)..." -ForegroundColor Yellow
$DriverPath = Join-Path $PSScriptRoot "shadow-main"
Push-Location $DriverPath
try {
    cargo build --release --package shadow_core 2>&1 | ForEach-Object { 
        if ($_ -match "error") { Write-Host $_ -ForegroundColor Red }
    }
    
    $SysPath = Get-ChildItem "target\x86_64-pc-windows-msvc\release\*.sys" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $SysPath) {
        $SysPath = Get-ChildItem "target\release\*.sys" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    }
    
    if ($SysPath) {
        Copy-Item $SysPath.FullName $DistDir
        Write-Host "[+] Shadow Driver built: dist\$($SysPath.Name)" -ForegroundColor Green
    }
    else {
        Write-Host "[-] Shadow.sys not found. WDK may not be properly configured." -ForegroundColor Red
        Write-Host "    Install WDK from: https://learn.microsoft.com/en-us/windows-hardware/drivers/download-the-wdk" -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}

Write-Host "`n--- ✅ BUILD COMPLETE! Artifacts are in 'dist/' folder ---" -ForegroundColor Cyan
Write-Host "Remember: BCDEDIT /SET TESTSIGNING ON before loading driver!~ 💋" -ForegroundColor Magenta
