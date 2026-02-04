# 💎 XORIUM STEALER PULSAR [GOD EDITION] 💎
### *The Last Stealer You Will Ever Need.*
[👉 Đọc bản tiếng Việt tại đây (Vietnamese Version)](file:///d:/Xorium_Stealer_Pulsar/Xorium%20Stealer%20Pulsar/README_VN.md)

[![Rust](https://img.shields.io/badge/Core-Rust_Shadow-orange?style=for-the-badge&logo=rust)]()
[![C#](https://img.shields.io/badge/Loader-C%23_GodPower-blue?style=for-the-badge&logo=csharp)]()
[![Status](https://img.shields.io/badge/Status-UNDETECTED-brightgreen?style=for-the-badge)]()
[![Privilege](https://img.shields.io/badge/Privilege-NT_AUTHORITY%5CSYSTEM-red?style=for-the-badge)]()

---

## 💀 WHY CHOOSE XORIUM?

You are tired of paying for "FUD" crypters that get detected in 24 hours. You are tired of Webhooks getting banned.
**Xorium Stealer Pulsar** is not just a stealer. It is a **Military-Grade Cyber Weapon**.

We combined the raw power of **C# .NET** with the invisibility of **Rust Assembly**.
The result? A malware that doesn't just steal—it **DOMINATES**.

---

## ⚡ KEY SELLING POINTS (The "God" Features)

### 1. 👻 PROJECT SHADOW (True Invisibility)
Most stealers are caught by heuristics. Xorium uses a **Custom Rust Stealth Core** (`shadow_core.dll`).
- **PEB Unlinking**: The DLL removes itself from the Process Environment Block immediately upon load. It effectively "vanishes" from the module list.
- **AMSI Patching**: Locates and neutralizes `AmsiScanBuffer` in memory. Bypasses in-memory scanning.
- **ETW Blindness**: Patches `EtwEventWrite` in `ntdll.dll` to return success without logging. Blinds behavioral analysis tools.

### 2. 👑 GOD POWER (Instant SYSTEM Access)
Why run as a user when you can run as **GOD**?
- **Integrated GodPotato**: We have integrated the legendary `GodPotato` exploit.
- **Auto-Escalate**: User -> `NT AUTHORITY\SYSTEM` in milliseconds.
- **Command Dominance**: Execute any command with the highest privileges available on Windows.

### 3. 🌑 VOID WALKER (Ring 0 Kernel Rootkit)
The ultimate weapon for absolute OS control. Xorium now integrates a **Rust-based Kernel Rootkit** with **Silent IOCTL Interaction**.
- **Ghost Communication**: No CLI calls, no noisy `.exe` wrappers. Communicates directly with Ring 0 via `DeviceIoControl`.
- **DKOM (Direct Kernel Object Manipulation)**: Manipulates `EPROCESS` structures to hide any process from Task Manager, ProcExp, and the kernel itself.
- **Kernel Keylogger**: Maps `gafAsyncKeyState` directly to user-space for zero-hook keystroke capture.
- **EDR Blinding**: Disables **ETWTI** (Event Tracing for Windows Threat Intelligence) and **DSE** (Driver Signature Enforcement).

---

## 🛠️ COMMAND SYSTEMS

Xorium Pulsar supports a wide range of remote commands. Here are some highlight commands:

### 🎮 General Commands
- `collect`: Scans 150+ targets (Browsers, Wallets, VPNs, etc.) and zips them.

### 🌑 Kernel Commands (Ring 0 - SILENT)
- `kernel_hide`: Hides the target PID from the entire OS via IOCTL.
- `kernel_elevate`: Forces the target PID to SYSTEM status via Token Stealing.
- `kernel_protect`: Makes the target PID unkillable.
- `kernel_keylog`: Activates the kernel-level stealth keylogger.
- `kernel_blind`: Disables ETWTI and DSE silencing security monitors.

---

## 💰 FEATURE MATRIX

| Feature | ❌ Garbage Stealer | ✅ XORIUM PULSAR |
| :--- | :---: | :---: |
| **Language** | Python/C# (Detected) | **Rust + C# Hybrid** |
| **Stealth** | None | **Ring 0 Kernel + Ring 3 Evasion** |
| **Concealment** | Task Manager Visible | **Fully Hidden via DKOM** |
| **Privilege** | User | **NT AUTHORITY\SYSTEM** |
| **C2** | Webhook Only | **GitHub / Discord / Tele** |

---

## 🚀 HOW TO DEPLOY (Start Your Empire)

### Step 1: Forge the Kernel & Client (Rust)
1.  Navigate to `shadow-main`:
    ```cmd
    cd shadow-main
    ```
2.  Compile the Kernel Driver and Client:
    ```cmd
    cargo build --release
    ```
3.  This generates:
    - `shadow.sys`: The Kernel Driver.
    - `shadow-client.exe`: The Command Bridge used by Xorium.

### Step 2: Build the Loader (C#)
1.  Open Project in Visual Studio.
2.  Ensure `shadow-client.exe` is in the output directory.
3.  Build the project. The loader will automatically interface with the Rust bridge.

---

## 🎮 REMOTE KERNEL COMMANDS
Xorium can now execute these commands via its C2:
- `kernel_hide` [PID]: Vanish from all system views.
- `kernel_elevate` [PID]: Token stealing for instant SYSTEM.
- `kernel_protect` [PID]: Make a process unkillable even by Admin.
- `kernel_hide_driver` [NAME]: Hide the rootkit driver itself.

---

## 🔮 FUTURE WARFARE (Roadmap)
- [x] **Ring 0 Rootkit**: Absolute OS control via Rust Kernel Driver.
- [ ] **HVCI Bypass**: Hypervisor-protected Code Integrity bypass.
- [ ] **UEFI Bootkit**: Persistence that survives OS reinstallation.

---

## ⚠️ DISCLAIMER
*This tool is a Proof of Concept for Red Team engagements. The developer is not responsible for damage caused by misuse. But if you use it... act like a God.*

---

### [ ⭐ STAR THIS REPO TO SUPPORT DEVELOPMENT ]
**Join the Elite. Use Xorium.**
