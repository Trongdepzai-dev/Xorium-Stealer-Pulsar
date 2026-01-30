# PHÂN TÍCH ĐIỂM YẾU & CƠ HỘI PHÒNG THỦ: XORIUM STEALER PULSAR

> **Tài liệu dành cho Blue Team & Incident Response**  
> **Mục đích**: Phát hiện, ngăn chặn và điều tra mã độc  
> **Ngày cập nhật**: 31/01/2026

---

## 1. ĐIỂM YẾU TRONG ANTI-VM (Dễ bị bypass)

### 1.1 Các kiểm tra Anti-VM rất cơ bản

```csharp
// Code Anti-VM của malware
public static void CheckOrExit()
{
    if (ProccessorCheck()) throw new Exception();      // CPU <= 1 core
    if (CheckDebugger()) throw new Exception();        // Debugger.IsAttached
    if (CheckMemory()) throw new Exception();          // RAM < 2GB
    if (CheckDriveSpace()) throw new Exception();      // Disk < 50GB
    if (CheckUserConditions()) throw new Exception();  // Hardcoded usernames
    if (CheckCache()) throw new Exception();           // WMI query
    if (CheckFileName()) throw new Exception();        // "sandbox" in filename
    if (CheckCim()) throw new Exception();             // WMI query
}
```

### 1.2 Cách bypass từng kiểm tra

| Kiểm tra | Điểm yếu | Cách bypass đơn giản |
|----------|----------|---------------------|
| **CPU <= 1** | Chỉ check số core | VM có 2+ cores sẽ bypass |
| **Debugger.IsAttached** | Dễ hook/patch | Dùng ScyllaHide, x64dbg stealth |
| **RAM < 2GB** | Check đơn giản | Cấu hình VM có 4GB+ RAM |
| **Disk < 50GB** | Check C: drive size | Tạo VM với 100GB+ disk |
| **Hardcoded users** | Chỉ 3 usernames | Đổi tên user/machine trong VM |
| **WMI queries** | Dễ spoof | Patch WMI responses hoặc hook COM |
| **Filename check** | Chỉ check "sandbox" | Đổi tên file thực thi |

### 1.3 Hardcoded Usernames (Chỉ 3 patterns!)

```csharp
// Tất cả chỉ có 3 điều kiện:
1. username == "WDAGUtilityAccount"  // Windows Defender App Guard
2. username == "frank" && hostname.Contains("desktop")
3. username == "robert" && hostname.Contains("22h2")
```

**→ Điểm yếu nghiêm trọng**: Chỉ cần đặt tên user khác "frank"/"robert" là bypass hoàn toàn!

---

## 2. CƠ HỘI PHÁT HIỆN (Detection Opportunities)

### 2.1 Process Termination Behavior (Dễ phát hiện)

**Danh sách 65 processes bị kill:**
```csharp
string[] Targets = new string[65]
{
    "thunderbird.exe", "icedragon.exe", "cyberfox.exe", "blackhawk.exe",
    "palemoon.exe", "ghostery.exe", "msedge.exe", "opera.exe",
    "operagx.exe", "chromium.exe", "chrome.exe", "vivaldi.exe",
    "brave.exe", "steam.exe", "telegram.exe", ...
};
```

**Cơ hội phát hiện:**
- EDR có thể detect hành vi "mass process termination"
- 65 processes bị terminate đồng thời là IOC rõ ràng
- API call sequence: `OpenProcess` → `TerminateProcess` lặp lại 65 lần

**Sigma Rule:**
```yaml
title: Xorium Stealer Process Termination
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
            - '\steam.exe'
            - '\telegram.exe'
    condition: selection | count() > 10  # 10+ browser/comm apps killed
```

### 2.2 WMI Queries (Dấu vết rõ ràng)

**Các WMI query đặc trưng:**
```csharp
"Select * from Win32_CacheMemory"      // Anti-VM check
"Select * from CIM_Memory"              // Anti-VM check  
"Select * From Win32_ComputerSystem"    // RAM check
```

**Phát hiện:**
- Sysmon Event ID 19/20/21 (WMI eventing)
- Windows Event Log 5857 (WMI activity)
- EDR telemetry về WMI queries bất thường

### 2.3 File Access Patterns (High-Value Targets)

**Truy cập đồng loạt nhiều sensitive files:**

```csharp
// Browser databases
%LOCALAPPDATA%\Google\Chrome\User Data\*\Login Data
%LOCALAPPDATA%\Google\Chrome\User Data\*\Cookies
%APPDATA%\Mozilla\Firefox\Profiles\*\logins.json
%APPDATA%\Mozilla\Firefox\Profiles\*\key4.db

// Crypto wallets
%APPDATA%\Exodus\exodus.wallet\*
%APPDATA%\Electrum\wallets\*
%APPDATA%\MetaMask\IndexedDB\*

// Telegram sessions
%APPDATA%\Telegram Desktop\tdata\*

// Steam configs
%ProgramFiles(x86)%\Steam\config\*.vdf
```

**Cơ hội phát hiện:**
- EDR file monitoring: 1 process đọc 50+ sensitive files
- File Integrity Monitoring (FIM) alerts
- Canary files trong browser profiles

### 2.4 Registry Access

**Keys đọc bởi malware:**
```
HKCU\Software\Valve\Steam\SteamPath
HKCU\Software\Valve\Steam\Apps\*
HKCU\Software\Bitcoin\Bitcoin-Qt\strDataDir
HKCU\Software\Litecoin\Litecoin-Qt\strDataDir
HKCU\Software\Dash\Dash-Qt\strDataDir
HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid  // For HWID
```

---

## 3. FORENSIC ARTIFACTS (Dấu vết điều tra)

### 3.1 Files tạo ra

| File | Location | Mục đích |
|------|----------|----------|
| `IntelixWifiExport_*` | `%TEMP%\` | WiFi profile export tạm |
| `*.zip` | `%TEMP%\` | Archive chứa dữ liệu đánh cắp |
| `counter.txt` | Trong ZIP | Báo cáo thống kê |
| `Information.txt` | Trong ZIP | System info |
| `Screenshot.png` | Trong ZIP | Desktop capture |

### 3.2 Process Artifacts

**Command line:**
```
netsh wlan export profile key=clear folder="%TEMP%\IntelixWifiExport_[GUID]"
```

**Parent-Child Process Tree:**
- Pulsar RAT (nếu có) → Stealer.Client.dll loaded
- Hoặc: Unknown process → .NET Assembly load

### 3.3 Network Artifacts

**Outbound connections:**
- `http://icanhazip.com` (IP public check)
- `https://discord.com/api/webhooks/*` (Exfiltration)
- `https://api.telegram.org/bot*/sendDocument` (Exfiltration)

**User-Agent:**
- WebClient default (không custom UA - điểm yếu!)

### 3.4 Memory Artifacts

**Strings trong memory:**
```
"Xorium"
"InteliX by dead artis"
"coded by @aesxor"
"Stealerv37"
"pulsar_cl_conf"
```

**Loaded Assemblies:**
- `Stealerv37.dll` (Stealer.Client)
- `Costura` assemblies (embedded dependencies)

---

## 4. YARA RULES (Phát hiện tĩnh)

### 4.1 Rule 1: Xorium Stealer Strings

```yara
rule Xorium_Stealer_Strings
{
    meta:
        description = "Detects Xorium Stealer by unique strings"
        author = "Malware Analyst"
        date = "2026-01-31"
        hash = ""
    
    strings:
        $str1 = "InteliX by dead artis" ascii wide
        $str2 = "Xorium" ascii wide
        $str3 = "coded by @aesxor" ascii wide
        $str4 = "Stealerv37" ascii wide
        $str5 = "IntelixWifiExport" ascii wide
        
        $anti1 = "WDAGUtilityAccount" ascii wide
        $anti2 = "frank" ascii wide
        $anti3 = "robert" ascii wide
        
        $proc1 = "thunderbird.exe" ascii wide
        $proc2 = "chrome.exe" ascii wide
        $proc3 = "telegram.exe" ascii wide
    
    condition:
        uint16(0) == 0x5A4D and  // MZ header
        (2 of ($str*) or 
         (1 of ($str*) and 2 of ($anti*)) or
         (1 of ($str*) and 3 of ($proc*)))
}
```

### 4.2 Rule 2: Xorium Process Killer

```yara
rule Xorium_Process_Killer
{
    meta:
        description = "Detects Xorium process termination list"
        author = "Malware Analyst"
        date = "2026-01-31"
    
    strings:
        $proc_list = "thunderbird.exe" ascii wide
        $p1 = "icedragon.exe" ascii wide
        $p2 = "cyberfox.exe" ascii wide
        $p3 = "palemoon.exe" ascii wide
        $p4 = "ghostery.exe" ascii wide
        $p5 = "seamonkey.exe" ascii wide
        $p6 = "slimbrowser.exe" ascii wide
        $p7 = "vivaldi.exe" ascii wide
        $p8 = "brave.exe" ascii wide
        $p9 = "cryptotab.exe" ascii wide
        $p10 = "telegram.exe" ascii wide
    
    condition:
        uint16(0) == 0x5A4D and
        $proc_list and
        8 of ($p*)
}
```

### 4.3 Rule 3: Xorium Crypto Wallets Targeting

```yara
rule Xorium_Crypto_Targeting
{
    meta:
        description = "Detects Xorium crypto wallet targeting"
        author = "Malware Analyst"
        date = "2026-01-31"
    
    strings:
        $wallet1 = "Exodus" ascii wide
        $wallet2 = "Electrum" ascii wide
        $wallet3 = "AtomicWallet" ascii wide
        $wallet4 = "MetaMask" ascii wide
        $wallet5 = "Phantom" ascii wide
        $wallet6 = "Ledger Live" ascii wide
        $wallet7 = "TrezorSuite" ascii wide
        $wallet8 = "TrustWallet" ascii wide
        
        $path1 = "exodus.wallet" ascii wide
        $path2 = "IndexedDB" ascii wide
        $path3 = "key_datas" ascii wide
    
    condition:
        uint16(0) == 0x5A4D and
        5 of ($wallet*) and
        2 of ($path*)
}
```

---

## 5. SIGMA RULES (Phát hiện hành vi)

### 5.1 Mass Process Termination

```yaml
title: Xorium Stealer - Mass Browser Process Termination
id: xxxx-xxxx-xxxx-xxxx
status: experimental
description: Detects mass termination of browser and communication processes indicative of Xorium Stealer
author: Malware Analyst
date: 2026/01/31
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
            - '\brave.exe'
            - '\vivaldi.exe'
            - '\thunderbird.exe'
            - '\telegram.exe'
            - '\steam.exe'
            - '\discord.exe'
    timeframe: 30s
    condition: selection | count() > 5
falsepositives:
    - Legitimate system maintenance (rare)
level: high
```

### 5.2 WiFi Profile Export

```yaml
title: Xorium Stealer - WiFi Password Export via Netsh
id: xxxx-xxxx-xxxx-xxxx
status: experimental
description: Detects WiFi profile export with clear key - characteristic of Xorium Stealer
author: Malware Analyst
date: 2026/01/31
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
falsepositives:
    - Legitimate IT administration (rare)
level: high
```

### 5.3 Sensitive File Access

```yaml
title: Xorium Stealer - Access to Multiple Sensitive Files
id: xxxx-xxxx-xxxx-xxxx
status: experimental
description: Detects process accessing multiple browser and wallet files
author: Malware Analyst
date: 2026/01/31
logsource:
    category: file_access
    product: windows
detection:
    selection:
        TargetFilename|contains:
            - 'Login Data'
            - 'Cookies'
            - 'logins.json'
            - 'key4.db'
            - 'exodus.wallet'
            - 'tdata'
            - 'ssfn'
    timeframe: 60s
    condition: selection | count() > 10
falsepositives:
    - Backup software
    - Antivirus scans
level: medium
```

---

## 6. BIỆN PHÁP NGĂN CHẶN (Preventive Controls)

### 6.1 Application Control (AppLocker/WDAC)

**Block rules:**
```powershell
# Block known malware filenames
New-AppLockerPolicyRule -Path "%TEMP%\Intelix*" -Action Deny
New-AppLockerPolicyRule -Path "%TEMP%\*Xorium*" -Action Deny

# Block unsigned .NET assemblies in temp
Set-AppLockerPolicy -RuleType Path -Path "%TEMP%\*.dll" -Action Deny
```

### 6.2 Windows Defender ASR Rules

**Enable ASR rules:**
```powershell
# Block process creations originating from PSExec and WMI commands
Set-MpPreference -AttackSurfaceReductionRules_Ids 75668c1f-73b5-4cf0-bb93-3ecf5cb7cc84 -AttackSurfaceReductionRules_Actions Enabled

# Block credential stealing from the Windows local security authority subsystem
Set-MpPreference -AttackSurfaceReductionRules_Ids 9e6c4e1f-7d60-472f-ba1a-a39ef669e4b2 -AttackSurfaceReductionRules_Actions Enabled

# Block process termination (if supported by EDR)
```

### 6.3 Browser Hardening

**Chrome/Edge Group Policy:**
```
Computer Configuration > Administrative Templates > Google Chrome
- Enable "Force users to save passwords to the password manager" = Disabled
- Enable "Password manager" = Disabled
- Enable "AutoFill" = Disabled
```

**Firefox:**
```javascript
// policies.json
{
  "policies": {
    "PasswordManagerEnabled": false,
    "AutofillCreditCardEnabled": false,
    "AutofillAddressEnabled": false
  }
}
```

### 6.4 Crypto Wallet Protection

**File System Permissions:**
```powershell
# Restrict access to wallet directories
icacls "%APPDATA%\Exodus" /deny *S-1-1-0:(OI)(CI)F
icacls "%APPDATA%\Electrum" /deny *S-1-1-0:(OI)(CI)F
```

**Monitor wallet file access:**
```powershell
# Audit wallet file access
auditpol /set /subcategory:"File System" /success:enable /failure:enable
```

---

## 7. PHÁT HIỆN BẰNG EDR/XDR

### 7.1 Behavioral Indicators

| Behavior | Detection Method |
|----------|-----------------|
| Mass process termination | EDR process event correlation |
| WMI queries for anti-VM | Sysmon Event ID 19/20 |
| Browser DB file access | EDR file monitoring |
| netsh wlan export | Command line monitoring |
| Discord/Telegram API calls | Network monitoring |
| Clipboard access | API hooking |
| Screenshot capture | Graphics API monitoring |

### 7.2 EDR Query Examples

**CrowdStrike Falcon:**
```
event_simpleName=ProcessRollup2 
| search (CommandLine="*netsh*wlan*export*" OR CommandLine="*key=clear*")
| stats count by ComputerName, UserName, CommandLine
```

**Microsoft Defender for Endpoint:**
```kusto
DeviceProcessEvents
| where CommandLine contains "netsh" and CommandLine contains "wlan" and CommandLine contains "export"
| summarize count() by DeviceName, AccountName, CommandLine
```

**SentinelOne:**
```
ProcessEvent 
| where TgtProcCmdLine contains "netsh wlan export profile key=clear"
| project EndpointName, UserName, TgtProcCmdLine, EventTime
```

---

## 8. INCIDENT RESPONSE PLAYBOOK

### 8.1 Detection → Containment

**Step 1: Xác nhận compromise**
```powershell
# Check for Intelix artifacts
Get-ChildItem -Path $env:TEMP -Filter "Intelix*" -Recurse
Get-ChildItem -Path $env:TEMP -Filter "*Xorium*" -Recurse

# Check process termination logs
Get-WinEvent -FilterHashtable @{LogName='Security'; ID=4689} | 
    Where-Object {$_.Message -match "chrome.exe|opera.exe|telegram.exe"}
```

**Step 2: Cô lập endpoint**
- Ngắt kết nối network
- Khóa tài khoản user bị ảnh hưởng
- Preserve memory dump

**Step 3: Thu thập evidence**
```powershell
# Collect ZIP archive if exists
Get-ChildItem -Path $env:TEMP -Filter "*.zip" | Where-Object {$_.Length -gt 1MB}

# Memory dump
.\procdump.exe -ma <PID> xorium_memory.dmp
```

### 8.2 Recovery Actions

**Immediate:**
1. Đổi tất cả password từ máy sạch
2. Revoke tất cả session tokens (Discord, Telegram, Steam...)
3. Transfer crypto assets sang ví mới
4. Kiểm tra 2FA/MFA settings

**Long-term:**
1. Deploy YARA rules to all endpoints
2. Enable ASR rules
3. Implement browser password manager disable
4. User training

---

## 9. TÓM TẮT IOCs

### 9.1 File Hashes (Cần cập nhật)

```
Stealer.Client.dll: [Cần hash]
Stealer.Server.dll: [Cần hash]
```

### 9.2 Network IOCs

```
http://icanhazip.com
https://discord.com/api/webhooks/*
https://api.telegram.org/bot*/sendDocument
```

### 9.3 Host IOCs

```
%TEMP%\IntelixWifiExport_*
%TEMP%\*Xorium*
%APPDATA%\pulsar_cl_conf.bin
```

### 9.4 Registry IOCs

```
HKCU\Software\Pulsar
```

---

## 10. KẾT LUẬN

### Điểm yếu chính của malware:

1. **Anti-VM yếu** - Chỉ 3 hardcoded usernames, dễ bypass
2. **Process termination rầm rộ** - 65 processes, dễ detect
3. **WMI queries đặc trưng** - Dấu vết rõ ràng trong logs
4. **Không mã hóa strings** - Dễ phát hiện bằng YARA
5. **File artifacts rõ ràng** - "Intelix", "Xorium" trong filenames
6. **Network IOCs cố định** - Discord/Telegram APIs dễ monitor
7. **Không persistence mạnh** - Không registry run keys, chỉ tạm thời

### Khuyến nghị phòng thủ:

1. Deploy YARA rules để scan static
2. Enable Sigma rules cho behavioral detection
3. Block Discord/Telegram webhooks tại firewall (nếu không dùng)
4. Implement browser hardening GPOs
5. Monitor WMI activity với Sysmon
6. Use hardware wallets cho crypto
7. Regular user training về phishing

---

**END OF DEFENSIVE ANALYSIS**

*Tài liệu này chỉ dùng cho mục đích phòng thủ và điều tra sự cố.*
