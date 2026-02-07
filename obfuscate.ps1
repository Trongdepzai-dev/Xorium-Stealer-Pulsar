# ═══════════════════════════════════════════════════════════════════════════
# 🌑 XORIUM PULSAR OBFUSCATION ENGINE v1.0 - ABYSS EDITION 🌑
# ═══════════════════════════════════════════════════════════════════════════
# Advanced Binary Hardening: Symbol Stripping, Entropy Management, Header Manipulation
# ═══════════════════════════════════════════════════════════════════════════

param(
    [Parameter(Mandatory = $true)]
    [string]$TargetPath,
    
    [switch]$SkipEntropyInjection,
    [switch]$SkipHeaderManipulation,
    [switch]$SkipDotNetObfuscation
)

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════
# ANSI COLORS (Abyss Edition Palette)
# ═══════════════════════════════════════════════════════════════════════════

$AbyssColors = @{
    DeepPurple  = @(75, 0, 130)
    CrimsonRed  = @(220, 20, 60)
    GhostWhite  = @(248, 248, 255)
    NeonCyan    = @(0, 255, 255)
    BloodOrange = @(255, 69, 0)
    ShadowGray  = @(105, 105, 105)
    AbyssGreen  = @(50, 205, 50)
}

function Write-RGB {
    param([string]$Text, [int[]]$RGB, [switch]$NoNewline)
    $r, $g, $b = $RGB
    $ansi = "`e[38;2;${r};${g};${b}m"
    $reset = "`e[0m"
    if ($NoNewline) { Write-Host "${ansi}${Text}${reset}" -NoNewline }
    else { Write-Host "${ansi}${Text}${reset}" }
}

function Write-Status { param([string]$msg); Write-RGB "  ◆ " -RGB $AbyssColors.DeepPurple -NoNewline; Write-RGB $msg -RGB $AbyssColors.GhostWhite }
function Write-Success { param([string]$msg); Write-RGB "  ✓ " -RGB $AbyssColors.AbyssGreen -NoNewline; Write-RGB $msg -RGB $AbyssColors.NeonCyan }
function Write-ErrorMsg { param([string]$msg); Write-RGB "  ✗ " -RGB $AbyssColors.CrimsonRed -NoNewline; Write-RGB $msg -RGB $AbyssColors.BloodOrange }
function Write-WarningMsg { param([string]$msg); Write-RGB "  ⚡ " -RGB $AbyssColors.BloodOrange -NoNewline; Write-RGB $msg -RGB $AbyssColors.ShadowGray }

# ═══════════════════════════════════════════════════════════════════════════
# ENTROPY CALCULATION
# ═══════════════════════════════════════════════════════════════════════════

function Get-FileEntropy {
    param([string]$FilePath)
    
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    $freq = @{}
    foreach ($b in $bytes) {
        if ($freq.ContainsKey($b)) { $freq[$b]++ }
        else { $freq[$b] = 1 }
    }
    
    $entropy = 0.0
    $total = $bytes.Length
    foreach ($count in $freq.Values) {
        $p = $count / $total
        if ($p -gt 0) {
            $entropy -= $p * [Math]::Log($p, 2)
        }
    }
    return [Math]::Round($entropy, 4)
}

# ═══════════════════════════════════════════════════════════════════════════
# SYMBOL STRIPPING (LLVM-STRIP)
# ═══════════════════════════════════════════════════════════════════════════

function Invoke-SymbolStrip {
    param([string]$FilePath)
    
    Write-Status "Stripping symbols from $(Split-Path $FilePath -Leaf)..."
    
    # Expanded search for llvm-strip
    $searchPaths = @(
        $env:USERPROFILE + "\.rustup\toolchains",
        $PSScriptRoot + "\tools",
        "C:\Program Files\LLVM\bin",
        "C:\Program Files (x86)\LLVM\bin",
        "C:\msys64\mingw64\bin",
        "C:\msys64\usr\bin"
    )
    
    $llvmStrip = $null
    foreach ($path in $searchPaths) {
        if (Test-Path $path) {
            $found = Get-ChildItem -Path $path -Recurse -Filter "llvm-strip.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) { $llvmStrip = $found; break }
        }
    }
    
    if ($llvmStrip) {
        try {
            & $llvmStrip.FullName --strip-all $FilePath 2>&1 | Out-Null
            Write-Success "Symbols stripped successfully."
            return $true
        }
        catch {
            Write-WarningMsg "llvm-strip failed: $_"
            return $false
        }
    }
    else {
        Write-WarningMsg "llvm-strip not found. Binary metadata might be intact."
        return $false
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# ENTROPY INJECTION (Random Byte Padding)
# ═══════════════════════════════════════════════════════════════════════════

function Invoke-EntropyInjection {
    param(
        [string]$FilePath, 
        [double]$TargetMin = 6.2,
        [double]$TargetMax = 6.8
    )
    
    $fileName = Split-Path $FilePath -Leaf
    Write-Status "Analyzing entropy for $fileName..."
    
    $currentEntropy = Get-FileEntropy -FilePath $FilePath
    Write-RGB "    → Current entropy: $currentEntropy bits/byte" -RGB $AbyssColors.ShadowGray
    
    if ($currentEntropy -ge $TargetMin -and $currentEntropy -le $TargetMax) {
        Write-Success "Entropy is in optimal stealth range ($TargetMin - $TargetMax). Skipping injection."
        return
    }
    
    if ($currentEntropy -gt $TargetMax) {
        # Natural system DLLs are usually around 5.0 - 6.5. Too high is suspicious.
        Write-WarningMsg "Entropy is already high ($currentEntropy). Skipping additional injection."
        return
    }

    Write-Status "Injecting entropy baseline..."
    
    # Append random bytes to the end of the file (overlay section)
    $random = New-Object System.Random
    $ratio = $random.NextDouble() * 0.1 + 0.05 # 5% to 15%
    $paddingSize = [Math]::Max(4096, [int]((Get-Item $FilePath).Length * $ratio)) 
    $padding = New-Object byte[] $paddingSize
    $random.NextBytes($padding)
    
    # Use polymorphic markers to avoid static string detection
    $markers = @("RDATA", "TEXT", "UPX0", "XORIUM", "ABYSS")
    $markerStr = $markers[$random.Next(0, $markers.Length)] + "_" + $random.Next(1000, 9999).ToString()
    $marker = [System.Text.Encoding]::ASCII.GetBytes($markerStr)
    
    try {
        $stream = [System.IO.File]::OpenWrite($FilePath)
        $stream.Seek(0, [System.IO.SeekOrigin]::End) | Out-Null
        $stream.Write($marker, 0, $marker.Length)
        $stream.Write($padding, 0, $padding.Length)
        $stream.Close()
        
        $newEntropy = Get-FileEntropy -FilePath $FilePath
        Write-RGB "    → New entropy: $newEntropy bits/byte" -RGB $AbyssColors.ShadowGray
        Write-Success "Entropy injection complete (+$paddingSize bytes with marker $markerStr)."
    }
    catch {
        Write-ErrorMsg "Failed to inject entropy: $_"
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# PE HEADER MANIPULATION
# ═══════════════════════════════════════════════════════════════════════════

function Invoke-HeaderManipulation {
    param([string]$FilePath)
    
    Write-Status "Manipulating PE headers of $(Split-Path $FilePath -Leaf)..."
    
    try {
        $bytes = [System.IO.File]::ReadAllBytes($FilePath)
        
        # Check for valid PE signature
        if ($bytes[0] -ne 0x4D -or $bytes[1] -ne 0x5A) {
            Write-WarningMsg "Not a valid PE file. Skipping header manipulation."
            return
        }
        
        # Get PE header offset (at offset 0x3C)
        $peOffset = [BitConverter]::ToInt32($bytes, 0x3C)
        
        # Verify PE signature
        if ($bytes[$peOffset] -ne 0x50 -or $bytes[$peOffset + 1] -ne 0x45) {
            Write-WarningMsg "Invalid PE signature. Skipping."
            return
        }
        
        # Randomize TimeDateStamp (offset PE+8, 4 bytes)
        $timestampOffset = $peOffset + 8
        $random = New-Object System.Random
        $fakeTimestamp = $random.Next(1262304000, 1735689600) # Random date between 2010-2025
        $timestampBytes = [BitConverter]::GetBytes($fakeTimestamp)
        [Array]::Copy($timestampBytes, 0, $bytes, $timestampOffset, 4)
        Write-RGB "    → TimeDateStamp randomized" -RGB $AbyssColors.ShadowGray
        
        # Randomize Major/Minor Linker Version (PE+26, PE+27)
        $bytes[$peOffset + 26] = [byte]$random.Next(10, 16)  # Major
        $bytes[$peOffset + 27] = [byte]$random.Next(0, 50)   # Minor
        Write-RGB "    → Linker version spoofed" -RGB $AbyssColors.ShadowGray
        
        [System.IO.File]::WriteAllBytes($FilePath, $bytes)
        Write-Success "PE header manipulation complete."
    }
    catch {
        Write-ErrorMsg "Header manipulation failed: $_"
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# .NET CODE OBFUSCATION (OBFUSCAR)
# ═══════════════════════════════════════════════════════════════════════════

function Invoke-DotNetObfuscation {
    param([string]$ConfigPath)
    
    Write-Status "Running Obfuscar .NET protection pipeline..."
    
    if (-not (Test-Path $ConfigPath)) {
        Write-WarningMsg "Obfuscar config not found at $ConfigPath. Skipping."
        return
    }

    try {
        # Ensure 'obfuscar.console' is available
        $obfuscar = Get-Command "obfuscar.console" -ErrorAction SilentlyContinue
        if (-not $obfuscar) {
            Write-WarningMsg "Obfuscar.GlobalTool not found. Install with: dotnet tool install --global Obfuscar.GlobalTool"
            return
        }

        & $obfuscar.Name $ConfigPath 2>&1 | Out-Null
        
        # Obfuscar outputs to dist/obfuscated/ per obfuscar.xml
        $outPath = Join-Path (Split-Path $ConfigPath) "dist\obfuscated"
        if (Test-Path $outPath) {
            Write-Status "Finalizing obfuscated .NET binaries..."
            Get-ChildItem $outPath -Filter *.dll | ForEach-Object {
                $target = Join-Path (Join-Path (Split-Path $ConfigPath) "dist") $_.Name
                Move-Item $_.FullName $target -Force
            }
            Remove-Item $outPath -Recurse -Force
            Write-Success ".NET code obfuscation complete (Strings Encrypted + Renamed)."
        }
    }
    catch {
        Write-ErrorMsg ".NET obfuscation failed: $_"
    }
}

# ═══════════════════════════════════════════════════════════════════════════
# MAIN OBFUSCATION PIPELINE
# ═══════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-RGB "╔═══════════════════════════════════════════════════════════════════════════╗" -RGB $AbyssColors.DeepPurple
Write-RGB "║         🌑 ABYSS OBFUSCATION ENGINE - Binary Hardening Pipeline 🌑        ║" -RGB $AbyssColors.NeonCyan
Write-RGB "╚═══════════════════════════════════════════════════════════════════════════╝" -RGB $AbyssColors.DeepPurple
Write-Host ""

# Special Stage: .NET Code Obfuscation (Runs before binary hardening)
if (-not $SkipDotNetObfuscation) {
    $config = Join-Path (Get-Location) "obfuscar.xml"
    Invoke-DotNetObfuscation -ConfigPath $config
}

if (-not (Test-Path $TargetPath)) {
    Write-ErrorMsg "Target path not found: $TargetPath"
    exit 1
}

# Process top-level binaries in target path
# We target .exe, .dll, and .sys files for hardening
Write-Status "Scanning for build artifacts in $(Split-Path $TargetPath -Leaf)..."
$files = Get-ChildItem -Path $TargetPath -File | Where-Object { 
    $_.Extension -match "\.(exe|dll|sys)$" -and $_.Extension -ne ".json" 
}

if (-not $files) {
    Write-WarningMsg "No supported binaries found to harden."
    exit 0
}

# Cleanup Stage: Remove sensitive build artifacts (like .deps.json)
$junkFiles = Get-ChildItem -Path (Join-Path $TargetPath "*") -Include *.deps.json, *.runtimeconfig.json -ErrorAction SilentlyContinue
if ($junkFiles) {
    Write-Status "Removing build metadata artifacts..."
    $junkFiles | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-RGB "    → Eliminated: $($_.Name)" -RGB $AbyssColors.ShadowGray
    }
}

$results = @()

foreach ($file in $files) {
    Write-Host ""
    Write-RGB "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -RGB $AbyssColors.ShadowGray
    Write-RGB "  Processing: $($file.Name)" -RGB $AbyssColors.GhostWhite
    Write-RGB "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -RGB $AbyssColors.ShadowGray
    
    $sizeBefore = $file.Length
    $entropyBefore = Get-FileEntropy -FilePath $file.FullName
    
    # Stage 1: Symbol Stripping
    Invoke-SymbolStrip -FilePath $file.FullName
    
    # Stage 2: Entropy Injection (optional)
    if (-not $SkipEntropyInjection) {
        Invoke-EntropyInjection -FilePath $file.FullName
    }
    
    # Stage 3: Header Manipulation (optional)
    if (-not $SkipHeaderManipulation) {
        Invoke-HeaderManipulation -FilePath $file.FullName
    }
    
    $sizeAfter = (Get-Item $file.FullName).Length
    $entropyAfter = Get-FileEntropy -FilePath $file.FullName
    
    $results += [PSCustomObject]@{
        File          = $file.Name
        SizeBefore    = [math]::Round($sizeBefore / 1KB, 2)
        SizeAfter     = [math]::Round($sizeAfter / 1KB, 2)
        EntropyBefore = $entropyBefore
        EntropyAfter  = $entropyAfter
    }
}

# Summary Table
Write-Host ""
Write-RGB "╔═══════════════════════════════════════════════════════════════════════════╗" -RGB $AbyssColors.DeepPurple
Write-RGB "║                        📊 OBFUSCATION SUMMARY                             ║" -RGB $AbyssColors.NeonCyan
Write-RGB "╚═══════════════════════════════════════════════════════════════════════════╝" -RGB $AbyssColors.DeepPurple
Write-Host ""

foreach ($r in $results) {
    Write-RGB "  ◆ $($r.File)" -RGB $AbyssColors.GhostWhite
    Write-RGB "    Size: $($r.SizeBefore) KB → $($r.SizeAfter) KB" -RGB $AbyssColors.ShadowGray
    Write-RGB "    Entropy: $($r.EntropyBefore) → $($r.EntropyAfter) bits/byte" -RGB $AbyssColors.ShadowGray
}

Write-Host ""
Write-Success "Obfuscation pipeline complete. $($files.Count) file(s) processed."
Write-Host ""
